using Netherlands3D.Cameras;
using Netherlands3D.LayerSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Underground
{
	public class RuntimeMask : MonoBehaviour
	{
		public enum MaskShape
		{
			RECTANGULAR,
			SPHERICAL
		}

		public enum MaskState
		{
			FOLLOW_MOUSE,
			STATIC_TRANSFORM
		}

		[SerializeField]
		private bool updateRuntimeDynamicMaterials = false;

		[SerializeField]
		private Transform targetMaterialsContainer;

		[SerializeField]
		private Material[] specificMaterials;

		private float maskMultiplier = 1.0f;

		[SerializeField]
		private Vector4 maskVector = default;

		[SerializeField]
		private AssetbundleMeshLayer groundMeshLayer;

		private MaskShape maskShape = MaskShape.RECTANGULAR;
		private MaskState maskState = MaskState.FOLLOW_MOUSE;

		private const string clipppingMaskPositionVector = "_ClippingMask";
		private const string clippingMaskTexture = "_MaskMap";
		private const string clippingMaskSize = "_Size";

		[SerializeField]
		private Vector2 maskSize;

		[SerializeField]
		private Vector2 offSet;

		[SerializeField]
		private Texture2D[] maskTextures;

		private Texture2D[] maskTexture;

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

			if()
			MoveMaskWithPointer();

			CalculateMaskStencil();
			UpdateSpecificMaterials();

			if (!updateRuntimeDynamicMaterials) return;
			UpdateDynamicCreatedInstancedMaterials();
		}

		private void MoveMaskWithPointer()
		{
			transform.position = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
			transform.transform.localScale = Vector3.one * maskMultiplier * CameraModeChanger.Instance.ActiveCamera.transform.position.y;
		}

		private void CalculateMaskStencil()
		{
			maskSize = new Vector2(1.0f / transform.transform.localScale.x, 1.0f / transform.transform.localScale.y);
			maskVector.Set(
				(-transform.position.x / transform.transform.localScale.x) + 0.5f,
				0,
				(-transform.position.z / transform.transform.localScale.y) + 0.5f,
				0);
		}

		/// <summary>
		/// Update specific shared materials that use the clipping mask subgraph
		/// </summary>
		/// <param name="resetToZero">Set all mask parameters to zero to make sure our editor does not change the materials</param>
		private void UpdateSpecificMaterials(bool resetToZero = false)
		{
			foreach (Material sharedMaterial in specificMaterials)
			{
				sharedMaterial.SetVector(clipppingMaskPositionVector, (resetToZero) ? Vector4.zero : maskVector);
				sharedMaterial.SetTexture(clippingMaskTexture, maskTexture);
				sharedMaterial.SetVector(clippingMaskSize, maskSize);
			}
		}

		/// <summary>
		/// Optionally find runtime created materials that we cant predefine in our specificMaterials list
		/// </summary>
		private void UpdateDynamicCreatedInstancedMaterials()
		{
			MeshRenderer[] meshRenderers = targetMaterialsContainer.GetComponentsInChildren<MeshRenderer>();

			foreach (MeshRenderer renderer in meshRenderers)
			{
				renderer.sharedMaterial.SetTexture(clippingMaskTexture, maskTexture);
				renderer.sharedMaterial.SetVector(clipppingMaskPositionVector, maskVector);
				renderer.sharedMaterial.SetVector(clippingMaskSize, maskSize);
			}
		}
	}
}