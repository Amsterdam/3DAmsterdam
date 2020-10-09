using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PremisesButton : MonoBehaviour
{
    public Button button;
    public Text buttonText;
    public Pand.Rootobject pandResult;
    public int premisesIndex;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ShowPremises);
    }

    public void Initiate(Pand.Rootobject pandObject, int index)
    {
        premisesIndex = index;
        pandResult = pandObject;
        buttonText.text = pandObject.results[premisesIndex]._display;
    }

    private void ShowPremises()
    {
        DisplayBAGData.Instance.loadingCirle.SetActive(true);
        DisplayBAGData.Instance.PlacePremises(pandResult, premisesIndex); // snelle data open methode maar hij checkt niet of de data al binnen is
        DisplayBAGData.Instance.buttonObjectTargetSpawn.gameObject.SetActive(false);
    }
}
