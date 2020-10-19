using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterTrees : MonoBehaviour
{
    [SerializeField]
    private GameObject[] treeTypes;
    private Mesh sharedMesh;

    private int currentVert = 0;

    [SerializeField]
    private int spawnTreesPerFrame = 20;

    void Start()
    {
        sharedMesh = GetComponent<MeshFilter>().sharedMesh;
    }

    void Update()
	{
		SpawnTreeBatch();
	}

	private void SpawnTreeBatch()
	{
		for (int i = 0; i < spawnTreesPerFrame; i++)
		{
			if (currentVert >= sharedMesh.vertexCount)
			{
				Destroy(this);
			}
			else
			{
				Instantiate(treeTypes[Random.Range(0, treeTypes.Length)], this.transform).transform.localPosition = sharedMesh.vertices[currentVert];
				currentVert++;
			}
		}
	}

	private void OnDrawGizmos()
	{
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        foreach(Vector3 vert in mesh.vertices)
        {
            Gizmos.DrawSphere(this.transform.position + vert, 1.0f);
		}
	}
}
