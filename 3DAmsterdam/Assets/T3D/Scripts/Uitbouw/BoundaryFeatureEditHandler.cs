using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Cameras;
using Netherlands3D.Interface;
using UnityEngine;

namespace Netherlands3D.T3D.Uitbouw.BoundaryFeatures
{
    public class BoundaryFeatureEditHandler : MonoBehaviour
    {
        //private List<BoundaryFeature> features = new List<BoundaryFeature>();
        public BoundaryFeature ActiveFeature { get; private set; }

        public void SelectFeature(BoundaryFeature feature)
        {
            if (ActiveFeature)
                ActiveFeature.SetDistanceMeasurementsActive(EditMode.None);
            feature.SetDistanceMeasurementsActive(EditMode.Reposition);
            ActiveFeature = feature;
        }

        private void Update()
        {
            Ray ray = CameraModeChanger.Instance.ActiveCamera.ScreenPointToRay(Input.mousePosition);
            LayerMask mask = LayerMask.GetMask("StencilMask");
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out var hit, Mathf.Infinity, mask))
            {
                print(hit.collider.gameObject.name);
                var bf = hit.collider.GetComponent<BoundaryFeature>();
                if (bf)
                {
                    SelectFeature(bf);
                }
            }
        }
    }
}
