using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Cameras;
using TMPro;

namespace Netherlands3D.Interface
{
    public class wijknaam : WorldPointFollower
    {
        public Vector3 position;
        public TextMeshPro textemesh; 
        public TextMesh textmesh;
        // Start is called before the first frame update
        public void Setup(string name, Vector3 worldposition)
        {
            position = worldposition;

            //textemesh.text = name;
            //textfield.text = name;
            AlignWithWorldPosition(worldposition);
        }

        //Update is called once per frame
        void Update()
        {
            transform.rotation = CameraModeChanger.Instance.ActiveCamera.transform.rotation;
            transform.Rotate(Vector3.up, 180f, Space.Self);
            //transform.LookAt(CameraModeChanger.Instance.ActiveCamera.transform);
        }
    }
}
