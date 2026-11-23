using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class PersistentWindowAnchor : MonoBehaviour
{
    [Tooltip("Об'єкт, поруч із яким закріплюється вікно (Cauldron)")]
    public Transform targetObject;

    [Tooltip("Відстань зліва від казана в метрах")]
    public float leftDistance = 0.85f;

    [Tooltip("Висота підняття вікна над казаном у метрах")]
    public float heightOffset = 0.45f;

    // Новий ключ (_v3), щоб перезаписати старий боковий якір
    private const string ANCHOR_UUID_KEY = "SavedInfoWindowAnchorUUID_v3";
    private OVRSpatialAnchor _spatialAnchor;

    void Start()
    {
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        }
        else
        {
            Invoke(nameof(OnSceneLoaded), 2f);
        }
    }

    private async void OnSceneLoaded()
    {
        await Task.Delay(600);

        if (PlayerPrefs.HasKey(ANCHOR_UUID_KEY))
        {
            string savedUuidStr = PlayerPrefs.GetString(ANCHOR_UUID_KEY);
            if (Guid.TryParse(savedUuidStr, out Guid savedUuid))
            {
                await LoadAnchor(savedUuid);
                return;
            }
        }

        PlaceLeftOfTargetFacingUser();
        await CreateAndSaveAnchor();
    }

    private void PlaceLeftOfTargetFacingUser()
    {
        if (targetObject == null) return;

        Transform cam = Camera.main != null ? Camera.main.transform : null;
        if (cam != null)
        {
            // Напрямок від гравця до казана по горизонталі
            Vector3 toTarget = targetObject.position - cam.position;
            toTarget.y = 0f;
            toTarget.Normalize();

            // Вектор «вліво» відносно погляду гравця
            Vector3 leftDir = -Vector3.Cross(Vector3.up, toTarget).normalized;

            // Ставимо вікно зліва від казана і трохи вище
            transform.position = targetObject.position + leftDir * leftDistance + Vector3.up * heightOffset;

            // Розвертаємо Canvas лицьовою стороною до гравця
            Vector3 lookAwayFromUser = transform.position - cam.position;
            lookAwayFromUser.y = 0f;
            if (lookAwayFromUser.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookAwayFromUser);
            }
        }
        else
        {
            transform.position = targetObject.position + new Vector3(-leftDistance, heightOffset, 0f);
        }
    }

    private async Task CreateAndSaveAnchor()
    {
        _spatialAnchor = gameObject.AddComponent<OVRSpatialAnchor>();

        while (_spatialAnchor != null && !_spatialAnchor.Created)
        {
            await Task.Yield();
        }

        if (_spatialAnchor == null) return;

        var result = await _spatialAnchor.SaveAnchorAsync();
        if (result.Success)
        {
            PlayerPrefs.SetString(ANCHOR_UUID_KEY, _spatialAnchor.Uuid.ToString());
            PlayerPrefs.Save();
            Debug.Log($"Spatial Anchor створено зліва від казана і збережено! UUID: {_spatialAnchor.Uuid}");
        }
        else
        {
            Debug.LogError($"Помилка збереження якоря: {result.Status}");
        }
    }

    private async Task LoadAnchor(Guid uuid)
    {
        var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(new[] { uuid }, unboundAnchors);

        if (result.Success && unboundAnchors.Count > 0)
        {
            var unbound = unboundAnchors[0];
            bool localized = await unbound.LocalizeAsync();
            if (localized)
            {
                _spatialAnchor = gameObject.AddComponent<OVRSpatialAnchor>();
                unbound.BindTo(_spatialAnchor);
                Debug.Log($"Spatial Anchor відновлено зліва від казана! UUID: {uuid}");
                return;
            }
        }

        Debug.LogWarning("Не вдалося завантажити якір, створюємо новий...");
        PlaceLeftOfTargetFacingUser();
        await CreateAndSaveAnchor();
    }
}