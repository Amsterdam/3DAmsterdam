using Netherlands3D.Cameras;
using Netherlands3D.LayerSystem;
using System;
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

		[SerializeField]
		private Material[] rectangularOnlyMaterials;

		private float maskMultiplier = 1.0f;

		[SerializeField]
		private Vector4 maskVector = default;

		[SerializeField]
		private AssetbundleMeshLayer groundMeshLayer;

		private MaskShape maskShape = MaskShape.SPHERICAL;
		private MaskState maskState = MaskState.STATIC_TRANSFORM;

		private const string clipppingMaskPositionVector = "_ClippingMask";
		private const string clippingMaskTexture = "_MaskMap";
		private const string clippingMaskSize = "_Size";

		[SerializeField]
		private Vector2 maskSize;

		[SerializeField]
		private Texture2D[] maskTextures;
		private Texture2D maskTexture;

		[SerializeField]
		private MeshRenderer domeRenderer;

		public static RuntimeMask Instance;

		public MaskState State { get => maskState; private set => maskState = value; }

		private void Awake()
		{
			Instance = this;

			if (!maskTexture) ChangeMaskShape(maskShape);
			Clear();
			gameObject.SetActive(false);
		}

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
			Clear();
		}

		public void ChangeMaskShape(MaskShape shape)
		{
			maskShape = shape;
			switch (maskShape)
			{
				case MaskShape.RECTANGULAR:
					if (!maskTextures[1].name.Contains("Clone"))
					{
						//Make sure our rectangular image is used as an instance, because we want to manipulate it at runtime.
						maskTextures[1] = Instantiate(maskTextures[1]);
					}
					maskTexture = maskTextures[1];
					break;
				case MaskShape.SPHERICAL:
					maskTexture = maskTextures[0];
					break;
				default:
					break;
			}
			UpdateSpecificMaterials();
			UpdateDynamicCreatedInstancedMaterials();
		}

		public void MoveWithMouse()
		{
			State = MaskState.FOLLOW_MOUSE;
			domeRenderer.enabled = true;
			gameObject.SetActive(true);
		}

		public void Clear()
		{
			UpdateSpecificMaterials(true);
			UpdateDynamicCreatedInstancedMaterials(true);
		}

		public void FlipMask()
		{
			//Make sure we are working on an instance, not our source asset
			if (maskShape == MaskShape.SPHERICAL)
			{
				Debug.LogWarning("Inverting mask currently only allowed on rectangular (small 32x32 texture) mask");
				return;
			}

			//We invert the pixels ( single R channel black and white texture )
			//This might seem slow, but only happens once and after that we avoid having to do this in the shader
			var pixels = maskTexture.GetPixels();
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i].r = (pixels[i].r == 0) ? 1 : 0;
			}
			maskTexture.SetPixels(pixels);
			maskTexture.Apply();
			
		}

		public void MoveToBounds(Bounds bounds = default)
		{
			ChangeMaskShape(MaskShape.RECTANGULAR);

			State = MaskState.STATIC_TRANSFORM;
			domeRenderer.enabled = false;

			if (bounds != default)
			{
				this.transform.position = bounds.center;
				this.transform.localScale = bounds.size * (1.0f + (2.0f / maskTexture.width)); //We use a margin of 1 pixel, so the white edge of our mask texture can be clamped
			}
			CalculateMaskStencil();
			UpdateSpecificMaterials();
			UpdateDynamicCreatedInstancedMaterials();

			gameObject.SetActive(true);
		}

		void Update()
		{
			if (CameraModeChanger.Instance.CameraMode != CameraMode.GodView) return;

			if (State == MaskState.FOLLOW_MOUSE)
			{
				//Continious update for moving camera/mouse
				MoveMaskWithPointer();
				CalculateMaskStencil();
				UpdateSpecificMaterials();
				UpdateDynamicCreatedInstancedMaterials();
			}
		}

		private void MoveMaskWithPointer()
		{
			transform.position = CameraModeChanger.Instance.CurrentCameraControls.GetMousePositionInWorld();
			transform.transform.localScale = Vector3.one * maskMultiplier * CameraModeChanger.Instance.ActiveCamera.transform.position.y;
		}

		private void CalculateMaskStencil()
		{
			maskSize = new Vector2(1.0f / transform.transform.localScale.x, 1.0f / transform.transform.localScale.z);
			maskVector.Set(
				(-transform.position.x / transform.transform.localScale.x) + 0.5f,
				0,
				(-transform.position.z / transform.transform.localScale.z) + 0.5f,
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
				sharedMaterial.SetTexture(clippingMaskTexture, (resetToZero) ? null : maskTexture);
				sharedMaterial.SetVector(clippingMaskSize, maskSize);
			}

			if(maskShape == MaskShape.RECTANGULAR)
			{
				foreach(Material sharedMaterial in rectangularOnlyMaterials)
				{
					sharedMaterial.SetVector(clipppingMaskPositionVector, (resetToZero) ? Vector4.zero : maskVector);
					sharedMaterial.SetTexture(clippingMaskTexture, (resetToZero) ? null : maskTexture);
					sharedMaterial.SetVector(clippingMaskSize, maskSize);
				}
			}
		}

		/// <summary>
		/// Optionally find runtime created materials that we cant predefine in our specificMaterials list
		/// </summary>
		private void UpdateDynamicCreatedInstancedMaterials(bool resetToZero = false)
		{
			if (!updateRuntimeDynamicMaterials || !targetMaterialsContainer) return;

			MeshRenderer[] meshRenderers = targetMaterialsContainer.GetComponentsInChildren<MeshRenderer>();

			foreach (MeshRenderer renderer in meshRenderers)
			{
				renderer.sharedMaterial.SetVector(clipppingMaskPositionVector, (resetToZero) ? Vector4.zero : maskVector);
				renderer.sharedMaterial.SetTexture(clippingMaskTexture, (resetToZero) ? null : maskTexture);
				renderer.sharedMaterial.SetVector(clippingMaskSize, maskSize);
			}
		}
	}
}