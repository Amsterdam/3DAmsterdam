using Netherlands3D.Events;
using Netherlands3D.TileSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHandlerProgress : MonoBehaviour
{
    [SerializeField]
    private GameObjectEvent tileHandlerWorking;

    [SerializeField]
    private GameObjectEvent tileHandlerDoneWorking;

    private bool tileHandlerBusy = false;

    void Update()
    {
        if(!tileHandlerBusy)
        {
            if (TileHandler.runningTileDataRequests > 0)
            {
                tileHandlerBusy = true;
                tileHandlerWorking.started.Invoke(gameObject);
            }
        }
        else
        {
            if (TileHandler.runningTileDataRequests == 0)
            {
                tileHandlerBusy = false;
                tileHandlerDoneWorking.started.Invoke(gameObject);
            }
        }
    }
}
