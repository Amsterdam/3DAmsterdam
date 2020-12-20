using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amsterdam3D.CameraMotion;
using ConvertCoordinates;
using UnityEngine.UI;

public class fireworksShooter : MonoBehaviour
{
    public SunSettings sunSettings;
    public Text tekst;
    public Slider slider;
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
    private Random random;
    // Start is called before the first frame update
    public void OnCameraChanged()
    {
        cameraExtents = CameraModeChanger.Instance.CurrentCameraExtends;
        activeCamera = CameraModeChanger.Instance.ActiveCamera;
    }

    void Start()
    {
        sunSettings.SetTime("23:55");
        random = new Random();
        cameraExtents = CameraModeChanger.Instance.CurrentCameraExtends;
        CameraModeChanger.Instance.OnFirstPersonModeEvent += OnCameraChanged;
        CameraModeChanger.Instance.OnGodViewModeEvent += OnCameraChanged;
        activeCamera = CameraModeChanger.Instance.ActiveCamera;
        //tekst.text = maxCount.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (activeCamera.gameObject.transform.position.y<100)
        {
            minDistance = 20;
        }
        else
        {
            minDistance = 100;
        }
        //tekst.text = transform.childCount + " van " + maxCount.ToString();
        if (isWaiting==false)
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
        maxCount = (int)slider.value;
        tekst.text = maxCount.ToString();
    }
    private void CreateNewFireWorks()
    {
        
        Vector3 bottomleft = CoordConvert.WGS84toUnity(cameraExtents.GetExtent().MinX, cameraExtents.GetExtent().MinY);
        Vector3 topRight = CoordConvert.WGS84toUnity(cameraExtents.GetExtent().MaxX, cameraExtents.GetExtent().MaxY);
        Vector3 fireWorksPosition = GetRandomPosition();

        if ((fireWorksPosition- activeCamera.gameObject.transform.position).magnitude>splitDistance)
        {
            GameObject newFirework = Instantiate(FireworksPrefabFaraway[Random.Range(0, FireworksPrefabFaraway.Count)], transform);
            newFirework.transform.position =fireWorksPosition;
        }
        else
        {
            GameObject newFirework = Instantiate(FireworksPrefabCloseby[Random.Range(0, FireworksPrefabCloseby.Count)], transform);
            newFirework.transform.position = fireWorksPosition;
        }

        
    }


    private Vector3 GetRandomPosition()
    {
        Vector3 output = new Vector3();

        //get screensize
        float pixelWidth = activeCamera.pixelRect.width;
        float pixelHeight = activeCamera.pixelRect.height;
        float halfMaxWidthAngle = 30*pixelWidth/pixelHeight;

        float relativeAngleHorizontal = Random.Range(0- halfMaxWidthAngle, halfMaxWidthAngle);
        float absoluteAngleHorizontal = relativeAngleHorizontal + activeCamera.gameObject.transform.rotation.eulerAngles.y;
        float Distance = Random.Range(minDistance, maxDistance);
        Vector3 relativePostion = Matrix4x4.Rotate(Quaternion.Euler(0, absoluteAngleHorizontal, 0)).MultiplyVector(new Vector3(0, 0, Distance));
        output = activeCamera.gameObject.transform.position + relativePostion;
        output.y = 0f - (float)CoordConvert.referenceRD.z+2;


        return output;
    }
}
