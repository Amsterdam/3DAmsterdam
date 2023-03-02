using Netherlands3D.Cameras;
using Netherlands3D.Events;
using Netherlands3D.Help;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.ObjectInteraction;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Netherlands3D.Interface
{
    [RequireComponent(typeof(WorldPointFollower))]
    public class FirstpersonCameraPlacer : Interactable
    {
        [SerializeField] private InputActionAsset actionAsset;
        private InputActionMap releaseActionMap;
        private InputAction releaseAction;

        [HideInInspector] public Quaternion cameraRotation = Quaternion.identity;
        [SerializeField] private TriggerEvent startPlacement;
        [SerializeField] private bool pushCameraOnScreenBounds = true;
        [SerializeField] private float panSpeed = 1.0f;
        private WorldPointFollower worldPointFollower;
        private Vector3 startPosition;
        private bool placing = false;

        private void Awake()
        {
            startPosition = this.transform.localPosition;

            releaseActionMap = actionAsset.FindActionMap("UI");
            releaseAction = releaseActionMap.FindAction("Click");

            worldPointFollower = GetComponent<WorldPointFollower>();
            startPlacement.AddListenerStarted(StartPlacement);
        }

        private void Placed()
        {
            placing = false;
            cameraRotation = CameraModeChanger.Instance.ActiveCamera.transform.rotation;
            CameraModeChanger.Instance.FirstPersonMode(worldPointFollower.WorldPosition, cameraRotation);
            StopInteraction();

            gameObject.SetActive(false);
        }

        public override void Escape()
        {
            base.Escape();
            this.gameObject.SetActive(false);
        }

        private void StartPlacement()
        {
            gameObject.SetActive(true);
            HelpMessage.Show("<b>Klik</b> op het maaiveld om een camerastandpunt te plaatsen\n\nGebruik de <b>Escape</b> toets om te annuleren");
            placing = true;
            StartCoroutine(FollowPointer());
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (placing)
            {
                placing = false;
                StopAllCoroutines();
            }
        }

        private IEnumerator FollowPointer()
        {
            yield return new WaitForEndOfFrame();
            var rectTransform = this.GetComponent<RectTransform>();
            while (placing)
            {
                yield return new WaitForEndOfFrame();
                if (pushCameraOnScreenBounds && Vector3.Distance(rectTransform.localPosition, startPosition) > 0.10f)
                {
                    var margin = 0.47f; //based on center based origin, ranging from -50 to 50
                    var normalisedPointerPosition = new Vector2(rectTransform.localPosition.x / Screen.width, rectTransform.localPosition.y / Screen.height);
                    var cameraForward = Camera.main.transform.forward;
                    cameraForward.y = 0;

                    if (Mathf.Abs(normalisedPointerPosition.x) > margin)
                    {
                        Camera.main.transform.Translate(Vector3.right * normalisedPointerPosition.x * panSpeed * Mathf.Abs(Camera.main.transform.position.y), Space.Self);
                    }
                    else if (Mathf.Abs(normalisedPointerPosition.y) > margin)
                    {
                        Camera.main.transform.Translate(cameraForward.normalized * normalisedPointerPosition.y * panSpeed * Mathf.Abs(Camera.main.transform.position.y), Space.World);
                    }
                }

                var targetLocation = (Selector.hits.Length > 0) ? Selector.hits[0].point : CameraModeChanger.Instance.CurrentCameraControls.GetPointerPositionInWorld();
                worldPointFollower.AlignWithWorldPosition(targetLocation + Vector3.up * 1.8f);

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    placing = false;
                    Placed();
                }
            }

        }
    }
}