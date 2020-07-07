using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ButtonExplainer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool mouseEnter = false;

    public string message;

    GameObject helpMessage;
    private GameObject _helpMessage;

    private Vector3 imagePos;
    private Vector3 position;

    private float offSetX, offSetY;
    private float scaling = 15f;

    public float offSet;

    private void Start()
    {
        // de UI van het bericht wordt ingeladen
        helpMessage = (GameObject) Resources.Load("ButtonHelp");

        // de positie wordt geplaatst op die van de knop
        imagePos = transform.position;
        
        // het bericht wordt geinstantieerd
        _helpMessage = Instantiate(helpMessage, imagePos, Quaternion.identity);

        // het bericht wordt child gemaakt van de bijbehorende knop
        _helpMessage.transform.SetParent(this.gameObject.transform);

        // de text in het bericht wordt aangepast
        _helpMessage.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = message;

        // de grootte van de image wordt bepaald
        RectTransform imageRectTransform = _helpMessage.GetComponent<RectTransform>();
        imageRectTransform.sizeDelta = new Vector2(message.Length * scaling, imageRectTransform.sizeDelta.y);

        // de grootte van de tekst wordt bepaald
        RectTransform textRectTransform = _helpMessage.transform.GetChild(0).gameObject.GetComponent<RectTransform>();
        textRectTransform.sizeDelta = new Vector2(message.Length * scaling, textRectTransform.sizeDelta.y);

        // de offset van het bericht wordt bepaald
        offSetX = GetComponent<RectTransform>().rect.width / 2;
        offSetX += (_helpMessage.GetComponent<RectTransform>().rect.width / 2) + offSet;
        offSetY = GetComponent<RectTransform>().rect.height / 2;

        _helpMessage.transform.position += new Vector3(offSetX, offSetY, 0);
    }

    private void Update()
    {
        if (mouseEnter)
        {
            _helpMessage.SetActive(true);
        } else
        {
            _helpMessage.SetActive(false);
        }
    }

    // als de muis over de knop beweegt komt het bericht tevoorschijn
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEnter = true;
    }

    // als de muis de knop verlaat verdwijnt het bericht
    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEnter = false;
    }
}
