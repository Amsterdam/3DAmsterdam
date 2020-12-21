using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ConvertCoordinates;
using System.Globalization;
using Amsterdam3D.CameraMotion;

namespace LayerSystem
{
    struct fireworksCoordinate
    {
        public Vector3 unityPosition;
        public Vector2Int tilecoordinates;
    }

    
    public class FireworksLayer : Layer
    {
        private List<fireworksCoordinate> coordinates = new List<fireworksCoordinate>();
        public SunSettings sunSettings;
        public List<GameObject> FireworksPrefabCloseby;
        public List<GameObject> FireworksPrefabFaraway;
        public int maxCount = 100;
        public float minDistance = 100;
        public float splitDistance = 1000;
        public float maxDistance = 2000;
        public ICameraExtents cameraExtents;
        public Camera activeCamera;
        private bool isWaiting = false;
        public int activeCount = 0;
        private UnityEngine.Random random;

        // Data Loading
        public override void onDisableTiles(bool isenabled)
        {
            isEnabled = isenabled;
        }

        public override void HandleTile(TileChange tileChange, Action<TileChange> callback = null)
        {

            TileAction action = tileChange.action;
            switch (action)
            {
                case TileAction.Create:
                    StartCoroutine(getCoordinates(tileChange, callback));
                    tiles.Add(new Vector2Int(tileChange.X, tileChange.Y), CreateNewTile(tileChange));
                    break;
                case TileAction.Remove:
                    RemoveTile(tileChange, callback);
                    tiles.Remove(new Vector2Int(tileChange.X, tileChange.Y));
                    callback(tileChange);
                    return;
                default:
                    callback(tileChange);
                    break;
            }

        }

        private void RemoveTile(TileChange tileChange, Action<TileChange> callback = null)
        {
            Vector2Int tilecoord = new Vector2Int(tileChange.X, tileChange.Y);
            for (int i = coordinates.Count - 1; i >= 0; i--)
            {
                if (coordinates[i].tilecoordinates ==tilecoord)
                {
                    coordinates.RemoveAt(i);
                }
            }
            callback(tileChange);
        }

        IEnumerator getCoordinates(TileChange tileChange, Action<TileChange> callback = null)
        {
            yield return null;
            string escapedUrl = "https://acc.3d.amsterdam.nl/web/data/feature/Special/roadNodes/"+tileChange.X +"-" + tileChange.Y + ".txt";
            var sewerageRequest = UnityWebRequest.Get(escapedUrl);

            yield return sewerageRequest.SendWebRequest();
            if (sewerageRequest.isNetworkError || sewerageRequest.isHttpError)
            {
                Debug.Log(tileChange.X + "-" + tileChange.Y + ".txt");
                Debug.Log("error reading file");
                callback(tileChange);
            }
            string dataString = sewerageRequest.downloadHandler.text;
            ReadCoordinates(dataString, tileChange);
            callback(tileChange);
        }

        private void ReadCoordinates(string dataString,TileChange tilechange)
        {
            Vector2Int tilecoordinates = new Vector2Int(tilechange.X, tilechange.Y);

            string[] textlines = dataString.Split('\n');
            foreach (var textline in textlines)
            {
                if (textline !="")
                {
                    string[] coordinatestring = textline.Split(';');
                    Vector3 coordinate = CoordConvert.RDtoUnity(new Vector3RD(double.Parse(coordinatestring[0], CultureInfo.InvariantCulture), double.Parse(coordinatestring[1], CultureInfo.InvariantCulture), 0));
                    coordinate.y = 0f - (float)CoordConvert.referenceRD.z + 2;
                    fireworksCoordinate coord = new fireworksCoordinate();
                    coord.tilecoordinates = tilecoordinates;
                    coord.unityPosition = coordinate;
                    coordinates.Add(coord);

                }

            }

        }
        private Tile CreateNewTile(TileChange tileChange)
        {
            Tile tile = new Tile();
            tile.LOD = 0;
            tile.tileKey = new Vector2Int(tileChange.X, tileChange.Y);
            tile.layer = transform.gameObject.GetComponent<Layer>();
            //tile.gameObject = new GameObject("sewerage-" + tileChange.X + "_" + tileChange.Y);
            //tile.gameObject.transform.parent = transform.gameObject.transform;
            //tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
            //Generate(tileChange, tile, callback);
            return tile;
        }

