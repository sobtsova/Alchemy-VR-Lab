using TMPro;
using UnityEngine;

public class WeatherDashboardUI : MonoBehaviour
{
    [Header("Джерело даних")]
    [SerializeField] private NetworkManager networkManager;

    [Header("Текстові поля дашборду")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text temperatureText;
    [SerializeField] private TMP_Text conditionText;
    [SerializeField] private TMP_Text adviceText;

    [Header("3D-префаби погоди")]
    [SerializeField] private GameObject sun3DObject;
    [SerializeField] private GameObject cloud3DObject;
    [SerializeField] private GameObject rain3DObject;
    [SerializeField] private GameObject storm3DObject;

    private void OnEnable()
    {
        if (networkManager != null)
        {
            networkManager.OnStatusChanged += HandleStatusChanged;
            networkManager.OnWeatherLoaded += HandleWeatherLoaded;
        }
    }

    private void OnDisable()
    {
        if (networkManager != null)
        {
            networkManager.OnStatusChanged -= HandleStatusChanged;
            networkManager.OnWeatherLoaded -= HandleWeatherLoaded;
        }
    }

    private void HandleStatusChanged(string status)
    {
        if (statusText != null)
        {
            statusText.text = $"Статус: {status}";
        }
    }

    private void HandleWeatherLoaded(NetworkManager.AlchemyWeatherInfo info)
    {
        if (temperatureText != null) temperatureText.text = $"{info.Temperature:0.#}°C";
        if (conditionText != null) conditionText.text = info.ConditionText;
        if (adviceText != null) adviceText.text = info.MagicAdvice;

        Update3DVisuals(info.WeatherCode);
    }

    private void Update3DVisuals(int code)
    {
        int activeIndex = 1; // 0-сонце, 1-хмара, 2-дощ, 3-гроза

        if (code == 0 || code == 1)
        {
            activeIndex = 0;
        }
        else if ((code >= 51 && code <= 67) || (code >= 80 && code <= 82))
        {
            activeIndex = 2;
        }
        else if (code >= 95)
        {
            activeIndex = 3;
        }

        if (sun3DObject != null) sun3DObject.SetActive(activeIndex == 0);
        if (cloud3DObject != null) cloud3DObject.SetActive(activeIndex == 1);
        if (rain3DObject != null) rain3DObject.SetActive(activeIndex == 2);
        if (storm3DObject != null) storm3DObject.SetActive(activeIndex == 3);
    }
}