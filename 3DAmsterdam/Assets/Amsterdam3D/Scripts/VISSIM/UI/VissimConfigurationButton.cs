using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VissimConfigurationButton : MonoBehaviour
{
    private Amsterdam3D.Interface.VissimConfiguration vissimConfiguration = default;
    private Button btn = default;
    // Start is called before the first frame update
    void Start()
    {
        vissimConfiguration = FindObjectOfType<Amsterdam3D.Interface.VissimConfiguration>();
        btn = GetComponent<Button>();
        btn.onClick.AddListener(vissimConfiguration.SetVehicles);
    }
}
