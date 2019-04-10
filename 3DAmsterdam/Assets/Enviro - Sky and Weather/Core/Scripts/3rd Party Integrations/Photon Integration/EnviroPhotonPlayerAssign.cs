using System.Collections;
using System.Collections.Generic;
#if ENVIRO_PHOTON_SUPPORT
using Photon.Pun;
#endif
using UnityEngine;
#if ENVIRO_PHOTON_SUPPORT
[RequireComponent(typeof(PhotonView))]
#endif
public class EnviroPhotonPlayerAssign : MonoBehaviour {
#if ENVIRO_PHOTON_SUPPORT
    public PhotonView myView;

	void Start ()
    {
        if(myView == null)
           myView = GetComponent<PhotonView>();

        if (myView.IsMine && EnviroSkyMgr.instance != null)
        {
              EnviroSkyMgr.instance.AssignAndStart(gameObject, Camera.main); 
        }
    }
#endif
}
