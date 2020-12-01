using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeepInScreenBounds : MonoBehaviour
{
    private Image mainImage;
    private Vector3 limitedPosition;

	private void Awake()
	{
        mainImage = GetComponent<Image>();
    }

	void OnEnable()
    {
        limitedPosition = new Vector3(
            Mathf.Clamp(this.transform.position.x, 0 , Screen.width - this.mainImage.rectTransform.sizeDelta.x),
            Mathf.Clamp(this.transform.position.y, this.mainImage.rectTransform.sizeDelta.y, Screen.height),
            0
        );

        this.transform.position = limitedPosition;
    }
}
