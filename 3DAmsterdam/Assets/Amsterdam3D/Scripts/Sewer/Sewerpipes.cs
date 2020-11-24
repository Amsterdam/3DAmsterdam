using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace sewer
{
    public class Sewerpipes:MonoBehaviour
    {
        public GameObject sewerPipePrefab;

        // Start is called before the first frame update
        void Start()
        {
            CreateSewerpipe(new Vector2(0, 0), new Vector2(10, 10), 0, 2, 350, sewerPipePrefab);
            CreateSewerpipe(new Vector2(10, 10), new Vector2(10, 20), 2, 2, 350, sewerPipePrefab);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startpositionXZ">unity XZ-coordinates of startpoint</param>
        /// <param name="endpositionXZ">unity XZ-coordinates of endpoint</param>
        /// <param name="startHeight">unity elevation of startpoint (BOB)</param>
        /// <param name="endHeight">unity elevation of endpoint (BOB)</param>
        /// <param name="diameterMM">diameter in mm</param>
        /// <param name="sewerPipeTemplate">GameObject with pipeTemplate, default length=1, default diameter = 1</param>
        /// <returns>GameObject with sewerpipe</returns>
        public GameObject CreateSewerpipe(Vector2 startpositionXZ, Vector2 endpositionXZ, float startHeight, float endHeight, double diameterMM, GameObject sewerPipeTemplate)
        {
            GameObject newSewerPipe = Instantiate(sewerPipeTemplate);
            Transform sewerPipe = newSewerPipe.transform;

            // rotate pipe in horizontally
            float angle = Vector2.SignedAngle((endpositionXZ - startpositionXZ), new Vector2(10, 0));
            Debug.Log(angle);
            sewerPipe.Rotate(new Vector3(0, 1, 0), angle);

            //rotate pipe vertically
            float distanceXY = (endpositionXZ - startpositionXZ).magnitude;
            Vector2 startpoint = new Vector2(0, startHeight);
            Vector2 endpoint = new Vector2(distanceXY, endHeight);
            angle = Vector2.Angle(endpoint - startpoint, new Vector2(10, 0));
            sewerPipe.Rotate(new Vector3(0, 0, 1), angle);

            //scale pipe to correct length
            Vector3 startpoint3D = new Vector3(startpositionXZ.x, startHeight, startpositionXZ.y);
            Vector3 endpoint3D = new Vector3(endpositionXZ.x, endHeight, endpositionXZ.y);
            float requiredLength = (endpoint3D - startpoint3D).magnitude;
            Vector3 pipeScale = sewerPipe.transform.localScale;
            pipeScale.x *= requiredLength;
            sewerPipe.localScale = pipeScale;

            // scale pipe to correct diameter
            pipeScale = sewerPipe.transform.localScale;
            pipeScale.y *= (float)(diameterMM / 1000);
            pipeScale.z *= (float)(diameterMM / 1000);
            sewerPipe.localScale = pipeScale;

            //move pipe to correct location
            sewerPipe.position = new Vector3(startpositionXZ.x, startHeight, startpositionXZ.y) - new Vector3(0, (float)(0.4 * diameterMM / 1000), 0);
            return newSewerPipe;
        }
    }
}
