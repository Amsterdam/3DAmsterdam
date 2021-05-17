using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

public interface ICameraExtents
{
    Extent GetExtent();
    Vector3 GetPosition();
}
