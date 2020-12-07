using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Amsterdam3D.Interface;

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
    /// <summary>
    /// Sets all text for the button
    /// </summary>
    /// <param name="pandObject"></param>
    /// <param name="index"></param>
    public void Initiate(Pand.Rootobject pandObject, int index)
    {
        premisesIndex = index;
        pandResult = pandObject;
        buttonText.text = pandObject.results[premisesIndex]._display;
    }
    /// <summary>
    /// Starts a script taht displays the current chosen Premises
    /// </summary>
    private void ShowPremises()
    {
        DisplayBAGData.Instance.loadingCirle.SetActive(true);
        DisplayBAGData.Instance.PlacePremises(pandResult, premisesIndex); // Quick method but doesn't check if all the data is there (It loads pretty fast)
        DisplayBAGData.Instance.buttonObjectTargetSpawn.gameObject.SetActive(false);
    }
}
