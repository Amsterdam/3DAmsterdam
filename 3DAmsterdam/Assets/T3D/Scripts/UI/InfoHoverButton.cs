using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw;
using UnityEngine;
using UnityEngine.EventSystems;

public class InfoHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, TextArea]
    protected string defaultPopupText;

    [SerializeField]
    protected GameObject popupPrefab;
    protected GameObject popup;

    public void OnPointerEnter(PointerEventData eventData)
    {
        CreatePopup();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(popup);
    }

    protected virtual void CreatePopup()
    {
        popup = Instantiate(popupPrefab, transform.position, transform.rotation, transform);
        popup.GetComponentInChildren<PopupInfo>().SetText(defaultPopupText);
    }
}
