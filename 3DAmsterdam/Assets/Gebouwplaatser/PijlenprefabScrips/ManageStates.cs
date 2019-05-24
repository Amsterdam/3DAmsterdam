//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//public class ManageStates : MonoBehaviour {

//    private static ManageStates _instance;

//    [HideInInspector]
//    public string selectionState;

//    public static ManageStates Instance
//    {
//        get
//        {
//            if (_instance == null)
//            {
//                GameObject createInstance = new GameObject("StateManager");
//                createInstance.transform.parent = GameObject.Find("PijlenPrefab(Clone)").transform; // maakt object child van pijlenprefab
//                createInstance.AddComponent<ManageStates>();
//            }

//            return _instance;
//        }
//    }

//    void Awake()
//    {
//        _instance = this;
//    }


//    // -------functionality---------
//    [HideInInspector]
//    public float genericSpeed;

//    private string stateDescription;

//    //private TextMeshProUGUI text;
//    //private TextMeshProUGUI position;
//    private List<GameObject> arrows;
//    private GameObject shiftButton;

//    void Start()
//    {
//        //text = GameObject.Find("StateIndicator").GetComponent<TextMeshProUGUI>();
//        //position = GameObject.Find("Position").GetComponent<TextMeshProUGUI>();

//        arrows = new List<GameObject>();
//        arrows.AddRange(new List<GameObject> {GameObject.Find("UpArrow"), GameObject.Find("DownArrow"),
//                        GameObject.Find("LeftArrow"), GameObject.Find("RightArrow")});
//        shiftButton = GameObject.Find("ShiftButton");
//    }

//    void Update()
//    {
//        // als er niks geselecteerd is dan is de state idle.
//        if (GameObject.Find(selectionState) == null) selectionState = "Idle";

//        // veranderd kleur van de selected state naar geel. Geldt niet voor 'scaling' en 'Y' want dit is gedaan met een knop.
//        if (selectionState != "Scaling" && selectionState != "Idle" && selectionState != "Y")
//        {
//            GameObject.Find(selectionState).GetComponent<Renderer>().material.color = Color.yellow;
//        }

//        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) genericSpeed = 2.0f;
//        else genericSpeed = 1.0f;

//        StateMessage();
//        PositionObject();
//        ChangeColor();
//    }


//    // laat de huidige state in de canvas zien. (scaling, moving of rotating)
//    void StateMessage()
//    {
//        if (ManageStates.Instance.selectionState == "Scaling") stateDescription = "Scaling";
//        else if (ManageStates.Instance.selectionState == "X" || ManageStates.Instance.selectionState == "Y" ||
//                 ManageStates.Instance.selectionState == "Z" || ManageStates.Instance.selectionState == "XZ")
//            stateDescription = "Moving";
//        else if (ManageStates.Instance.selectionState == "Ry") stateDescription = "Rotating";
//        else stateDescription = "Idle";

//        //text.text = "State: " + stateDescription;
//    }

//    // laat de positie van het object (in world space) in de canvas zien.
//    void PositionObject()
//    {
//        //position.text = transform.parent.parent.position.ToString();
//    }

//    // verandert de kleur van de indicatiepijlen die in de canvas wordt laten zien.
//    void ChangeColor()
//    {
//    //    if (ManageStates.Instance.selectionState == "Scaling" || ManageStates.Instance.selectionState == "Z" ||
//    //        ManageStates.Instance.selectionState == "Y")
//    //    {
//    //        arrows[0].GetComponent<Image>().color = Color.yellow; arrows[1].GetComponent<Image>().color = Color.yellow;
//    //        shiftButton.GetComponent<Image>().color = Color.yellow;
//    //    }
//    //    else
//    //    {
//    //        arrows[0].GetComponent<Image>().color = Color.white; arrows[1].GetComponent<Image>().color = Color.white;
//    //    }

//    //    if (ManageStates.Instance.selectionState == "Ry" || ManageStates.Instance.selectionState == "X")
//    //    {
//    //        arrows[2].GetComponent<Image>().color = Color.yellow; arrows[3].GetComponent<Image>().color = Color.yellow;
//    //        shiftButton.GetComponent<Image>().color = Color.yellow;
//    //    }
//    //    else
//    //    {
//    //        arrows[2].GetComponent<Image>().color = Color.white; arrows[3].GetComponent<Image>().color = Color.white;
//    //    }

//    //    if (ManageStates.Instance.selectionState == "XZ")
//    //    {
//    //        arrows[0].GetComponent<Image>().color = Color.yellow; arrows[1].GetComponent<Image>().color = Color.yellow;
//    //        arrows[2].GetComponent<Image>().color = Color.yellow; arrows[3].GetComponent<Image>().color = Color.yellow;
//    //        shiftButton.GetComponent<Image>().color = Color.yellow;
//    //    }
//    }
//}
