using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDrawPolygon : MonoBehaviour
{
    [SerializeField]
    private Vector3ListsEvent testDrawPolyEventTrigger;

    [ContextMenu("DrawRandomPoly")]
    private void DrawRandomPoly()
    {
        var testPolygons = new List<IList<Vector3>>();
        var outer = new List<Vector3>();
        var inner = new List<Vector3>();

        outer.Add(new Vector3(-100, 20, -100));
        outer.Add(new Vector3(-100, 0, 100));
        outer.Add(new Vector3(100, 0, 100));
        outer.Add(new Vector3(100, 0, -100));

        inner.Add(new Vector3(50, 0, -50));
        inner.Add(new Vector3(50, 0, 50));
        inner.Add(new Vector3(-50, 0, 50));
        inner.Add(new Vector3(-50, 0, -50));

        testPolygons.Add(outer);
        testPolygons.Add(inner);

        testDrawPolyEventTrigger.unityEvent.Invoke(testPolygons);
    }
}
