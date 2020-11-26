using UnityEngine;

namespace Amsterdam3D.Sewerage
{
    public class ManHoleSpawner : MonoBehaviour
    {
        public GameObject manHolePrefab;

        void Start()
        {
            CreateManhole(new Vector3(0, 43, 0), manHolePrefab);
            CreateManhole(new Vector3(10, 45, 5), manHolePrefab);
        }
        /// <summary>
        /// Create a Manhole (rioolput)
        /// uses TerrainHeight for elevation as default, uses y-value of param:position if no terrainheight found
        /// requiers sewerpipes to be present for accurate depth
        /// </summary>
        /// <param name="position">Unity-coordinates of the top-center of the manhole</param>
        /// <param name="manHoleTemplate">GameObject with manholeTemplate, default height=1, origin = top-center, local x-direction is down</param>
        /// <param name="defaultDepth">default manhole-depth if no sewerpipes are found</param>
        /// <returns></returns>
        public GameObject CreateManhole(Vector3 position, GameObject manHoleTemplate, float defaultDepth=1.50f)
        {
            // get top-center position
            Vector3 lidPosition = GetPositionAtSurface(position);
            // get manhole-height
            float depth =GetDepthAtPosition(lidPosition,defaultDepth);
            // create manhole
            GameObject manHole = Instantiate(manHoleTemplate);
            // move manhole
            manHole.transform.localPosition = lidPosition;
            // scale manhole to correct height
            Vector3 scale = manHole.transform.localScale;
            scale.y = depth;
            manHole.transform.localScale = scale;
            // return manhole
            return manHole;
        }
        /// <summary>
        /// get the position of top-center of the manhole, prioritize on terrain-layer
        /// </summary>
        /// <param name="position">Vector3 theoretical position of manhole according to WFS</param>
        /// <returns>Vector3 top-center position of the manhole</returns>
        private Vector3 GetPositionAtSurface(Vector3 position)
        {
            Vector3 foundPosition = new Vector3();

            // setup raycast
            RaycastHit hit;
            // set layerMask to terrain-only
            int layerMask = 1<<LayerMask.NameToLayer("Terrain");

            // set RaycastOrigin to 10 above theoretical manhole-position
            Vector3 rayCastPosition = position + new Vector3(0, 10, 0);

            if (Physics.Raycast(rayCastPosition, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
            {
                // set foundposition to hitpoint if terrain is found
                foundPosition = hit.point;
                // lift position a little bit for better visibility
                foundPosition += new Vector3(0, 0.02f, 0);
            }
            else
            {
                // keep theoretical position if terrain is not found
                foundPosition = position;
            }
            return foundPosition;

        }
        /// <summary>
        /// get the required depth of the manhole
        /// </summary>
        /// <param name="position">Vector3 top-center postion of manhole</param>
        /// <param name="defaultDepth">default depth if to sewerpipes are detected</param>
        /// <returns>required height of manhole</returns>
        private float GetDepthAtPosition(Vector3 position, float defaultDepth)
        {
            float height = defaultDepth;
            RaycastHit hit;
            int layerMask = 1<<LayerMask.NameToLayer("Sewer");
            if (Physics.Raycast(position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
            {
                // if sewerpipe found, depth is distance between top-center and hit.point plus 0.5m for bottom-part of manhole
                height = position.y-hit.point.y+0.5f;
            }

            return height;
        }
    }
}
