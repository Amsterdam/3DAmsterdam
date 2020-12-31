using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Amsterdam3D.JavascriptConnection;


namespace Amsterdam3D.CameraMotion
{
	public class StreetViewCamera : MonoBehaviour, ICameraControls
	{
		private Vector2 rotation = new Vector2(0, 0);
		public float speed = 3;

		[SerializeField]
		private GameObject mainMenu;

		[SerializeField]
		private GameObject interfaceLayers;

		private Camera cameraComponent;

		private Ray ray;
		private RaycastHit hit;

		private void OnEnable()
		{
			cameraComponent = GetComponent<Camera>();
			DisableMenus();
		}

		public void EnableMenus()
		{
			interfaceLayers.SetActive(true);
			mainMenu.SetActive(true);
		}

		public void DisableMenus()
		{
			interfaceLayers.SetActive(false);
			mainMenu.SetActive(false);
		}

		public void MoveAndFocusOnLocation(Vector3 targetLocation, Quaternion rotation)
		{
			if (targetLocation.y < Constants.ZERO_GROUND_LEVEL_Y + 1.8f)
			{
				targetLocation.y = Constants.ZERO_GROUND_LEVEL_Y + 1.81f;
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
			if (PointerLock.GetMode() == PointerLock.Mode.DEFAULT)
			{
				if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
				{
					DisableMenus();
					PointerLock.SetMode(PointerLock.Mode.FIRST_PERSON);
				}
			}
			else
			{
				FollowMouseRotation();

				if (Input.GetMouseButtonDown(0) && PointerLock.GetMode() != PointerLock.Mode.FIRST_PERSON)
				{
					DisableMenus();
					PointerLock.SetMode(PointerLock.Mode.FIRST_PERSON);
				}
			}
		}

		private void FollowMouseRotation()
		{
			rotation.y += Input.GetAxis("Mouse X") * speed;
			rotation.x += -Input.GetAxis("Mouse Y") * speed;
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

		public Vector3 GetMousePositionInWorld(Vector3 optionalPositionOverride = default)
		{
			var pointerPosition = Input.mousePosition;
			if (optionalPositionOverride != default) pointerPosition = optionalPositionOverride;

			ray = cameraComponent.ScreenPointToRay(pointerPosition);
			float distance = 99;
			if (Physics.Raycast(ray, out hit, distance))
			{
				return hit.point;
			}
			else
			{
				// return end of mouse ray if nothing collides
				return ray.origin + ray.direction * (distance / 10);
			}
		}

		public void SetNormalizedCameraHeight(float height)
		{
			//TODO: Determine if we want to expose the height slider.
		}
	}
}