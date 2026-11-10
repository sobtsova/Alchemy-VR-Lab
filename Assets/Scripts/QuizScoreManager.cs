using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuizScoreManager : MonoBehaviour
{
    [Header("Panel Steps (0 to 3)")]
    public GameObject[] steps; // Масив усіх 4 кроків

    [Header("Scoring & Reward")]
    public int totalScore = 0;
    public TMP_Text rewardTextMesh; // Текст результату
    public GameObject recipesBookPage; // Права сторінка книги рецептів (об'єкт Recipes)

    [Header("Step 2 References")]
    public Slider step2Slider;
    public Toggle step2Toggle;
    public TMP_Text sliderValueText;

    [Header("Step 3 References")]
    public TMP_InputField step3InputField;
    public GameObject vrKeyboardObject;

    private int currentStep = 0;

    void Start()
    {
        UpdateSteps();

        if (rewardTextMesh != null)
            rewardTextMesh.gameObject.SetActive(false);

        if (recipesBookPage != null)
            recipesBookPage.SetActive(false);

        // Ховаємо клавіатуру на старті
        if (vrKeyboardObject != null)
            vrKeyboardObject.gameObject.SetActive(false);

        if (step3InputField != null)
        {
            step3InputField.onSelect.AddListener(ShowKeyboard);
        }
    }

    private void ShowKeyboard(string text)
{
    if (vrKeyboardObject != null)
    {
        vrKeyboardObject.SetActive(true);
    }
}

    void Update()
    {
        // Оновлюємо текст значення слайдера в реальному часі
        if (step2Slider != null && sliderValueText != null)
        {
            sliderValueText.text = Mathf.Round(step2Slider.value).ToString();
        }
    }

    // Метод автоматичного відкриття віртуальної клавіатури при кліку на поле вводу
    private void OnInputSelected(string text)
    {
        TouchScreenKeyboard.Open(text, TouchScreenKeyboardType.Default, false, false, false, false);
    }

    // Загальний метод переходу вперед із нарахуванням бала
    public void NextStepWithScore(bool addPoint)
    {
        if (addPoint)
        {
            totalScore++;
        }

        if (currentStep < steps.Length - 1)
        {
            currentStep++;
            UpdateSteps();
        }
    }

    // Звичайний перехід (без бала)
    public void OnNextButtonClicked()
    {
        if (currentStep < steps.Length - 1)
        {
            currentStep++;
            UpdateSteps();
        }
    }

    // Кнопка Назад
    public void OnBackButtonClicked()
    {
        if (currentStep > 0)
        {
            currentStep--;
            UpdateSteps();
        }
    }

    // Перевірка Кроку 2 (Слайдер біля 75 + Тоггл)
    public void CheckStep2AndProceed()
    {
        bool isSliderCorrect = step2Slider != null && Mathf.Abs(step2Slider.value - 75f) < 5f;
        bool isToggleCorrect = step2Toggle != null && step2Toggle.isOn;

        bool earnedPoint = isSliderCorrect && isToggleCorrect;
        NextStepWithScore(earnedPoint);
    }

    // Перевірка Кроку 3 (Текстове поле "acid")
    public void CheckStep3InputAndProceed()
    {
        bool isInputCorrect = step3InputField != null && step3InputField.text.Trim().ToLower() == "acid";
        NextStepWithScore(isInputCorrect);
    }

    // Фінальна перевірка на Кроці 4
    public void SubmitFinalAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            totalScore++; // Останній бал за правильну пляшку
        }

        // Показуємо загальний текст результату
        if (rewardTextMesh != null)
        {
            rewardTextMesh.gameObject.SetActive(true);
            rewardTextMesh.text = $"Ви набрали балів: {totalScore} з 3!";
        }

        // Головна умова: якщо набрано рівно 3 з 3 балів — розблоковуємо книгу рецептів на правій сторінці!
        if (totalScore == 3)
        {
            if (recipesBookPage != null)
            {
                recipesBookPage.SetActive(true); // Появляється права сторінка з рецептами
            }
        }
        else
        {
            if (recipesBookPage != null)
            {
                recipesBookPage.SetActive(false); // Якщо менше 3 балів — книга не з'являється
            }
        }
    }

    void UpdateSteps()
    {
        for (int i = 0; i < steps.Length; i++)
        {
            if (steps[i] != null)
            {
                steps[i].SetActive(i == currentStep);
            }
        }
    }
}