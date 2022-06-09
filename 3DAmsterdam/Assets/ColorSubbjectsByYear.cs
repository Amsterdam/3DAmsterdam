using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSubbjectsByYear : MonoBehaviour
{
    [SerializeField]
    private ObjectEvent gotIdsAndYears;

    private void Awake()
    {
        gotIdsAndYears.started.AddListener(GotIdsAndYears);
    }

    private void GotIdsAndYears(object idsAndYears)
    {
        throw new NotImplementedException();
    }

    void Start()
    {
        
    }

    
    public void ApplyYearFromDateTime(DateTime dateTime)
    {
        
    }
}
