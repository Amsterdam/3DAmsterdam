//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.EventSystems;

//public class SelectBuilding : MonoBehaviour
//{
//    Ray ray;
//    RaycastHit hit;

//    List<Transform> children;

//    Material[] mats;
//    Material[] parentMats;

//    List<Color> originalColours;

//    public GameObject panel;

//    GameObject selectedObj;

//    public MenuFunctions menuFunctions;

//    List<GameObject> dataFields;

//    public GameObject infoMenu;

//    public GetBagData api;
//    string BagID;

//    public GameObject bagData;


//    public GetObjectname martijnScript;

//    public void Start()
//    {
//        children = new List<Transform>();
//        originalColours = new List<Color>();
//        dataFields= new List<GameObject>();

//        menuFunctions.GetComponent<MenuFunctions>();
//    }

//    //public void FixedUpdate()
//    //{
//    //    ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//    //    SelectHoogbouw();
//    //}

//    public void SelectHoogbouw()
//    {
//        for (int i = 0; i < dataFields.Count; i++)
//        {
//            Destroy(dataFields[i]);
//        }

//        dataFields.Clear();

//        for (int i = 0; i < martijnScript.Bagids.Count; i++)
//        {
//            GameObject instantiateData = Instantiate(bagData, Vector3.zero, Quaternion.identity, panel.transform);
    
//            instantiateData.SetActive(true);

//            instantiateData.GetComponent<GetBagData>().Begin(martijnScript.o);

//            dataFields.Add(instantiateData);
//        }
        

//        //api.dropDown.ClearOptions();

//        //if (Input.GetMouseButtonDown(0))
//        //{
//        //menuFunctions.SelectMenu(4);



//        //if (selectedObj != null)
//        //{
//        //    if (parentMats.Length != 0)
//        //    {
//        //        for (int i = 0; i < parentMats.Length; i++)
//        //        {
//        //            parentMats[i].color = originalParentColors[i];
//        //        }
//        //    }

//        //    for (int i = 0; i < children.Count; i++)
//        //    {
//        //        mats = children[i].gameObject.GetComponent<Renderer>().materials;

//        //        for (int j = 0; j < mats.Length; j++)
//        //        {
//        //            if (mats.Length > 0)
//        //            {
//        //                mats[j].color = originalColours[j];
//        //            }
//        //        }
//        //    }
//        //}

//        //selectedObj = hit.transform.gameObject;

//        //parentMats = hit.transform.gameObject.GetComponent<Renderer>().materials;

//        //foreach (Transform child in hit.transform)
//        //{
//        //    children.Add(child);
//        //}

//        //for (int i = 0; i < parentMats.Length; i++)
//        //{
//        //    originalParentColors.Add(parentMats[i].color);
//        //    parentMats[i].color = Color.red;
//        //}

//        //for (int i = 0; i < children.Count; i++)
//        //{
//        //    mats = children[i].gameObject.GetComponent<Renderer>().materials;

//        //    for (int j = 0; j < mats.Length; j++)
//        //    {
//        //        if (mats.Length > 0)
//        //        {
//        //            originalColours.Add(mats[j].color);
//        //            mats[j].color = Color.red;
//        //        }
//        //    }
//        //}
//        //}


//        //if (hit.transform.gameObject != selectedObj && hit.transform.gameObject != EventSystem.current.IsPointerOverGameObject() && menuFunctions.currentMenu == 3)
//        //{
//        //    if (Input.GetMouseButtonDown(0))
//        //    {
//        //        StopCoroutine(api.OnResponse(api.request));
//        //        StopCoroutine(api.AdressLoader(api.verblijfRequest));
//        //        menuFunctions.SelectMenu(4);

//        //        if (parentMats.Length != 0)
//        //        {
//        //            for (int i = 0; i < parentMats.Length; i++)
//        //            {
//        //                parentMats[i].color = originalParentColors[i];
//        //            }
//        //        }

//        //        for (int i = 0; i < children.Count; i++)
//        //        {
//        //            mats = children[i].gameObject.GetComponent<Renderer>().materials;

//        //            for (int j = 0; j < mats.Length; j++)
//        //            {
//        //                if (mats.Length > 0)
//        //                {
//        //                    mats[j].color = originalColours[j];
//        //                }
//        //            }
//        //        }
//        //    }
//        //}
//    }
//}

