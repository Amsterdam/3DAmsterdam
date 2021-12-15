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

    //[SerializeField]
    //private UitbouwRestrictionType restrictionType;
    //private UitbouwRestriction restriction;

    //private void Awake()
    //{
        //RestrictionChecker.ActiveRestrictions.TryGetValue(restrictionType, out restriction);
    //}

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
