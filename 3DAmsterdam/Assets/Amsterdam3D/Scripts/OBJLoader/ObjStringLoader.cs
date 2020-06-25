using Amsterdam3D.CameraMotion;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Amsterdam3D.UserLayers {
    public class ObjStringLoader : MonoBehaviour
    {
        private Pointer pointer;

        [SerializeField]
        private Material defaultLoadedObjectsMaterial;

        private void Start()
        {
            pointer = FindObjectOfType<Pointer>();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
                LoadOBJFromString(File.ReadAllText("C:/Users/Sam/Desktop/untitled.obj"));
        }
#endif

        public void LoadOBJFromString(string objText) {
            var newOBJ = new GameObject().AddComponent<OBJ>();
            newOBJ.SetGeometryData(objText);
            newOBJ.Build(defaultLoadedObjectsMaterial);
            newOBJ.transform.position = pointer.WorldPosition;

            gameObject.SetActive(false);
        }
    }
}