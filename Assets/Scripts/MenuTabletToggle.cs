using UnityEngine;

public class MenuTabletToggle : MonoBehaviour
{
    [Header("Main Canvas to Spawn")]
    public GameObject mainCanvasObject; 
    public QuizScoreManager scoreManager; 

    [Header("Settings")]
    public float spawnDistance = 1.5f;
    public Transform playerCamera;

    // Метод викликається кнопкою на столі
    public void OpenMenuByButton()
    {
        if (mainCanvasObject == null || scoreManager == null) return;

        // Перемикаємо весь Canvas у погодженні з катерогою погляду
        bool isActive = mainCanvasObject.activeSelf;
        mainCanvasObject.SetActive(!isActive);

        // Якщо канвас щойно відкрився — ставимо його прямо перед очима гравця
        if (!isActive && playerCamera != null)
        {
            Vector3 targetPosition = playerCamera.position + playerCamera.forward * spawnDistance;
            mainCanvasObject.transform.position = targetPosition;
            mainCanvasObject.transform.LookAt(playerCamera);
            mainCanvasObject.transform.Rotate(0, 180f, 0);
        }
    }
}