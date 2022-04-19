using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Netherlands3D.LayerSystem;
using Netherlands3D.Interface.SidePanel;
using Netherlands3D.Help;
using Netherlands3D.Settings;

namespace Netherlands3D.Cameras
{
	public class StreetViewCamera : MonoBehaviour, ICameraControls
	{
		[SerializeField]
		private AssetbundleMeshLayer terrainContainerLayer;
		private Vector2 rotation = new Vector2(0, 0);
		public float speed = 3;

		[SerializeField]
		private GameObject mainMenu;

		[SerializeField]
		private Button hideMenuButton;

		[SerializeField]
		private Button exitFirstPersonButton;

		[SerializeField]
		private GameObject interfaceLayers;

		private Camera cameraComponent;

		private Ray ray;
		private RaycastHit hit;

		[SerializeField]
		private string helpMessage = "Kijk rond met de <b>muis</b>. Gebruik de <b>pijltjestoetsen</b> om te lopen. Houd <b>Shift</b> ingedrukt om te rennen.";
		public CameraMode Mode => CameraMode.StreetView;

		private void OnEnable()
		{
			cameraComponent = GetComponent<Camera>();
			ServiceLocator.GetService<HelpMessage>().Show(helpMessage);

			exitFirstPersonButton.gameObject.SetActive(false);
			DisableMenus();
		}

		private void OnDisable()
		{
			exitFirstPersonButton.gameObject.SetActive(true);
			hideMenuButton.gameObject.SetActive(false);
		}

		public void EnableKeyboardActionMap(bool enabled)
		{
			//
		}

		public void EnableMouseActionMap(bool enabled)
		{
			//
		}

		public void EnableMenus()
		{
			PointerLock.SetMode(PointerLock.Mode.DEFAULT);
			ServiceLocator.GetService<PropertiesPanel>().gameObject.SetActive(true);
			mainMenu.SetActive(true);
			hideMenuButton.gameObject.SetActive(true);
		}

		public void DisableMenus()
		{
			PointerLock.SetMode(PointerLock.Mode.FIRST_PERSON);
			ServiceLocator.GetService<PropertiesPanel>().gameObject.SetActive(false);
			mainMenu.SetActive(false);
			hideMenuButton.gameObject.SetActive(false);
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
				EnableMenus();
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

		public Vector3 GetPointerPositionInWorld(Vector3 optionalPositionOverride = default)
		{
			var pointerPosition = Input.mousePosition;
			if (optionalPositionOverride != default) pointerPosition = optionalPositionOverride;

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
	}
}