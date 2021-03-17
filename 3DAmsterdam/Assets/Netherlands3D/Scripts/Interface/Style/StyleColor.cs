using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StyleColor : MonoBehaviour
{
    public enum ColorStyle{
        PRIMARY,
        SECONDARY
	}

    [SerializeField]
    private ColorStyle colorStyle = ColorStyle.PRIMARY; 

    void Start()
    {
        GetComponent<Graphic>().color = (colorStyle == ColorStyle.PRIMARY) ? Config.activeConfiguration.primaryColor : Config.activeConfiguration.secondaryColor;
    }
}
