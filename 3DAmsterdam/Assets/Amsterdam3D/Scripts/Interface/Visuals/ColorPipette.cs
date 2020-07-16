using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPipette : MonoBehaviour
{
    [SerializeField]
    private ColorPicker colorPicker;
    private Texture2D screenTexture;

    [SerializeField]
    private Color pickedColor = Color.white;

    [SerializeField]
    private ClickOutsideToClose panelCloseCatcher;

    [SerializeField]
    private Image selectionPointer;

    [SerializeField]
    private Image activeImageIcon;
    [SerializeField]
    private Color activeIconColor;
    private Color defaultIconColor;

    private Rect viewRectangle;

    private void Start()
    {
        //Move our selectionpointer circle to the front canvas layer
        selectionPointer.rectTransform.SetParent(selectionPointer.canvas.transform);
        selectionPointer.rectTransform.SetAsLastSibling();

        defaultIconColor = activeImageIcon.color;
    }

    public void StartColorSelection()
    {
        panelCloseCatcher.IgnoreClicks(1); //This will keep our color panel open when we click the screen
        selectionPointer.gameObject.SetActive(true);

        //We create a new texture once, for the sake of performance
        viewRectangle = Camera.main.pixelRect;
        screenTexture = new Texture2D((int)viewRectangle.width, (int)viewRectangle.height, TextureFormat.RGB24, false);

        activeImageIcon.color = activeIconColor;

        StartCoroutine(ContinuousColorPick());
    }

    private IEnumerator ContinuousColorPick()
    {
        while (true)
        {
            if (Input.GetMouseButton(0))
            {
                DonePicking(true);
            }
            else if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                DonePicking(false);
            }

            yield return new WaitForEndOfFrame();
            ReadCurrentScreenToTexture();
            GrabPixelUnderMouse();

            yield return null;
        }
    }

    private void GrabPixelUnderMouse()
    {
        pickedColor = screenTexture.GetPixel((int)Input.mousePosition.x, (int)Input.mousePosition.y);
        selectionPointer.transform.position = Input.mousePosition;
        selectionPointer.color = pickedColor;
    }

    private void ReadCurrentScreenToTexture()
    {
        screenTexture.ReadPixels(viewRectangle, 0, 0, false);
        screenTexture.Apply(false);
    }

    private void DonePicking(bool useColor)
    {
        Destroy(screenTexture);

        activeImageIcon.color = defaultIconColor;

        selectionPointer.gameObject.SetActive(false);
        if (useColor)
        {
            colorPicker.ChangeColorInput(pickedColor);
            colorPicker.selectedNewColor.Invoke(pickedColor, colorPicker);
        }
        StopAllCoroutines();
    }
}
