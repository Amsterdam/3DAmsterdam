using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netherlands3D.TileSystem;
using UnityEngine;

public class HideBuildingsOnEnable : MonoBehaviour
{
    private SelectSubObjects selectSubObjects;

    [SerializeField] private List<string> hiddenIDs = new();
    [SerializeField] private List<string> tilesWithInteractedSubObjects = new();
    private bool initialised = false;

    void Awake()
    {
        selectSubObjects = FindObjectOfType<SelectSubObjects>();
    }

    private void Start() {
        initialised = true;
    }

    private void OnEnable() {
        if(!initialised)
            return;

        if(selectSubObjects != null){
            //Append the hiddenIDs to the current lists
            selectSubObjects.TilesWithInteractedSubObjects = selectSubObjects.TilesWithInteractedSubObjects.Union(tilesWithInteractedSubObjects).ToList();
            selectSubObjects.HiddenIDs = selectSubObjects.HiddenIDs.Union(hiddenIDs).ToList();
        }

        selectSubObjects.UpdateHiddenListToChildren(true);
    }

    [ContextMenu("GetHiddenIDs")]
    public void GetListFromCurrentHiddenIDs()
    {
        if(selectSubObjects != null){
            hiddenIDs = selectSubObjects.HiddenIDs;
            tilesWithInteractedSubObjects = selectSubObjects.TilesWithInteractedSubObjects;
        }
    }
    
}
