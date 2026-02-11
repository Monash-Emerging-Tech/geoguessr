using UnityEngine;
using UnityEngine.EventSystems;

/***
 * 
 * TooltipTrigger, Responsible for showing and hiding tooltips on UI elements
 *
 * Written by aleu0007
 * Last Modified: 11/02/2026
 * 
 */
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltipBox;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipBox != null)
            tooltipBox.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipBox != null)
            tooltipBox.SetActive(false);
    }

    private void Awake()
    {
        if (tooltipBox != null)
            tooltipBox.SetActive(false);
    }
}