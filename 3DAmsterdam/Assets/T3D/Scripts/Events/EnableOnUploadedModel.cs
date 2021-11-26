using Netherlands3D.T3D.Uitbouw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableOnUploadedModel : MonoBehaviour
{
    
    void Start()
    {       
            gameObject.SetActive(MetadataLoader.Instance.UploadedModel);        
    }

    
}
