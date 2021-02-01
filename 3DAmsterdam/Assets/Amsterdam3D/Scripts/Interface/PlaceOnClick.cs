using Amsterdam3D.CameraMotion;
using Amsterdam3D.InputHandler;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Amsterdam3D.Interface
{
	public class PlaceOnClick : MonoBehaviour, IDragHandler
    {
        protected bool waitingForClick = true;
        private IAction placeAction;

        private InputActionMap actionMap;

        private WorldPointFollower worldPointerFollower;

        public virtual void Start()
        {
            worldPointerFollower = GetComponent<WorldPointFollower>();
            actionMap = ActionHandler.actions.PlaceOnClick;

            placeAction = ActionHandler.instance.GetAction(ActionHandler.actions.PlaceOnClick.Place);
            placeAction.SubscribePerformed(Place);
            PlaceUsingPointer();
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!waitingForClick) return;
            FollowMousePointer();
        }

        public void Place(IAction action)
        {
            if (waitingForClick && action.Performed)
            {
                waitingForClick = false;
            }
        }

        public void PlaceUsingPointer()
        {
            StopAllCoroutines();
            StartCoroutine(StickToPointer());
        }

        /// <summary>
        /// Stick to the mouse pointer untill we click. 
        /// Starts editing after the click.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator StickToPointer()
        {
            while (waitingForClick)
            {
                FollowMousePointer();
                yield return new WaitForEndOfFrame();
            }
            Placed();
        }

        protected virtual void Placed()
        {
            Debug.Log("Placed object", this.gameObject);
		}

        /// <summary>
        /// Align the annotation with the mouse pointer position
        /// </summary>
        private void FollowMousePointer()
        {
            worldPointerFollower.AlignWithWorldPosition(CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld());
        }
	}
}