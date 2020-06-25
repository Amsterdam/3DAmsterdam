using Dummiesman;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class ObjStringLoader : MonoBehaviour
{
    public void LoadOBJFromString(string objText){
        var textStream = new MemoryStream(Encoding.UTF8.GetBytes(objText));
        GameObject loadedObj = new OBJLoader().Load(textStream);
    }
}
