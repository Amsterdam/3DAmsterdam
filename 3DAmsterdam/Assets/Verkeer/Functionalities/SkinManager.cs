using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public Mesh[] skins;
    public Transform[] bodies;
    private SkinnedMeshRenderer render;
    private int skinSelect;
    private int bodySelect;

    public void Start()
    {
        render = GetComponentInChildren<SkinnedMeshRenderer>();

        Randomize();
    }

    public void Randomize()
    {
        for (int i = 0; i < skins.Length; i++)
        {
            for (int j = 0; j < bodies.Length; j++)
            {
                skinSelect = Random.Range(0, skins.Length);
                bodySelect = Random.Range(0, bodies.Length);

                render.sharedMesh = skins[skinSelect];
                render.rootBone = bodies[bodySelect];

                render.material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            }
        }
    }

}
