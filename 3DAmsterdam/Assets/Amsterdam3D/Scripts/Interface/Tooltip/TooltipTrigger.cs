using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField]
    private string tooltipText = "";
    public string TooltipText { get => tooltipText; set => tooltipText = value; }

    public void OnPointerDown(PointerEventData eventData)
    {
        TooltipDialog.Instance.Hide();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipDialog.Instance.ShowMessage(tooltipText);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipDialog.Instance.Hide();
    }
}
