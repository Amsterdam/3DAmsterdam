using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameObjectsGroup", order = 1)]
public class GameObjectsGroup : ScriptableObject
{
    public GameObject[] items;
}