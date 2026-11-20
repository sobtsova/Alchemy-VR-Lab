using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    [Header("Налаштування API погоди (Open-Meteo)")]
    [SerializeField] private string weatherApiUrl =
        "https://api.open-meteo.com/v1/forecast?latitude=50.4501&longitude=30.5234&current=temperature_2m,weather_code";

    [Serializable]
    public class CurrentWeatherData
    {
        public float temperature_2m;
        public int weather_code;
    }

    [Serializable]
    public class WeatherApiResponse
    {
        public CurrentWeatherData current;
    }

    public struct AlchemyWeatherInfo
    {
        public float Temperature;
        public int WeatherCode;
        public string ConditionText;
        public string MagicAdvice;
    }

    public event Action<string> OnStatusChanged;
    public event Action<AlchemyWeatherInfo> OnWeatherLoaded;

    private void Start()
    {
        FetchAlchemyWeather();
    }

    public void FetchAlchemyWeather()
    {
        StartCoroutine(GetWeatherRoutine());
    }

    private IEnumerator GetWeatherRoutine()
    {
        Debug.Log($"[NetworkManager] Відправляю GET-запит: {weatherApiUrl}");
        OnStatusChanged?.Invoke("Завантаження даних погоди...");

        using (UnityWebRequest request = UnityWebRequest.Get(weatherApiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                OnStatusChanged?.Invoke($"Помилка з'єднання: {request.error}");
                Debug.LogError($"[NetworkManager] Помилка GET-запиту ({request.responseCode}): {request.error}");
                yield break;
            }

            string rawJson = request.downloadHandler.text;
            Debug.Log($"[NetworkManager] GET успішний (HTTP {request.responseCode}). JSON: {rawJson}");

            WeatherApiResponse parsed = JsonUtility.FromJson<WeatherApiResponse>(rawJson);

            if (parsed == null || parsed.current == null)
            {
                OnStatusChanged?.Invoke("Помилка формату даних сервера");
                Debug.LogError("[NetworkManager] Не вдалося розпарсити JSON (parsed або current == null).");
                yield break;
            }

            Debug.Log($"[NetworkManager] Розпарсено -> temperature_2m: {parsed.current.temperature_2m}, weather_code: {parsed.current.weather_code}");

            AlchemyWeatherInfo info = InterpretWeatherForAlchemy(
                parsed.current.temperature_2m,
                parsed.current.weather_code
            );

            Debug.Log($"[NetworkManager] Результат інтерпретації -> {info.ConditionText} ({info.Temperature:0.#}°C) | {info.MagicAdvice}");

            OnStatusChanged?.Invoke("Успішно оновлено");
            OnWeatherLoaded?.Invoke(info);
        }
    }

    private AlchemyWeatherInfo InterpretWeatherForAlchemy(float temp, int code)
    {
        string condition;
        string advice;

        if (code == 0 || code == 1)
        {
            condition = "Ясно / Сонячно";
            advice = "Сонячно: чаклувати не рекомендується — магія розсіюється.";
        }
        else if (code == 2 || code == 3 || (code >= 45 && code <= 48))
        {
            condition = "Хмарно / Туман";
            advice = "Хмарно: непогані умови для спокійних експериментів.";
        }
        else if ((code >= 51 && code <= 67) || (code >= 80 && code <= 82))
        {
            condition = "Дощ";
            advice = "Дощ: чудова погода для чаклування та змішування зілля!";
        }
        else if (code >= 95)
        {
            condition = "Гроза";
            advice = "Гроза: найкраща погода для чаклування — сила потоку на максимумі!";
        }
        else
        {
            condition = $"Код погоди {code}";
            advice = "Атмосфера стабільна — можна проводити досліди.";
        }

        return new AlchemyWeatherInfo
        {
            Temperature = temp,
            WeatherCode = code,
            ConditionText = condition,
            MagicAdvice = advice
        };
    }
}