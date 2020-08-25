using Amsterdam3D.CameraMotion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Pointer : WorldPointFollower
{
    private Image pointerImage;

    [SerializeField]
    private float fadeOutTime = 0.3f;

    void Start()
    {
        CameraControls.focusPointChanged += AlignWithWorldPosition;
        pointerImage = GetComponent<Image>();
    }

    public void Show()
    {
        StopAllCoroutines();
        pointerImage.color = Color.white;
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutImage());
    }

    IEnumerator FadeOutImage()
    {
        var elapsedTime = 0.0f;
        while (elapsedTime < fadeOutTime)
        {
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            var newColor = pointerImage.color;
            newColor.a = Mathf.Clamp01(elapsedTime / fadeOutTime);
            pointerImage.color = newColor;
        }
    }
}
