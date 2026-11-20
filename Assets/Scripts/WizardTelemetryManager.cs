using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WizardTelemetryManager : MonoBehaviour
{
    [Header("Налаштування POST-сервера")]
    [SerializeField] private string telemetryPostUrl = "https://httpbin.org/post";
    [SerializeField] private string wizardId = "lab3_alchemy_wizard_v1";
    [SerializeField] private bool startTimerOnStart = true;

    [Serializable]
    public class WizardTelemetryPayload
    {
        public string wizardId;
        public string sessionId;
        public float executionTimeSeconds;
        public int errorCount;
        public string[] errors;
        public string completedAtUtc;
    }

    public event Action<string> OnTelemetryStatusChanged;
    public event Action<string> OnTelemetrySentSuccess;

    private string _sessionId;
    private float _startTime;
    private bool _isRunning;
    private readonly System.Collections.Generic.List<string> _errors = new System.Collections.Generic.List<string>();

    private void Start()
    {
        if (startTimerOnStart)
        {
            BeginWizardSession();
        }
        else
        {
            OnTelemetryStatusChanged?.Invoke("Телеметрія: очікування запуску візарду");
        }
    }

    /// <summary>
    /// Викликай на старті проходження візарду.
    /// </summary>
    public void BeginWizardSession()
    {
        _sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
        _startTime = Time.realtimeSinceStartup;
        _errors.Clear();
        _isRunning = true;

        OnTelemetryStatusChanged?.Invoke($"Телеметрія: сесію #{_sessionId} розпочато");
    }

    /// <summary>
    /// Викликай щоразу, коли користувач робить помилку на кроці візарду.
    /// </summary>
    public void RecordWizardError(string errorMessage)
    {
        string entry = string.IsNullOrWhiteSpace(errorMessage)
            ? $"Невідома помилка на кроці ({_errors.Count + 1})"
            : errorMessage.Trim();

        _errors.Add(entry);
        Debug.LogWarning($"[WizardTelemetry] Зафіксовано помилку: {entry}");
    }

    /// <summary>
    /// Викликай після успішного завершення останнього кроку візарду.
    /// </summary>
    public void CompleteWizardAndSendReport()
    {
        float elapsed = _isRunning
            ? Mathf.Max(0f, Time.realtimeSinceStartup - _startTime)
            : 0f;

        _isRunning = false;

        WizardTelemetryPayload payload = new WizardTelemetryPayload
        {
            wizardId = wizardId,
            sessionId = string.IsNullOrEmpty(_sessionId) ? "manual_test" : _sessionId,
            executionTimeSeconds = (float)Math.Round(elapsed, 2),
            errorCount = _errors.Count,
            errors = _errors.ToArray(),
            completedAtUtc = DateTime.UtcNow.ToString("o")
        };

        string jsonPayload = JsonUtility.ToJson(payload, true);
        StartCoroutine(PostTelemetryRoutine(jsonPayload));
    }

    private IEnumerator PostTelemetryRoutine(string jsonPayload)
    {
        OnTelemetryStatusChanged?.Invoke("Статус: надсилання звіту телеметрії...");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(telemetryPostUrl, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string failMsg = $"Помилка з'єднання: {request.error}";
                OnTelemetryStatusChanged?.Invoke(failMsg);
                Debug.LogError($"[WizardTelemetry] POST-запит не вдався: {request.error}\nPayload:\n{jsonPayload}");
                yield break;
            }

            Debug.Log($"[WizardTelemetry] Успішно надіслано JSON:\n{jsonPayload}\nВідповідь сервера:\n{request.downloadHandler.text}");
            OnTelemetryStatusChanged?.Invoke("Статус: звіт телеметрії успішно надіслано");
            OnTelemetrySentSuccess?.Invoke(jsonPayload);
        }
    }
}