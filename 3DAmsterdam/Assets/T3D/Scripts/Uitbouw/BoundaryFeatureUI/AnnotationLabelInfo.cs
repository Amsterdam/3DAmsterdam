using UnityEngine;
using UnityEngine.UI;

public class AnnotationLabelInfo : MonoBehaviour
{
    [SerializeField]
    private Text annotationNumber, annotationText;

    public void SetInfo(AnnotationUI ann)
    {
        annotationNumber.text = (ann.Id + 1).ToString();
        annotationText.text = ann.Text;
        //featureSize.text = FormatSize(ann.Size);
    }

    //private string FormatSize(Vector2 size)
    //{
    //    var x = Mathf.RoundToInt(size.x * 100f);
    //    var y = Mathf.RoundToInt(size.y * 100f);
    //    return x + " x " + y;
    //}
}
