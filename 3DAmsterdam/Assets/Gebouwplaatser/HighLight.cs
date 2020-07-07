using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighLight : MonoBehaviour
{
    Color[] originalColour;
    List<Transform> children;
    Material[] mats;

    void Start()
    {
        children = new List<Transform>();

        foreach (Transform child in transform)
        {
            children.Add(child);
        }
    }

    void OnMouseOver()
    {
        if (children.Count != 0)
        {
            for (int i = 0; i < children.Count; i++)
            {
                mats = children[i].gameObject.GetComponent<MeshRenderer>().materials;

                for (int j = 0; j < mats.Length; j++)
                {
                    //originalColour[j] = mats[j].color;
                    mats[j].color = Color.red;
                }
            }
        }
        else if (children.Count == 0)
        {
            mats = transform.gameObject.GetComponent<MeshRenderer>().materials;

            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].color = Color.red;
            }
        }
    }

    void OnMouseExit()
    {
        if (children.Count != 0)
        {
           for (int j = 0; j < mats.Length; j++)
           {
                mats[j].color = Color.white;
           }
        }
        else if (children.Count == 0)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].color = Color.white;
            }
        }
    }
}
