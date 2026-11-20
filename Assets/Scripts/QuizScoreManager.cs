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
    public GameObject retryButtonObject; // Кнопка "Спробувати ще раз", коли балів < 3

    [Header("Step 2 References")]
    public Slider step2Slider;
    public Toggle step2Toggle;
    public TMP_Text sliderValueText;

    [Header("Step 3 References")]
    public TMP_InputField step3InputField;
    public GameObject vrKeyboardObject;

    [Header("Telemetry (Lab 5 POST)")]
    public WizardTelemetryManager telemetryManager;

    private int currentStep = 0;

    void Start()
    {
        if (step3InputField != null)
        {
            step3InputField.onSelect.AddListener(ShowKeyboard);
        }

        if (telemetryManager == null)
        {
            telemetryManager = FindFirstObjectByType<WizardTelemetryManager>();
        }

        ResetWizardState(startTelemetrySession: true);
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

        if (!earnedPoint && telemetryManager != null)
        {
            float val = step2Slider != null ? Mathf.Round(step2Slider.value) : -1f;
            bool tog = step2Toggle != null && step2Toggle.isOn;
            telemetryManager.RecordWizardError($"Крок 2: невірні параметри (слайдер={val}, тоггл={tog})");
        }

        NextStepWithScore(earnedPoint);
    }

    // Перевірка Кроку 3 (Текстове поле "acid")
    public void CheckStep3InputAndProceed()
    {
        string rawInput = step3InputField != null ? step3InputField.text.Trim() : "";
        bool isInputCorrect = rawInput.ToLower() == "acid";

        if (!isInputCorrect && telemetryManager != null)
        {
            telemetryManager.RecordWizardError($"Крок 3: очікувалось 'acid', введено '{rawInput}'");
        }

        NextStepWithScore(isInputCorrect);
    }

    // Фінальна перевірка на Кроці 4
    public void SubmitFinalAnswer(bool isCorrect)
    {
        if (isCorrect)
        {
            totalScore++; // Останній бал за правильну пляшку
        }
        else if (telemetryManager != null)
        {
            telemetryManager.RecordWizardError("Крок 4: обрано невірну пляшку зілля");
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

            if (retryButtonObject != null)
            {
                retryButtonObject.SetActive(false); // При повному успіху кнопка повтору не потрібна
            }
        }
        else
        {
            if (recipesBookPage != null)
            {
                recipesBookPage.SetActive(false); // Якщо менше 3 балів — книга не з'являється
            }

            if (retryButtonObject != null)
            {
                retryButtonObject.SetActive(true); // Показуємо кнопку "Пройти заново" при 0, 1 або 2 балах
            }
        }

        // Після завершення візарду формуємо JSON і відправляємо POST-запит
        if (telemetryManager != null)
        {
            telemetryManager.CompleteWizardAndSendReport();
        }
    }

    // Викликається при натисканні кнопки "Спробувати ще раз"
    public void RestartWizard()
    {
        ResetWizardState(startTelemetrySession: true);
    }

    private void ResetWizardState(bool startTelemetrySession)
    {
        currentStep = 0;
        totalScore = 0;

        if (rewardTextMesh != null)
        {
            rewardTextMesh.text = "";
            rewardTextMesh.gameObject.SetActive(false);
        }

        if (recipesBookPage != null)
            recipesBookPage.SetActive(false);

        if (retryButtonObject != null)
            retryButtonObject.SetActive(false);

        if (vrKeyboardObject != null)
            vrKeyboardObject.SetActive(false);

        if (step2Slider != null)
            step2Slider.value = step2Slider.minValue;

        if (step2Toggle != null)
            step2Toggle.isOn = false;

        if (step3InputField != null)
            step3InputField.text = "";

        UpdateSteps();

        if (startTelemetrySession && telemetryManager != null)
        {
            telemetryManager.BeginWizardSession();
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