using UnityEngine;
using UnityEngine.UI;

public class AnnotationLabelInfo : MonoBehaviour
{
    [SerializeField]
    private Text annotationNumber, annotationText;

    public void SetInfo(AnnotationUI ann)
    {
        annotationNumber.text = "Annotatie " + (ann.Id + 1).ToString();
        annotationText.text = ann.Text;

        //annotationText.rectTransform.sizeDelta = new Vector2(annotationText.rectTransform.sizeDelta.x, annotationText.preferredHeight);
    }
}
