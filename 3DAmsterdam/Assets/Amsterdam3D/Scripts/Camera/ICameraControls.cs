using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

   public interface ICameraControls
    {
    float GetNormalizedCameraHeight();

    float GetCameraHeight();

    void MoveAndFocusOnLocation(Vector3 targetLocation, Quaternion rotation);





}

