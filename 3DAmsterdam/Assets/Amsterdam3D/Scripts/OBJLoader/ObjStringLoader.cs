using Amsterdam3D.CameraMotion;
using Amsterdam3D.Interface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Amsterdam3D.UserLayers {
    public class ObjStringLoader : MonoBehaviour
    {
        [SerializeField]
        private Material defaultLoadedObjectsMaterial;

        [SerializeField]
        private PlaceCustomObject customObjectPlacer;

        private string objFileName = "model.obj";

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
                LoadOBJFromString(File.ReadAllText("C:/Projects/GemeenteAmsterdam/TestModels/wetransfer-73a599/KRZNoord_OBJ/Testgebied_3DAmsterdam.obj"));
        }
#endif
        public void SetOBJFileName(string fileName)
        {
            objFileName = fileName;
        }

        public void LoadOBJFromJavascript()
        {
            LoadOBJFromString(JavascriptMethodCaller.FetchOBJDataAsString());
        }

        public void LoadOBJFromString(string objText) {
            var newOBJ = new GameObject().AddComponent<ObjLoad>();
            newOBJ.SetGeometryData(objText);
            newOBJ.Build(defaultLoadedObjectsMaterial);
            newOBJ.name = objFileName;

            customObjectPlacer.AtPointer(newOBJ.gameObject);

            //hide panel after loading
            gameObject.SetActive(false);
        }
    }
}