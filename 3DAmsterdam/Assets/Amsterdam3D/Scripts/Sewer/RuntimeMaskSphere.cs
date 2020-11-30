using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeMaskSphere : MonoBehaviour
{
    [SerializeField]
    private Transform targetMaterialsContainer;

    [SerializeField]
    private Material[] specificMaterials;

    [SerializeField]
    private Transform pointerFollower;

    [SerializeField]
    private float maskScale = 2.0f;

	Vector4 maskVector = default;


	private void OnEnable()
	{
        targetMaterialsContainer.GetComponent<TileLoader>()?.EnableShadows(true);
    }

    private void OnDisable()
    {
        targetMaterialsContainer.GetComponent<TileLoader>()?.EnableShadows(false);
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
			(CameraModeChanger.Instance.ActiveCamera.transform.position.y > pointerFollower.position.y) ?
			pointerFollower.transform.localScale.x * 0.47f : 0.0f
		);

		//Hide mask object if we are underground
		pointerFollower.gameObject.SetActive(maskVector.z != 0.0f);
	}

	private void UpdateSpecificMaterials()
	{
		foreach (Material sharedMaterial in specificMaterials)
		{
			sharedMaterial.SetVector("_ClippingMaskDome", maskVector);
		}
	}

	private void UpdateDynamicCreatedInstancedMaterials()
	{
		MeshRenderer[] meshRenderers = targetMaterialsContainer.GetComponentsInChildren<MeshRenderer>();

		foreach (MeshRenderer renderer in meshRenderers)
		{
			renderer.sharedMaterial.SetVector("_ClippingMaskDome", maskVector);
		}
	}
}
