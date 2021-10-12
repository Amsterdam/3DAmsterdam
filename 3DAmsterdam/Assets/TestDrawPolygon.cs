using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TestDrawPolygon : MonoBehaviour
{
    [SerializeField]
    private Vector3ListsEvent testDrawPolyEventTrigger;

    [ContextMenu("Draw10000Polies")]
    private void Draw10000Polies()
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

		for (int i = 0; i < 10000; i++)
		{
            DrawPolyWithHole();
        }
        stopWatch.Stop();

        print("Took: " + stopWatch.ElapsedMilliseconds);
	}

    [ContextMenu("DrawPolyWithHole")]
    private void DrawPolyWithHole()
    {
        var testPolygons = new List<IList<Vector3>>();
        var outer = new List<Vector3>();
        var inner = new List<Vector3>();

        outer.Add(new Vector3(-100, 0, -100));
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
