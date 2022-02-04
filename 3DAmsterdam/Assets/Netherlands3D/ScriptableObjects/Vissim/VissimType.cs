
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "VissimType", menuName = "Vissim/VissimType", order = 1)]
public class VissimType : ScriptableObject
{
    public new string name;
    public Sprite typeImage;
    public GameObject[] vissimTypeAssets;
}