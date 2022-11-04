using Netherlands3D;
using Netherlands3D.TileSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLoadingSpinner : MonoBehaviour
{
    private TileHandler tileHandler;

    private void Awake()
    {
        tileHandler = FindObjectOfType<TileHandler>();
    }

    void LateUpdate()
    {
        OverallProgressIndicator.Show(tileHandler.pendingTileChanges.Count>0);
    }
}
