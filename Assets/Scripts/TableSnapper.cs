using UnityEngine;
using Meta.XR.MRUtilityKit;

public class TableSnapper : MonoBehaviour
{
    [Tooltip("Висота підняття казана над стільницею (підбери, якщо провалюється)")]
    public float heightOffset = 0.25f;

    void Start()
    {
        if (MRUK.Instance != null)
        {
            MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
        }
    }

    void OnSceneLoaded()
    {
        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null) return;

        foreach (MRUKAnchor anchor in room.Anchors)
        {
            if (anchor.Label == MRUKAnchor.SceneLabels.TABLE)
            {
                Vector3 tableTopPos = anchor.GetAnchorCenter();
                if (anchor.VolumeBounds.HasValue)
                {
                    tableTopPos.y = anchor.transform.position.y + heightOffset;
                }

                transform.position = tableTopPos;
                transform.rotation = Quaternion.Euler(0, anchor.transform.eulerAngles.y, 0);

                Debug.Log($"Казан прив'язано до реального столу: {tableTopPos}");
                break;
            }
        }
    }
}