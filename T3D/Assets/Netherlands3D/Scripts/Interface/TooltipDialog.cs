using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Netherlands3D.Interface
{
    public class TooltipDialog : MonoBehaviour, IUniqueService
    {
        private Animator animator;
        private Text tooltiptext;
        private RectTransform rectTransform;
        private ContentSizeFitter contentSizeFitter;

        private bool pivotRight = false;

        private void Awake()
        {
            contentSizeFitter = GetComponent<ContentSizeFitter>();
            tooltiptext = GetComponentInChildren<Text>();
            rectTransform = GetComponent<RectTransform>();
            Hide();
        }

        private void FollowPointer()
		{
			rectTransform.position = Mouse.current.position.ReadValue();

			SwapPivot();
		}

		private void SwapPivot()
		{
			//Swap pivot based on place in screen (to try to stay in the screen horizontally)
			if (!pivotRight && rectTransform.position.x > Screen.width * 0.9f)
			{
				pivotRight = true;
				rectTransform.pivot = Vector2.right;
			}
			else if (pivotRight && rectTransform.position.x <= Screen.width * 0.9f)
			{
				pivotRight = false;
				rectTransform.pivot = Vector2.zero;
			}
		}

		public void AlignOnElement(RectTransform element)
        {
            if (element)
            {
                rectTransform.position = Vector3.Lerp(GetRectTransformBounds(element).center, GetRectTransformBounds(element).max, 0.5f);
                SwapPivot();
            }
        }

        public void ShowMessage(string message = "Tooltip", RectTransform hoverTarget = null){

            Debug.Log($"ShowMessage:{message}");

            //Restart our animator
            gameObject.transform.SetAsLastSibling(); //Make sure we are in front of all the UI

            AlignOnElement(hoverTarget);

            gameObject.SetActive(false);
            gameObject.SetActive(true);

            tooltiptext.text = message;

            StartCoroutine(FitContent());
        }

        private IEnumerator FitContent(){
            yield return new WaitForEndOfFrame();
            contentSizeFitter.enabled = false;
            contentSizeFitter.enabled = true;
        }

        public void Hide(){
            gameObject.SetActive(false);
        }

        private Vector3[] worldCorners = new Vector3[4];
        private Bounds GetRectTransformBounds(RectTransform transform)
        {
            transform.GetWorldCorners(worldCorners);
            Bounds bounds = new Bounds(worldCorners[0], Vector3.zero);
            for (int i = 1; i < 4; ++i)
            {
                bounds.Encapsulate(worldCorners[i]);
            }
            return bounds;
        }
    }
}
