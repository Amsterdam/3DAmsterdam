using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Utilities;
using ConvertCoordinates;
using UnityEngine.UI;
using Netherlands3D.Interface;
using Netherlands3D;
using Netherlands3D.Cameras;

public class wijken : MonoBehaviour
{
    public string wijkenFilepath = "D:/3DAmsterdam/wijken_amsterdam.txt";
    public string buurtenFilepath = "D:/3DAmsterdam/buurten_amsterdam.txt";
    public GameObject wijkmarker;

    private List<wijknaam> wijknamen = new List<wijknaam>();
    private List<wijknaam> buurtnamen = new List<wijknaam>();
    private bool allOff = false;

    private string latstring = "\"lat\": ";
    private string lonstring = "\"lon\": ";
    private string namestring = "\"name\": ";
    private int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(readFile());
    }


    IEnumerator readFile()
    {
        var wijkenRequest = UnityWebRequest.Get(wijkenFilepath);

        yield return wijkenRequest.SendWebRequest();

        if (!wijkenRequest.isNetworkError && !wijkenRequest.isHttpError)
        {
            string wijkentext = wijkenRequest.downloadHandler.text;
            int nextIndex;
            int nameEndIndex;
            bool keepGoing = true;
            while (keepGoing)
            {
                nextIndex = wijkentext.IndexOf(latstring,index)+latstring.Length;
                if (nextIndex<index)
                {
                    keepGoing = false; break;
                }
                index = nextIndex;
                if (index==-1) { keepGoing = false; break; }
                double lat = StringManipulation.ParseNextDouble(wijkentext, ',', index, out index);
                index = wijkentext.IndexOf(lonstring,index)+lonstring.Length;
                if (index == -1) { keepGoing = false; break; }
                double lon = StringManipulation.ParseNextDouble(wijkentext, ',', index, out index);
                index = wijkentext.IndexOf(namestring,index)+namestring.Length+1;
                if (index == -1) { keepGoing = false; break; }
                nameEndIndex = wijkentext.IndexOf('"', index);
                string name = wijkentext.Substring(index, nameEndIndex - index);
                index = nameEndIndex;
                if (index <0) { keepGoing = false; break; }
                Vector3 coordinate = CoordConvert.WGS84toUnity(lon, lat);
                coordinate.y = Config.activeConfiguration.zeroGroundLevelY + 2.0f;

                GameObject go = Instantiate(wijkmarker, transform);
                //go.transform.position = coordinate;
                //go.GetComponentInChildren<Text>().text = name;
                go.GetComponentInChildren<TextMesh>().text=name;
                go.transform.position = coordinate;
                wijknamen.Add(go.GetComponent<wijknaam>());
                Debug.Log(index);

                yield return null;
            }
        }
            yield return null;


        var buurtenRequest = UnityWebRequest.Get(buurtenFilepath);
        yield return buurtenRequest.SendWebRequest();
        if (!buurtenRequest.isNetworkError && !buurtenRequest.isHttpError)
        {
            string wijkentext = buurtenRequest.downloadHandler.text;
            int nextIndex;
            int nameEndIndex;
            bool keepGoing = true;
            while (keepGoing)
            {
                nextIndex = wijkentext.IndexOf(latstring, index) + latstring.Length;
                if (nextIndex < index)
                {
                    keepGoing = false; break;
                }
                index = nextIndex;
                if (index == -1) { keepGoing = false; break; }
                double lat = StringManipulation.ParseNextDouble(wijkentext, ',', index, out index);
                index = wijkentext.IndexOf(lonstring, index) + lonstring.Length;
                if (index == -1) { keepGoing = false; break; }
                double lon = StringManipulation.ParseNextDouble(wijkentext, ',', index, out index);
                index = wijkentext.IndexOf(namestring, index) + namestring.Length + 1;
                if (index == -1) { keepGoing = false; break; }
                nameEndIndex = wijkentext.IndexOf('"', index);
                string name = wijkentext.Substring(index, nameEndIndex - index);
                index = nameEndIndex;
                if (index < 0) { keepGoing = false; break; }
                Vector3 coordinate = CoordConvert.WGS84toUnity(lon, lat);
                coordinate.y = Config.activeConfiguration.zeroGroundLevelY + 2.0f;

                GameObject go = Instantiate(wijkmarker, transform);
                //go.transform.position = coordinate;
                //go.GetComponentInChildren<Text>().text = name;
                go.GetComponentInChildren<TextMesh>().text = name;
                go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                go.transform.position = coordinate;
                buurtnamen.Add(go.GetComponent<wijknaam>());


                yield return null;
            }
        }
        yield return null;

    }

    // Update is called once per frame
    void Update()
    {
        Transform camTransform = CameraModeChanger.Instance.ActiveCamera.transform;
        Vector3 camposition = camTransform.position;
        camTransform.Rotate(Vector3.up, 180f, Space.Self);
        Quaternion camRotation = camTransform.rotation;
        camTransform.Rotate(Vector3.up, 180f, Space.Self);
        float camheight = camTransform.position.y;
        float camAngle = camTransform.localRotation.eulerAngles.x;
        float maxDistance=0;
        if (camAngle>45f) // looking down;

        {
            maxDistance = 2 * 2000;
        }
        else
        {
            maxDistance = 5 * camheight;
        }
        if (camheight<100)
        {
            maxDistance = 0;
        }
        Transform itemTransform;
        float characterSize = 1+Mathf.Max(camheight / 500,0);

            foreach (wijknaam item in wijknamen)
            {
            itemTransform = item.transform;
            if ((itemTransform.position - camposition).magnitude  <  maxDistance)
                {
                        item.gameObject.SetActive(true);
                }
            else
            {
                item.gameObject.SetActive(false);
            }
            itemTransform.rotation = camTransform.rotation;
                item.textmesh.characterSize = characterSize;
            }
        foreach (wijknaam item in buurtnamen)
        {
            itemTransform = item.transform;
            if ((itemTransform.position - camposition).magnitude < 0.5 * maxDistance)
            {
                item.gameObject.SetActive(true);

            }
            else
            {
                item.gameObject.SetActive(false);
            }

            itemTransform.rotation = camRotation;
            item.textmesh.characterSize = characterSize;
        }

        //}

    }


}
