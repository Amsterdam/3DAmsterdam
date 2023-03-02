using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Netherlands3D.TileSystem;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.Help;
using Netherlands3D.Settings;
using Netherlands3D.Events;

namespace Netherlands3D.Cameras
{
	public class StreetViewCamera : MonoBehaviour, ICameraControls
	{
		[SerializeField]
		private BinaryMeshLayer terrainContainerLayer;
		private Vector2 rotation = new Vector2(0, 0);
		public float speed = 3;

		private Camera cameraComponent;

		private Ray ray;
		private RaycastHit hit;

		[Multiline]
		[SerializeField]
		private string helpMessage = "Kijk rond met de <b>muis</b>. Gebruik de <b>pijltjestoetsen</b> om te lopen. Houd <b>Shift</b> ingedrukt om te rennen.\n\n Gebruik de <b>Escape</b> toets om te stoppen.";

		[SerializeField]
		private BoolEvent firstPersonModeActive;

		private void Awake()
		{
			cameraComponent = GetComponent<Camera>();
		}

		private void OnEnable()
		{
			HelpMessage.Show(helpMessage);
			PointerLock.SetMode(PointerLock.Mode.FIRST_PERSON);
			firstPersonModeActive.InvokeStarted(true);
		}

		private void OnDisable()
		{
			HelpMessage.Hide();
			firstPersonModeActive.InvokeStarted(false);
		}

		public void EnableKeyboardActionMap(bool enabled)
		{
			//
		}

		public void EnableMouseActionMap(bool enabled)
		{
			//
		}

		public void MoveAndFocusOnLocation(Vector3 targetLocation, Quaternion rotation)
		{
			if (targetLocation.y < Config.activeConfiguration.zeroGroundLevelY + 1.8f)
			{
				targetLocation.y = Config.activeConfiguration.zeroGroundLevelY + 1.81f;
			}

			transform.position = targetLocation;
			transform.rotation = rotation;
			Vector2 rotationEuler = rotation.eulerAngles;
			if (rotationEuler.x > 180)
			{
				rotationEuler.x -= 360f;
			}

			if (rotationEuler.x < -180)
			{
				rotationEuler.x += 360f;
			}
			this.rotation = rotationEuler;
		}

		void Update()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			//In WebGL we catch this unlocking on the JS side
			if(Input.GetKeyDown(KeyCode.Escape) && PointerLock.GetMode() == PointerLock.Mode.FIRST_PERSON)
			{
				CameraModeChanger.Instance.GodViewMode();
				HelpMessage.Hide(true);
			}
#endif
			if (PointerLock.GetMode() == PointerLock.Mode.FIRST_PERSON)
			{
				FollowMouseRotation();
			}
		}
		private void FollowMouseRotation()
		{
			rotation.y += Mouse.current.delta.ReadValue().x * speed * ApplicationSettings.settings.rotateSensitivity;
			rotation.x += -Mouse.current.delta.ReadValue().y * speed * ApplicationSettings.settings.rotateSensitivity;
			rotation.x = ClampAngle(rotation.x, -90, 90);
			transform.eulerAngles = (Vector2)rotation;
		}

		public float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360F)
				angle += 360F;
			if (angle > 360F)
				angle -= 360F;
			return Mathf.Clamp(angle, min, max);
		}

		public float GetNormalizedCameraHeight()
		{
			return Mathf.InverseLerp(1.8f, 2500, transform.position.y);
		}

		public float GetCameraHeight()
		{
			return transform.position.y;
		}
		public void OnRotation(Quaternion rotation)
		{
			Vector2 rotationEuler = rotation.eulerAngles;
			if (rotationEuler.x > 180)
			{
				rotationEuler.x -= 360f;
			}

			if (rotationEuler.x < -180)
			{
				rotationEuler.x += 360f;
			}

			this.rotation = rotationEuler;
		}

		public Ray GetMainPointerRay()
		{
			var pointerPosition = Mouse.current.position.ReadValue();
			if (PointerLock.GetMode() == PointerLock.Mode.FIRST_PERSON)
			{
				pointerPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
			}

			return cameraComponent.ScreenPointToRay(pointerPosition);
		}

		public Vector3 GetPointerPositionInWorld(Vector3 optionalPositionOverride = default)
		{
			var pointerPosition = Mouse.current.position.ReadValue();
			if (optionalPositionOverride != default)
			{
				pointerPosition = optionalPositionOverride;
			}
			else if(PointerLock.GetMode() == PointerLock.Mode.FIRST_PERSON)
			{
				pointerPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f,0);
			}

			if(cameraComponent)
				ray = cameraComponent.ScreenPointToRay(pointerPosition);
			
			float distance = 99;
			if (Physics.Raycast(ray, out hit, distance))
			{
				terrainContainerLayer.AddMeshColliders(hit.point);
				return hit.point;
			}
			else
			{
				terrainContainerLayer.AddMeshColliders(ray.origin + ray.direction * (distance / 10));
				// return end of mouse ray if nothing collides
				return ray.origin + ray.direction * (distance / 10);
			}
		}

		public void SetNormalizedCameraHeight(float height)
		{
			//TODO: Determine if we want to expose the height slider.
		}

		public bool UsesActionMap(InputActionMap actionMap)
		{
			//TODO: Requires switch to actionmap inputs
			return false;
		}

		public void ResetNorth(bool resetTopDown = false)
		{
			this.transform.forward = Vector3.forward;
		}

		/// <summary>
		/// Ortographic wouldnt make sense in an FPS camera, but we can change the fov to something a little less extreme as a fallback.
		/// </summary>
		/// <returns>Ortographic on</returns>
		public void ToggleOrtographic(bool ortographicOn)
		{
			if (ortographicOn)
			{
				cameraComponent.fieldOfView = 30;
			}

			cameraComponent.fieldOfView = 60;
		}
	}
}