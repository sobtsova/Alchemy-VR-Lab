using UnityEngine;

public class ObjectTooltipController : MonoBehaviour
{
    [Header("Tooltip Settings")]
    public GameObject tooltipCanvasObject; 
    public string tooltipText;             
    public TMPro.TextMeshProUGUI labelUI;  

    // Метод спрацьовує, коли курсор, лазер (Ray) або рука наводиться на об'єкт
    public void ShowTooltip()
    {
        if (tooltipCanvasObject != null)
        {
            // Змінюємо текст під конкретний об'єкт
            if (labelUI != null)
            {
                labelUI.text = tooltipText;
            }

            // Ставимо картку трохи вище об'єкта у просторі
            tooltipCanvasObject.transform.position = transform.position + Vector3.up * 0.3f;
            
            // Вмикаємо картку
            tooltipCanvasObject.SetActive(true);
        }
    }

    // Метод спрацьовує, коли гравець прибирає руку або промінь
    public void HideTooltip()
    {
        if (tooltipCanvasObject != null)
        {
            tooltipCanvasObject.SetActive(false);
        }
    }
}