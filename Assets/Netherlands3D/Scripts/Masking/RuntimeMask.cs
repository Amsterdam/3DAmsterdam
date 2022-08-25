using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Masking
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
		private bool allowInvert = false;

		[SerializeField]
		private bool updateRuntimeDynamicMaterials = false;

		[SerializeField]
		private Transform targetMaterialsContainer;

		[SerializeField]
		private Material[] specificMaterials;

		[SerializeField]
		private float maskScaleMultiplier = 1.0f;
		private Vector4 maskVector = default;

		public const string clipppingMaskPositionVector = "_ClippingMask";
		public const string clippingMaskTexture = "_MaskMap";
		public const string clippingMaskSize = "_Size";

		[SerializeField]
		private Vector2 maskSize;

		[SerializeField]
		private Texture2D maskTexture;

		public static RuntimeMask current;
		private RuntimeMask previousMask;

		private Bounds lastBounds;

		private MeshRenderer visual;

		public float MaskScaleMultiplier { get => maskScaleMultiplier; }

		private void Awake()
		{
			visual = GetComponent<MeshRenderer>();
			Clear();
		}

		//We enable shadows for the underground when we use the mask sphere, to give more depth
		private void OnEnable()
		{
			//We only allow one runtime mask atm. Make sure to enable any active one.
			if (current && current != this)
			{
				previousMask = current; //Save this, because we want to restore this one if we deactive this mask
				current.gameObject.SetActive(false);
			}
			current = this;

			if(lastBounds != null)
			{
				MoveToBounds(lastBounds);
			}
		}

		//We enable shadows for the underground when we use the mask sphere, to give more depth
		private void OnDisable()
		{
			//Enable previous masking object if it was active when we enabled this one
			if (previousMask)
			{
				current = previousMask;
				previousMask.gameObject.SetActive(true);
				previousMask.Clear();
			}
			else{
				current = null;
			}
			//make sure to reset mask shaders
			Clear();
		}

		public void Clear()
		{
			UpdateSpecificMaterials(true);
			UpdateDynamicCreatedInstancedMaterials(true);
		}

		public void FlipMask()
		{
			//Make sure we are working on an instance, not our source asset
			if (!allowInvert)
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

		public void MoveToBounds(Bounds bounds)
		{
			lastBounds = bounds;

			this.transform.position = bounds.center;
			this.transform.localScale = bounds.size * (1.0f + (2.0f / maskTexture.width)); //We use a margin of 1 pixel, so the white edge of our mask texture can be clamped
			ApplyNewPositionAndScale();

			gameObject.SetActive(true);
		}

		public void ApplyNewPositionAndScale()
		{
			CalculateMaskStencil();
			UpdateSpecificMaterials();
			UpdateDynamicCreatedInstancedMaterials();
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
				sharedMaterial.SetVector(clipppingMaskPositionVector, (resetToZero) ? Vector4.one * float.MaxValue : maskVector);
				sharedMaterial.SetTexture(clippingMaskTexture, (resetToZero) ? null : maskTexture);
				sharedMaterial.SetVector(clippingMaskSize, (resetToZero) ? Vector2.zero : maskSize);
			}
		}

		/// <summary>
		/// Optionally find runtime created materials that we cant predefine in our specificMaterials list
		/// </summary>
		private void UpdateDynamicCreatedInstancedMaterials(bool resetToZero = false)
		{
			if (!updateRuntimeDynamicMaterials || !targetMaterialsContainer) return;

			if (resetToZero)
			{
				maskVector = Vector4.one * float.MaxValue;
			}

			MeshRenderer[] meshRenderers = targetMaterialsContainer.GetComponentsInChildren<MeshRenderer>();

			foreach (MeshRenderer renderer in meshRenderers)
			{
				renderer.sharedMaterial.SetVector(clipppingMaskPositionVector, (resetToZero) ? Vector4.one * float.MaxValue : maskVector);
				renderer.sharedMaterial.SetTexture(clippingMaskTexture, (resetToZero) ? null : maskTexture);
				renderer.sharedMaterial.SetVector(clippingMaskSize, (resetToZero) ? Vector2.zero : maskSize);
			}
		}
	}
}
