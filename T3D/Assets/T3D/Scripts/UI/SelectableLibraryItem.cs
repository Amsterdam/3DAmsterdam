using System.Collections;
using System.Collections.Generic;
using Netherlands3D.T3D.Uitbouw.BoundaryFeatures;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SelectableLibraryItem : MonoBehaviour, IDragHandler
{
    public GameObject DragContainer;
    public Sprite DragContainerImage;
    public Color SelectedColor;
    public bool IsTopComponent;

    private Toggle toggle;
    private Image image;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        image = GetComponent<Image>();
    }

    protected abstract void OnLibraryItemSelected();

    public void Deslect()
    {
        SetComponentSelected(false);
    }

    void SetComponentSelected(bool changed)
    {
        toggle.isOn = changed;
        image.color = changed ? SelectedColor : Color.white;

        if (changed)
        {
            DragContainer.transform.position = Input.mousePosition;
            DragContainer.SetActive(true);
            DragContainer.GetComponent<HandleDragContainer>().ComponentImage.enabled = true;

        }
        else if (!toggle.group.AnyTogglesOn())
        {
            DragContainer.SetActive(false);
        }
        OnLibraryItemSelected();
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetComponentSelected(true);
    }
}
