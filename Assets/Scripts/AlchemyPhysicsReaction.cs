using UnityEngine;

public class AlchemyPhysicsReaction : MonoBehaviour
{
    // Спрацьовує при фізичному ударі об твердий стіл або підлогу
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Collision]: {gameObject.name} вдарився об {collision.gameObject.name}!");
    }

    // Спрацьовує при падінні крізь невидиму зону-тригер казанка
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Trigger]: {other.gameObject.name} потрапив у чарівний тригер {gameObject.name}!");
    }
}
