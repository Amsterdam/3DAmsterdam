using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectComponent : MonoBehaviour
{
    public GameObject DragContainer;
    public Color SelectedColor;
    public bool IsTopComponent;
    public float ComponentWidth;
    public float ComponentHeight;

    public GameObject ComponentObject;

    private Toggle toggle;
    private Image image;
    
    private Vector3 startPositionDragContainer;

    void Start()
    {
        startPositionDragContainer = DragContainer.transform.position;

        toggle = GetComponent<Toggle>();
        image = GetComponent<Image>();

        toggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(toggle);
        });        
    }

    public void SetToggle(bool ison)
    {
        toggle.isOn = ison;
    }

    void ToggleValueChanged(Toggle changed)
    {
        image.color = changed.isOn ? SelectedColor : Color.white; 

        if (changed.isOn)
        {
            DragContainer.transform.position = startPositionDragContainer;
            DragContainer.SetActive(true);
            DragContainer.GetComponent<HandleDragContainer>().ComponentImage.enabled = true;

        }
        else if(toggle.group.AnyTogglesOn() == false)
        {
            DragContainer.SetActive(false);
        }

        LibraryComponentSelectedEvent.Raise(this, image, IsTopComponent, ComponentWidth, ComponentHeight, ComponentObject, this);


    }

}
