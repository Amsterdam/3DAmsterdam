using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

public interface ICameraExtents
{
	Vector3[] GetWorldSpaceCorners();
	Extent GetExtent();
    Vector3 GetPosition();
}
