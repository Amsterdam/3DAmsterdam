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
    private bool pointerInRect;

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInRect = true;
        if (!popup)
            CreatePopup();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInRect = false;
    }

    private void Update()
    {
        if (popup && !pointerInRect && !popup.GetComponentInChildren<PopupInfo>().PointerInRect)
        {
            Destroy(popup);
        }
    }

    protected virtual void CreatePopup()
    {
        popup = Instantiate(popupPrefab, transform.position, transform.rotation, GetComponentInParent<State>().transform);
        popup.GetComponentInChildren<PopupInfo>().SetText(defaultPopupText);
    }
}
