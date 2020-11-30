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
		UpdateDynamicCreatedInstancedMaterials();
		UpdateSpecificMaterials();
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
		maskVector.Set(pointerFollower.position.x, pointerFollower.position.y, pointerFollower.position.z, pointerFollower.localScale.x * maskScale);

		foreach (MeshRenderer renderer in meshRenderers)
		{
			renderer.sharedMaterial.SetVector("_ClippingMaskDome", maskVector);
		}
	}
}
