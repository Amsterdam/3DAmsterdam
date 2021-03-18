using Netherlands3D.Cameras;
using Netherlands3D.LayerSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Underground
{
	public class RuntimeMaskSphere : MonoBehaviour
	{
		[SerializeField]
		private bool updateRuntimeDynamicMaterials = false;

		[SerializeField]
		private Transform targetMaterialsContainer;

		[SerializeField]
		private Material[] specificMaterials;

		[SerializeField]
		private Transform pointerFollower;

		[SerializeField]
		private float maskScale = 2.0f;

		Vector4 maskVector = default;

		[SerializeField]
		private AssetbundleMeshLayer groundMeshLayer;

		//We enable shadows for the underground when we use the mask sphere, to give more depth
		private void OnEnable()
		{
			//groundMeshLayer.EnableShadows(true); //We use shadows all the time now, but this might give a performance boost later on
		}

		//We enable shadows for the underground when we use the mask sphere, to give more depth
		private void OnDisable()
		{
			//groundMeshLayer.EnableShadows(false); //We use shadows all the time now, but this might give a performance boost later on

			//make sure to reset mask shaders
			UpdateSpecificMaterials(true);
		}

		void Update()
		{
			if (CameraModeChanger.Instance.CameraMode != CameraMode.GodView) return;

			MoveMask();

			UpdateDynamicCreatedInstancedMaterials();
			UpdateSpecificMaterials();
		}

		private void MoveMask()
		{
			pointerFollower.position = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
			pointerFollower.transform.localScale = Vector3.one * maskScale * CameraModeChanger.Instance.ActiveCamera.transform.position.y;
			maskVector.Set(
				pointerFollower.position.x,
				pointerFollower.position.y,
				pointerFollower.position.z,
				((CameraModeChanger.Instance.ActiveCamera.transform.position.y > pointerFollower.position.y) && !EventSystem.current.IsPointerOverGameObject()) ?
				pointerFollower.transform.localScale.x * 0.47f : 0.0f
			);

			//Hide mask object if we are underground
			pointerFollower.gameObject.SetActive((maskVector.w > 0.0f));
		}

		/// <summary>
		/// Update specific shared materials that use the clipping mask subgraph
		/// </summary>
		/// <param name="resetToZero">Set all mask parameters to zero to make sure our editor does not change the materials</param>
		private void UpdateSpecificMaterials(bool resetToZero = false)
		{
			foreach (Material sharedMaterial in specificMaterials)
			{
				sharedMaterial.SetVector("_ClippingMaskDome", (resetToZero) ? Vector4.zero : maskVector);
			}
		}

		/// <summary>
		/// Optionally find runtime created materials that we cant predefine in our specificMaterials list
		/// </summary>
		private void UpdateDynamicCreatedInstancedMaterials()
		{
			if (!updateRuntimeDynamicMaterials) return;

			MeshRenderer[] meshRenderers = targetMaterialsContainer.GetComponentsInChildren<MeshRenderer>();

			foreach (MeshRenderer renderer in meshRenderers)
			{
				renderer.sharedMaterial.SetVector("_ClippingMaskDome", maskVector);
			}
		}
	}
}