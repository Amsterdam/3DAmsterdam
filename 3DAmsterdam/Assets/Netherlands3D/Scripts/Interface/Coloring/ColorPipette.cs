using Netherlands3D.Cameras;
using Netherlands3D.Interface.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Netherlands3D.Interface.Coloring
{ 
    public class ColorPipette : MonoBehaviour
    {
        [SerializeField]
        private ColorPicker colorPicker;
        private Texture2D screenTexture;

        [SerializeField]
        private Color pickedColor = Color.white;

        [SerializeField]
        private Image selectionPointer;

        [SerializeField]
        private Image activeImageIcon;
        [SerializeField]
        private Color activeIconColor;
        private Color defaultIconColor;

        private Rect viewRectangle;

        private GraphicRaycaster parentCanvasGraphicRaycaster;

        private void Start()
        {
            //Move our selectionpointer circle to the front canvas layer
            selectionPointer.rectTransform.SetParent(selectionPointer.canvas.transform);
            selectionPointer.rectTransform.SetAsLastSibling();

            parentCanvasGraphicRaycaster = selectionPointer.canvas.GetComponent<GraphicRaycaster>();
            defaultIconColor = activeImageIcon.color;
        }

        /// <summary>
        /// Start using the pipette color selection.
        /// We jump in to our color selection mode, where we can only click to select a color.
        /// </summary>
        public void StartColorSelection()
        {
            selectionPointer.gameObject.SetActive(true);

            //We dont allow clicks on the canvas, untill we are done picking a color
            parentCanvasGraphicRaycaster.enabled = false;

            //We create a new texture once, for the sake of performance
            viewRectangle = CameraModeChanger.Instance.ActiveCamera.pixelRect;
            screenTexture = new Texture2D((int)viewRectangle.width, (int)viewRectangle.height, TextureFormat.RGB24, false);

            activeImageIcon.color = activeIconColor;

            StartCoroutine(ContinuousColorPick());
        }

        /// <summary>
        /// Keep reading the pixel color under our mouse
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// We get the pixel Color under our mouse pointer.
        /// </summary>
        private void GrabPixelUnderMouse()
        {
            pickedColor = screenTexture.GetPixel((int)Input.mousePosition.x, (int)Input.mousePosition.y);
            selectionPointer.transform.position = Input.mousePosition;
            selectionPointer.color = pickedColor;
        }

        /// <summary>
        /// Grabs the pixels of the rendered screen and apply them to our texture.
        /// </summary>
        private void ReadCurrentScreenToTexture()
        {
            screenTexture.ReadPixels(viewRectangle, 0, 0, false);
            screenTexture.Apply(false);
        }

        /// <summary>
        /// We clicked and go back to normal.
        /// Optionaly we send the color we selected to our ColorPicker.
        /// </summary>
        /// <param name="useColor">Use color in ColorPicker</param>
        private void DonePicking(bool useColor)
        {
            Destroy(screenTexture);

            //Allow interaction on the canvas again
            parentCanvasGraphicRaycaster.enabled = true;

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
}