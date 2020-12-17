using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amsterdam3D.CameraMotion;
using ConvertCoordinates;

public class fireworksShooter : MonoBehaviour
{

    public GameObject FireworksPrefab;
    public int maxCount = 100;
    
    public ICameraExtents cameraExtents;
    private bool isWaiting = false;
    public int activeCount = 0;
    private Random random;
    // Start is called before the first frame update
    public void OnCameraChanged()
    {
        cameraExtents = CameraModeChanger.Instance.CurrentCameraExtends;
    }

    void Start()
    {
        random = new Random();
        cameraExtents = CameraModeChanger.Instance.CurrentCameraExtends;
        CameraModeChanger.Instance.OnFirstPersonModeEvent += OnCameraChanged;
        CameraModeChanger.Instance.OnGodViewModeEvent += OnCameraChanged;
        
    }

    // Update is called once per frame
    void Update()
    {
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
            yield return new WaitForSeconds(0.01f);
            isWaiting = false;

        }
        
    }
    private void CreateNewFireWorks()
    {
        Vector3 bottomleft = CoordConvert.WGS84toUnity(cameraExtents.GetExtent().MinX, cameraExtents.GetExtent().MinY);
        Vector3 topRight = CoordConvert.WGS84toUnity(cameraExtents.GetExtent().MaxX, cameraExtents.GetExtent().MaxY);
        GameObject newFirework = Instantiate(FireworksPrefab, transform);
        float posX = Random.Range(bottomleft.x, topRight.x);
        float posY = Random.Range(bottomleft.z, topRight.z);
        float posZ = 0f - (float)CoordConvert.referenceRD.z;
        newFirework.transform.position = new Vector3(posX, posZ, posY);
    }
}
