using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PandButton : MonoBehaviour
{
    public Button btn;
    public Text btnText;
    public Pand.Rootobject pandResult;
    public int pandIndex;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(ShowPand);
    }

    public void Initiate(Pand.Rootobject pandObject, int index)
    {
        pandIndex = index;
        pandResult = pandObject;
        btnText.text = pandObject.results[pandIndex]._display;
    }

    private void ShowPand()
    {
        DisplayBAGData.Instance.loadingCirle.SetActive(true);
        DisplayBAGData.Instance.PlacePand(pandResult, pandIndex); // snelle data open methode maar hij checkt niet of de data al binnen is
        //StartCoroutine(DisplayBAGData.Instance.PlaceCoroutine(pandResult, pandIndex)); // indien hij de data niet goed ophaalt, roep dit script aan, deze wacht tot de data eerst binnen is
        DisplayBAGData.Instance.buttonObjectTargetSpawn.gameObject.SetActive(false);
    }
}
