using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSpeed = 2f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotationX = rot.x;
        rotationY = rot.y;
    }

    void Update()
    {
        // Рух на клавіші WASD або стрілочки
        float moveX = Input.GetAxis("Horizontal"); // A/D
        float moveZ = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        transform.position += move * moveSpeed * Time.deltaTime;

        // Поворот голови/камери затиснутою правою кнопкою миші
        if (Input.GetMouseButton(1))
        {
            rotationY += Input.GetAxis("Mouse X") * lookSpeed;
            rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -80f, 80f);

            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        }
    }
}
