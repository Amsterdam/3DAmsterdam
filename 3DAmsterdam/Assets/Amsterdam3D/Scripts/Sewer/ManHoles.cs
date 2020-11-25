using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace sewer
{
    public class ManHoles : MonoBehaviour
    {
        public GameObject manHolePrefab;
        // Start is called before the first frame update
        void Start()
        {
            CreateManhole(new Vector3(0, 43, 0), manHolePrefab);
            CreateManhole(new Vector3(10, 45, 5), manHolePrefab);
        }

        public GameObject CreateManhole(Vector3 position, GameObject manHoleTemplate, float defaultHeight=1.50f)
        {
            
            Vector3 lidPosition = GetPositionAtSurface(position);
            float height =GetHeightAtPosition(lidPosition,defaultHeight);
            GameObject manHole = Instantiate(manHoleTemplate);
            manHole.transform.localPosition = lidPosition;
            Vector3 scale = manHole.transform.localScale;
            scale.y = height;
            manHole.transform.localScale = scale;

            return manHole;
        }

        private Vector3 GetPositionAtSurface(Vector3 position)
        {
            Vector3 foundPosition = new Vector3();
            RaycastHit hit;
            int layerMask = 1<<LayerMask.NameToLayer("Terrain");
            Vector3 rayCastPosition = position + new Vector3(0, 10, 0);

            if (Physics.Raycast(rayCastPosition, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
            {
                foundPosition = hit.point;
            }
            else
            {
                foundPosition = position;
            }
            return foundPosition;

        }

        private float GetHeightAtPosition(Vector3 position, float defaultHeight)
        {
            float height = defaultHeight;
            RaycastHit hit;
            int layerMask = 1<<LayerMask.NameToLayer("Sewer");
            if (Physics.Raycast(position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
            {
                height = position.y-hit.point.y;
            }

            return height;
        }
    }
}
