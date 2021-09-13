using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualStatOverlay : MonoBehaviour
{
    [SerializeField]
    private Text text;

    [SerializeField]
    private Image backgroundImage;

    private RectTransform followParent;

    public void SetParameters(RectTransform parent, string percentageOfTotal, string visitsWithThisEvent)
    {
        followParent = parent;

        //We multiply this by two, because every event also send a Performance event which we want to ignore in % count
        float percentageOfTotalFloat = float.Parse(percentageOfTotal) * 2;
        int visitsWithThisEventNumber = int.Parse(visitsWithThisEvent);
        text.text = $"{visitsWithThisEventNumber}%, {visitsWithThisEvent}";

        float colorLerp = percentageOfTotalFloat / 100.0f;
        Color bgColor = Color.blue;
        if (colorLerp < 0.5f){
            bgColor = Color.Lerp(Color.blue, Color.green, colorLerp / 0.5f);
        }
        else{
            bgColor = Color.Lerp(Color.green, Color.red, (colorLerp- 0.5f) / 0.5f);
        }
        backgroundImage.color = bgColor;
    }

    private void Update()
    {
        this.transform.localScale = followParent.gameObject.activeInHierarchy ? Vector3.one:Vector3.zero;
        this.transform.position = followParent.TransformPoint((followParent.transform as RectTransform).rect.center);
    }
}
