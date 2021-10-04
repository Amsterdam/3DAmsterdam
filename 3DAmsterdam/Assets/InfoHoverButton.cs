using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InfoHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, TextArea]
    private string infoText;
    [SerializeField]
    private GameObject popupPrefab;
    private GameObject popup;

    public void OnPointerEnter(PointerEventData eventData)
    {
        CreatePopup();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
            Destroy(popup);
    }

    void CreatePopup()
    {
        popup = Instantiate(popupPrefab, transform.position, transform.rotation, transform);
        popup.GetComponentInChildren<PopupInfo>().SetText(infoText);
    }
}
