using Amsterdam3D.CameraMotion;
using Amsterdam3D.InputHandler;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Amsterdam3D.Interface
{
    [RequireComponent(typeof(WorldPointFollower))]
	public class PlaceOnClick : Interactable, IDragHandler
    {
        public bool waitingForClick = true;
        private IAction placeAction;

        private WorldPointFollower worldPointerFollower;
		public WorldPointFollower WorldPointerFollower { get => worldPointerFollower; private set => worldPointerFollower = value; }

        public CustomLayer interfaceLayer { get; set; }

        private Vector3 targetLocation;

        public virtual void Awake()
		{
            WorldPointerFollower = GetComponent<WorldPointFollower>(); 
        }

		public virtual void Start()
        {
            ActionMap = ActionHandler.actions.PlaceOnClick;
            placeAction = ActionHandler.instance.GetAction(ActionHandler.actions.PlaceOnClick.Place);
            if (waitingForClick)
            {
                placeAction.SubscribePerformed(Place);
                PlaceUsingPointer();
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (waitingForClick) return;
            print("Drag action");
            FollowMousePointer();
        }

        private void Place(IAction action)
        {
            if (waitingForClick && action.Performed)
            {
                StopInteraction();
                Placed();
            }
        }
        protected virtual void Placed()
        {
            Debug.Log("Placed object", this.gameObject);
            waitingForClick = false;
        }

        public void PlaceUsingPointer()
        {
            TakeInteractionPriority();
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
        }

        /// <summary>
        /// Align the annotation with the mouse pointer position (on raycast hits, or a fallback to a world plane)
        /// </summary>
        private void FollowMousePointer()
        {
            //Check if the selector has raycast hits, otherwise use our camera world plane
            targetLocation = (Selector.hits.Length > 0) ? Selector.hits[0].point : CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
            targetLocation += Vector3.up * 1.8f;

            WorldPointerFollower.AlignWithWorldPosition(targetLocation);
        }
	}
}