using UnityEngine;

public class SnapZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Перевіряємо, чи це пляшечка з зіллям
        if (other.name.Contains("PotionBottle"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Скидаємо швидкість польоту/падіння
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Ставимо пляшку рівно по центру нашої підставки
            other.transform.position = transform.position + Vector3.up * 0.12f;
            other.transform.rotation = Quaternion.identity;
        }
    }
}