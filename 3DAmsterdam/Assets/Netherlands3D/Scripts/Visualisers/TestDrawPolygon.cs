using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Netherlands3D.Visualisers
{
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
            var outerCircle = new List<Vector3>();
            var innerSquareHole = new List<Vector3>();
            var innerRoundHole = new List<Vector3>();

            //Outer circle
            var numberOfOuterVerts = 100;
            var radius = 100;
            for (int i = 0; i < numberOfOuterVerts; i++)
            {
                var radians = 2 * Mathf.PI / numberOfOuterVerts * i;
                var vertical = Mathf.Sin(radians);
                var horizontal = Mathf.Cos(radians);
                var spawnDir = new Vector3(horizontal, 0, vertical);
                var spawnPos = spawnDir * radius;

                outerCircle.Add(spawnPos);
            }
            outerCircle.Reverse();

            //Inner round hole
            radius = 10;
            var offset = new Vector3(70, 0, 0);
            for (int i = 0; i < numberOfOuterVerts; i++)
            {
                var radians = 2 * Mathf.PI / numberOfOuterVerts * i;
                var vertical = Mathf.Sin(radians);
                var horizontal = Mathf.Cos(radians);
                var spawnDir = new Vector3(horizontal, 0, vertical);
                var spawnPos = offset + (spawnDir * radius);

                innerRoundHole.Add(spawnPos);
            }


            innerSquareHole.Add(new Vector3(50, 0, -50));
            innerSquareHole.Add(new Vector3(50, 0, 50));
            innerSquareHole.Add(new Vector3(-50, 0, 50));
            innerSquareHole.Add(new Vector3(-50, 0, -50));

            testPolygons.Add(outerCircle);
            testPolygons.Add(innerSquareHole);
            testPolygons.Add(innerRoundHole);

            testDrawPolyEventTrigger.started.Invoke(testPolygons);
        }
    }
}