        // Fireworks Shooting

        public void OnCameraChanged()
        {
            cameraExtents = CameraModeChanger.Instance.CurrentCameraExtends;
            activeCamera = CameraModeChanger.Instance.ActiveCamera;
        }

        new void Start()
        {
            foreach (DataSet dataset in Datasets)
            {
                dataset.maximumDistanceSquared = dataset.maximumDistance * dataset.maximumDistance;
            }
            sunSettings.SetTime("23:55");
            random = new UnityEngine.Random();
            cameraExtents = CameraModeChanger.Instance.CurrentCameraExtends;
            CameraModeChanger.Instance.OnFirstPersonModeEvent += OnCameraChanged;
            CameraModeChanger.Instance.OnGodViewModeEvent += OnCameraChanged;
            activeCamera = CameraModeChanger.Instance.ActiveCamera;
            //tekst.text = maxCount.ToString();
        }

        // Update is called once per frame
        void Update()
        {
            if (activeCamera.gameObject.transform.position.y < 100)
            {
                minDistance = 20;
            }
            else
            {
                minDistance = 100;
            }
            //tekst.text = transform.childCount + " van " + maxCount.ToString();
            if (isWaiting == false)
            {
                StartCoroutine(launchFireworks());

            }
        }

        private IEnumerator launchFireworks()
        {
            activeCount = transform.childCount;
            if (activeCount < maxCount)
            {
                isWaiting = true;
                CreateNewFireWorks();
                yield return null;
                isWaiting = false;

            }

        }

        public void SetMaxFireworksCount()
        {
           
        }
        private void CreateNewFireWorks()
        {


            Vector3 fireWorksPosition = GetRandomPosition();

            if ((fireWorksPosition - activeCamera.gameObject.transform.position).magnitude > splitDistance)
            {
                GameObject newFirework = Instantiate(FireworksPrefabFaraway[UnityEngine.Random.Range(0, FireworksPrefabFaraway.Count)], transform);
                newFirework.transform.position = fireWorksPosition;
            }
            else
            {
                GameObject newFirework = Instantiate(FireworksPrefabCloseby[UnityEngine.Random.Range(0, FireworksPrefabCloseby.Count)], transform);
                newFirework.transform.position = fireWorksPosition;
            }


        }


        private Vector3 GetRandomPosition()
        {
            Vector3 output = new Vector3();

            float rootMinDistance = Mathf.Sqrt(minDistance);
            float rootMaxDistance = Mathf.Sqrt(maxDistance);

            //get screensize
            float pixelWidth = activeCamera.pixelRect.width;
            float pixelHeight = activeCamera.pixelRect.height;
            float halfMaxWidthAngle = 30 * pixelWidth / pixelHeight;

            float relativeAngleHorizontal = UnityEngine.Random.Range(0 - halfMaxWidthAngle, halfMaxWidthAngle);
            float absoluteAngleHorizontal = relativeAngleHorizontal + activeCamera.gameObject.transform.rotation.eulerAngles.y;
            float Distance = UnityEngine.Random.Range(rootMinDistance, rootMaxDistance);
            Distance = Distance * Distance;
            Vector3 relativePostion = Matrix4x4.Rotate(Quaternion.Euler(0, absoluteAngleHorizontal, 0)).MultiplyVector(new Vector3(0, 0, Distance));
            output = activeCamera.gameObject.transform.position + relativePostion;
            output.y = 0f - (float)CoordConvert.referenceRD.z + 2;
            output = FindNearestCoordinate(output);

            return output;
        }
        private Vector3 FindNearestCoordinate(Vector3 point)
        {
            Vector3 output = Vector3.zero;
            float distance;
            float minimumDistance = float.MaxValue;

            for (int i = 0; i < coordinates.Count; i++)
            {
                distance = (point - coordinates[i].unityPosition).magnitude;
                if (distance<minimumDistance)
                {
                    minimumDistance = distance;
                    output = coordinates[i].unityPosition;
                }
            }
            return output;
        }
    }
}
