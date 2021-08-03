using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Cameras;


namespace Netherlands3D.Interface
{
    public class wijknaam : WorldPointFollower
    {
        public Vector3 position;
        public Text textfield;
        // Start is called before the first frame update
        public void Setup(string name, Vector3 worldposition)
        {
            position = worldposition;
            
            textfield.text = name;
            AlignWithWorldPosition(worldposition);
        }

        //Update is called once per frame
        void Update()
        {
            Vector3 camposition = CameraModeChanger.Instance.ActiveCamera.transform.position;
            float camHeight = camposition.y;
            float maxDistance = 5f * camHeight;
            if ((camposition-position).magnitude>maxDistance)
            {
                isEnabled = false;
            }
            else
            {
                isEnabled = true;
            }
            AutoHideByCamera();
        }
    }
}
