using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Netherlands3D.Utilities;
using ConvertCoordinates;
using UnityEngine.UI;
using Netherlands3D.Interface;
using Netherlands3D;

public class wijken : MonoBehaviour
{
    public string filepath = "D:/3DAmsterdam/wijken_amsterdam.txt";
    public GameObject wijkmarker;

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
        var wijkenRequest = UnityWebRequest.Get(filepath);

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
                coordinate.y = Config.activeConfiguration.zeroGroundLevelY * 2.0f;

                GameObject go = Instantiate(wijkmarker, transform);
                //go.transform.position = coordinate;
                //go.GetComponentInChildren<Text>().text = name;
                go.GetComponent<wijknaam>().Setup(name, coordinate);

                Debug.Log(index);

                yield return null;
            }
        }
            yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
