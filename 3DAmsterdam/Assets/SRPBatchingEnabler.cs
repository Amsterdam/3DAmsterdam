using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SRPBatchingEnabler : MonoBehaviour
{
   
    public void EnableSRP(bool enable)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = enable;
    }

}
