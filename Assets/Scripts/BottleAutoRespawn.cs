using UnityEngine;

public class BottleAutoRespawn : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        string hitName = collision.gameObject.name.ToLower();

        // Якщо вдарилася об стіл, стіну чи підлогу (але не об казан/іншу баночку)
        if (hitName.Contains("table") || hitName.Contains("desk") || 
            hitName.Contains("floor") || hitName.Contains("plane") || 
            hitName.Contains("room"))
        {
            GameObject root = AlchemyPhysicsReaction.FindBottleRoot(gameObject);
            if (root != null)
            {
                AlchemyPhysicsReaction.ReturnBottleToShelf(root);
                Debug.Log($"[Баночка]: {root.name} впала на стіл/підлогу та повернулася на полицю!");
            }
        }
    }
}