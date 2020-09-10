using UnityEngine;
using System.Collections;
using BruTile;
using ConvertCoordinates;
using LayerSystem;

public class FirstPersonExtents : MonoBehaviour, ICameraExtents
{
    private UnityEngine.Vector3[] corners;

    private UnityEngine.Vector3 topLeft = new UnityEngine.Vector3(-100, 0, 100);
    private UnityEngine.Vector3 topRight = new UnityEngine.Vector3(100, 0, 100);
    private UnityEngine.Vector3 bottomRight = new UnityEngine.Vector3(100, 0, -100);
    private UnityEngine.Vector3 bottomLeft = new UnityEngine.Vector3(-100, 0, -100);


    // quick fix to get extent data there, should be replaced   
    [SerializeField]
    private TileHandler tileHandler;

    [SerializeField]
    private TileLoader tileLoader;

    public Extent currentExtent;

    // Use this for initialization
    void Start()
    {
        tileHandler.CV = this;
        tileLoader.CV = this;
    }

    private void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {
        currentExtent = CalculateExtents();
    }


    
    
    private Extent CalculateExtents() 
    {
        corners = new UnityEngine.Vector3[4];
        corners[0] = transform.position + topLeft;
        corners[1] = transform.position + topRight;
        corners[2] = transform.position + bottomRight;
        corners[3] = transform.position + bottomLeft;

        // Determine the min and max X- en Z-value of the visible coordinates
        var unityMax = new UnityEngine.Vector3(-9999999, -9999999, -99999999);
        var unityMin = new UnityEngine.Vector3(9999999, 9999999, 9999999);
        for (int i = 0; i < 4; i++)
        {
            unityMin.x = Mathf.Min(unityMin.x, corners[i].x);
            unityMin.z = Mathf.Min(unityMin.z, corners[i].z);
            unityMax.x = Mathf.Max(unityMax.x, corners[i].x);
            unityMax.z = Mathf.Max(unityMax.z, corners[i].z);
        }

        // Convert min and max to WGS84 coordinates
        var wGSMin = CoordConvert.UnitytoWGS84(unityMin);
        var wGSMax = CoordConvert.UnitytoWGS84(unityMax);

        // Area that should be loaded
        var extent = new Extent(wGSMin.lon, wGSMin.lat, wGSMax.lon, wGSMax.lat);

        return extent;
    }

    public Extent GetExtent()
    {
        return currentExtent;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
