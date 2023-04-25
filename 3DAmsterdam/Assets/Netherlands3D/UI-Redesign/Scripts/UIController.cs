using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{

    public Button myButton;
    public Label myLabel;

    // Start is called before the first frame update
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        myButton = root.Q<Button>("myButton");
        myLabel = root.Q<Label>("myLabel");

        myButton.clicked += MyButtonPressed;
    }

    void MyButtonPressed()
    {
        myLabel.text = "ay hello";
        myLabel.style.display = DisplayStyle.Flex;
    }

 
}
