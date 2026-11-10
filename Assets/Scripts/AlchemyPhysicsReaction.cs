using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemyPhysicsReaction : MonoBehaviour
{
    [Header("Рідина казанка")]
    public Renderer liquidRenderer;

    [Header("Ефекти")]
    public ParticleSystem successSmoke;  // Постійний магічний дим до скидання
    public ParticleSystem explosionVFX;  // Спалах або вибух при невдачі

    [Header("Глибокі темні кольори інгредієнтів")]
    public Color greenColor  = new Color(0.04f, 0.40f, 0.08f, 1.0f); // Глибокий темно-зелений
    public Color purpleColor = new Color(0.38f, 0.02f, 0.48f, 1.0f); // Глибокий оксамитовий фіолетовий
    public Color redColor    = new Color(0.65f, 0.01f, 0.01f, 1.0f); // Чистий темно-червоний (без жовтизни)
    public Color blueColor   = new Color(0.02f, 0.12f, 0.58f, 1.0f); // Глибокий темно-синій (вода)
    public Color acidColor   = new Color(0.62f, 0.50f, 0.02f, 1.0f); // Гірчично-темна кислота

    [Header("Фінальні кольори готових зіллів")]
    public Color glowPotionColor = new Color(0.08f, 0.55f, 0.20f, 1.0f); // Рецепт 1: Темне світне зілля
    public Color deepBlueColor   = new Color(0.00f, 0.06f, 0.42f, 1.0f); // Рецепт 2: Густий нічний синій
    public Color rustColor       = new Color(0.52f, 0.22f, 0.01f, 1.0f); // Рецепт 3: Темна іржава суміш
    public Color failColor       = new Color(0.02f, 0.02f, 0.02f, 1.0f); // Невдача: чорна сажа / жижа
    public Color defaultWaterColor = new Color(0.12f, 0.30f, 0.38f, 0.85f); // Початкова темна вода

    private List<string> addedIngredients = new List<string>();
    private bool reactionLocked = false;
    private bool isProcessingBottle = false;

    private static Dictionary<string, Vector3> initialPositions = new Dictionary<string, Vector3>();
    private static Dictionary<string, Quaternion> initialRotations = new Dictionary<string, Quaternion>();

    private Material liquidMat;
    private Color currentColor;
    private Coroutine colorCoroutine; // Для контролю зміни кольорів без конфліктів

    private void Awake()
    {
        string[] bottleNames = { "PotionBottle_Purple", "PotionBottle_Blue", "PotionBottle_Green", "PotionBottle_Red", "PotionBottle_Acid" };
        foreach (string bName in bottleNames)
        {
            GameObject bObj = GameObject.Find(bName);
            if (bObj != null && !initialPositions.ContainsKey(bName))
            {
                initialPositions[bName] = bObj.transform.position;
                initialRotations[bName] = bObj.transform.rotation;
            }
        }
    }

    private void Start()
    {
        if (liquidRenderer != null)
        {
            liquidMat = liquidRenderer.material;
            // Глушимо старий зелений Emission матеріалу M_Poison, щоб не збивав кольори
            if (liquidMat.HasProperty("_EmissionColor"))
            {
                liquidMat.SetColor("_EmissionColor", Color.black);
            }
            currentColor = defaultWaterColor;
            ApplyColorToMaterial(currentColor);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (reactionLocked || isProcessingBottle) return;

        GameObject bottleRoot = FindBottleRoot(other.gameObject);
        if (bottleRoot == null) return;

        string ingredient = IdentifyBottleType(bottleRoot.name);
        if (string.IsNullOrEmpty(ingredient)) return;

        isProcessingBottle = true;
        addedIngredients.Add(ingredient);
        Debug.Log($"[Казан]: Додано {ingredient}! Всього у казані: {addedIngredients.Count}");

        StartCoroutine(RespawnBottleRoutine(bottleRoot));

        // Перевіряємо, чи це потенційний Рецепт 3 (потрібно 4 інгредієнти: Purple + Acid + 2 Green)
        int greenCount  = CountIngredient("Green");
        int purpleCount = CountIngredient("Purple");
        int acidCount   = CountIngredient("Acid");

        bool isPotentiallyRecipe3 = (purpleCount == 1 && acidCount == 1 && greenCount == 1);

        // Якщо закинуто 3 пляшки, і це саме початок Рецепту 3 — чекаємо 4-ту (другу зелену)
        if (addedIngredients.Count == 3 && isPotentiallyRecipe3)
        {
            Color targetColor = CalculateMixedColor();
            ChangeColorSmooth(targetColor, 0.35f);
            return;
        }

        // Якщо набралося 3 звичайних або вже 4 інгредієнти — оцінюємо рецепт
        if (addedIngredients.Count >= 3)
        {
            EvaluateFinalRecipe();
        }
        else
        {
            Color targetColor = CalculateMixedColor();
            ChangeColorSmooth(targetColor, 0.35f);
        }
    }

    // Змішування спирається виключно на список закинутих банок
    private Color CalculateMixedColor()
    {
        if (addedIngredients.Count == 0) return defaultWaterColor;

        if (addedIngredients.Count == 1)
        {
            return GetColorByIngredient(addedIngredients[0]);
        }

        Color blend = GetColorByIngredient(addedIngredients[0]);
        for (int i = 1; i < addedIngredients.Count; i++)
        {
            Color nextColor = GetColorByIngredient(addedIngredients[i]);
            blend = Color.Lerp(blend, nextColor, 0.5f);
        }
        return blend;
    }

    private Color GetColorByIngredient(string ingredient)
    {
        switch (ingredient)
        {
            case "Green":  return greenColor;
            case "Purple": return purpleColor;
            case "Red":    return redColor;
            case "Water":  return blueColor;
            case "Acid":   return acidColor;
            default:       return defaultWaterColor;
        }
    }

    private void EvaluateFinalRecipe()
    {
        reactionLocked = true;

        int greenCount  = CountIngredient("Green");
        int purpleCount = CountIngredient("Purple");
        int redCount    = CountIngredient("Red");
        int acidCount   = CountIngredient("Acid");
        int waterCount  = CountIngredient("Water");

        // РЕЦЕПТ 1 (3 шт): Зелена + Фіолетова + Кислота
        if (greenCount == 1 && purpleCount == 1 && acidCount == 1 && addedIngredients.Count == 3)
        {
            Debug.Log("[Успіх]: Сварено світне зілля незвичайної сили!");
            StartSuccessState(glowPotionColor);
        }
        // РЕЦЕПТ 2 (3 шт): Червона + Зелена + Синя вода
        else if (redCount == 1 && greenCount == 1 && waterCount == 1 && addedIngredients.Count == 3)
        {
            Debug.Log("[Успіх]: Отримано темний синій стабілізатор!");
            StartSuccessState(deepBlueColor);
        }
        // РЕЦЕПТ 3 (4 шт): Фіолетова + Кислота + 2 Зелені
        else if (purpleCount == 1 && acidCount == 1 && greenCount == 2 && addedIngredients.Count == 4)
        {
            Debug.Log("[Успіх]: Сварено розчинник залишків та іржі!");
            StartSuccessState(rustColor);
        }
        // Невдала комбінація -> ВИБУХ
        else
        {
            Debug.LogWarning("[Помилка]: Неправильні пропорції або суміш! Вибух казанка!");
            StartFailState();
        }
    }

    private void StartSuccessState(Color finalColor)
    {
        ChangeColorSmooth(finalColor, 0.4f);

        if (successSmoke != null)
        {
            var mainModule = successSmoke.main;
            mainModule.startColor = new ParticleSystem.MinMaxGradient(finalColor);
            successSmoke.gameObject.SetActive(true);
            successSmoke.Play();
        }
    }

    private void StartFailState()
    {
        ChangeColorSmooth(failColor, 0.2f);

        if (explosionVFX != null)
        {
            explosionVFX.gameObject.SetActive(true);
            explosionVFX.Stop();
            explosionVFX.Play();
        }

        if (successSmoke != null)
        {
            var mainModule = successSmoke.main;
            mainModule.startColor = new ParticleSystem.MinMaxGradient(new Color(0.06f, 0.06f, 0.06f, 0.95f));
            successSmoke.gameObject.SetActive(true);
            successSmoke.Play();
        }
    }

    private void ChangeColorSmooth(Color targetColor, float duration)
    {
        if (colorCoroutine != null)
        {
            StopCoroutine(colorCoroutine);
        }
        colorCoroutine = StartCoroutine(SmoothColorChange(targetColor, duration));
    }

    private IEnumerator SmoothColorChange(Color targetColor, float duration)
    {
        if (liquidMat == null) yield break;

        Color startColor = currentColor;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentColor = Color.Lerp(startColor, targetColor, elapsed / duration);
            ApplyColorToMaterial(currentColor);
            yield return null;
        }

        currentColor = targetColor;
        ApplyColorToMaterial(currentColor);
        colorCoroutine = null;
    }

    private void ApplyColorToMaterial(Color c)
    {
        if (liquidMat == null) return;

        liquidMat.color = c;
        if (liquidMat.HasProperty("_BaseColor"))
        {
            liquidMat.SetColor("_BaseColor", c);
        }
    }

    private IEnumerator RespawnBottleRoutine(GameObject bottle)
    {
        yield return new WaitForSeconds(0.1f);
        ReturnBottleToShelf(bottle);
        yield return new WaitForSeconds(0.2f);
        isProcessingBottle = false;
    }

    public static void ReturnBottleToShelf(GameObject bottle)
    {
        if (bottle == null) return;

        Rigidbody rb = bottle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (initialPositions.ContainsKey(bottle.name))
        {
            bottle.transform.position = initialPositions[bottle.name];
            bottle.transform.rotation = initialRotations[bottle.name];
        }
    }

    public static GameObject FindBottleRoot(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.name.StartsWith("PotionBottle_"))
            {
                return current.gameObject;
            }
            current = current.parent;
        }
        return null;
    }

    private string IdentifyBottleType(string rootName)
    {
        if (rootName.Contains("Acid"))   return "Acid";
        if (rootName.Contains("Blue") || rootName.Contains("Water")) return "Water";
        if (rootName.Contains("Purple")) return "Purple";
        if (rootName.Contains("Green"))  return "Green";
        if (rootName.Contains("Red"))    return "Red";
        return null;
    }

    private int CountIngredient(string name)
    {
        int count = 0;
        foreach (string item in addedIngredients)
        {
            if (item == name) count++;
        }
        return count;
    }

    public void ResetCauldron()
    {
        addedIngredients.Clear();
        isProcessingBottle = false;
        reactionLocked = false;

        ChangeColorSmooth(defaultWaterColor, 0.4f);

        if (successSmoke != null)
        {
            successSmoke.Stop();
        }

        if (explosionVFX != null)
        {
            explosionVFX.Stop();
        }

        Debug.Log("[Казан]: Казан повністю очищено та скинуто!");
    }
}