using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupInfo : MonoBehaviour
{
    private RectTransform rectTransform;
    private Text text;
    [SerializeField]
    private Vector2 margin;
    [SerializeField]
    private Vector2 padding;
    [SerializeField]
    private float width = 400f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        text = GetComponentInChildren<Text>();
    }

    public void SetText(string newText)
    {
        TextGenerator textGen = new TextGenerator();
        TextGenerationSettings generationSettings = text.GetGenerationSettings(text.rectTransform.rect.size);
        //float width = textGen.GetPreferredWidth(newText, generationSettings); //width is fixed, and the height is calculated based on the width
        float height = textGen.GetPreferredHeight(newText, generationSettings);

        text.text = newText;
        rectTransform.sizeDelta = new Vector2(width + margin.x + padding.x, height + margin.y + padding.y);
    }
}
