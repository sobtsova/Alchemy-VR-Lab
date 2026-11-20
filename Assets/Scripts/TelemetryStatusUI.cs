using TMPro;
using UnityEngine;

public class TelemetryStatusUI : MonoBehaviour
{
    [Header("Посилання")]
    [SerializeField] private WizardTelemetryManager telemetryManager;
    [SerializeField] private TMP_Text statusText;

    [Header("Кольори індикації")]
    [SerializeField] private Color loadingColor = new Color(0.95f, 0.78f, 0.30f); // тепле золото при завантаженні
    [SerializeField] private Color successColor = new Color(0.45f, 0.90f, 0.65f); // м'ятний при успіху
    [SerializeField] private Color errorColor = new Color(0.98f, 0.42f, 0.42f);   // червоний при помилці з'єднання
    [SerializeField] private Color idleColor = new Color(0.75f, 0.72f, 0.85f);    // нейтральний

    private void OnEnable()
    {
        if (telemetryManager != null)
        {
            telemetryManager.OnTelemetryStatusChanged += HandleStatusChanged;
        }
    }

    private void OnDisable()
    {
        if (telemetryManager != null)
        {
            telemetryManager.OnTelemetryStatusChanged -= HandleStatusChanged;
        }
    }

    private void HandleStatusChanged(string message)
    {
        if (statusText == null) return;

        statusText.text = message;

        string lower = message.ToLowerInvariant();
        if (lower.Contains("помилка") || lower.Contains("з'єднання"))
        {
            statusText.color = errorColor;
        }
        else if (lower.Contains("успішно") || lower.Contains("надіслано"))
        {
            statusText.color = successColor;
        }
        else if (lower.Contains("надсилання") || lower.Contains("завантаження"))
        {
            statusText.color = loadingColor;
        }
        else
        {
            statusText.color = idleColor;
        }
    }
}