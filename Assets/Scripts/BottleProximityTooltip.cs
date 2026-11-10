using UnityEngine;

public class BottleProximityTooltip : MonoBehaviour
{
    public ObjectTooltipController tooltipController;

    private void OnTriggerEnter(Collider other)
    {
        // Якщо поруч рука або гравець
        if (other.name.Contains("Hand") || other.name.Contains("Controller") || other.name.Contains("Camera"))
        {
            if (tooltipController != null) tooltipController.ShowTooltip();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name.Contains("Hand") || other.name.Contains("Controller") || other.name.Contains("Camera"))
        {
            if (tooltipController != null) tooltipController.HideTooltip();
        }
    }
}