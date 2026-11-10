using UnityEngine;

public class ButtonClickRelay : MonoBehaviour
{
    public MenuTabletToggle tabletToggle;

    // Метод спрацьовує при дотику або кліку віртуальним контролером
    private void OnTriggerEnter(Collider other)
    {
        // Перевіряємо, чи це рука/контроллер гравця
        if (other.CompareTag("Player") || other.name.Contains("Hand") || other.name.Contains("Controller"))
        {
            if (tabletToggle != null)
            {
                tabletToggle.OpenMenuByButton();
            }
        }
    }
}