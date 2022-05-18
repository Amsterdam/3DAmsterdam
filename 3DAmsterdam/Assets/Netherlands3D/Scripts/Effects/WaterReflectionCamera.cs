using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterReflectionCamera : MonoBehaviour
{
    [SerializeField]
    private Material waterMaterial;

    private Shader defaultShader;
    [SerializeField]
    private Shader advancedShader;

    private RenderTexture renderTexture;

    [SerializeField]
    private float downscale = 0.5f;

    private Camera camera;
    private Camera followCamera;

    [SerializeField]
    private Color advancedColor;
    private Color defaultColor;

    private int screenWidthOnInit = 512;
    private int screenHeightOnInit = 512;

    private void OnEnable()
    {
        if (!camera)
            camera = GetComponent<Camera>();

        if (!followCamera)
            followCamera = Camera.main;

        if (!renderTexture)
            CreateNewRenderTexture();

        defaultColor = waterMaterial.GetColor("_BaseColor");
        waterMaterial.SetColor("_BaseColor", advancedColor);
    }

    private void CreateNewRenderTexture()
    {
        renderTexture = new RenderTexture(Mathf.RoundToInt(followCamera.pixelWidth * downscale), Mathf.RoundToInt(followCamera.pixelHeight * downscale), 0);

        screenWidthOnInit = followCamera.pixelWidth;
        screenHeightOnInit = followCamera.pixelHeight;
        camera.targetTexture = renderTexture;

        defaultShader = waterMaterial.shader;
        waterMaterial.shader = advancedShader;
        waterMaterial.SetTexture("_ReflectionCameraTexture", renderTexture);
    }

    private void OnDisable()
    {
        waterMaterial.shader = defaultShader;
        waterMaterial.SetColor("_BaseColor", defaultColor);
        camera.targetTexture = null;
        Destroy(renderTexture);
    }

    void Update()
    {
        followCamera = Camera.main;

        if(Screen.width != followCamera.pixelHeight || screenHeightOnInit != followCamera.pixelHeight)
        {
            camera.targetTexture = null;
            Destroy(renderTexture);
            CreateNewRenderTexture();
        }

        camera.farClipPlane = followCamera.farClipPlane;
        camera.nearClipPlane = followCamera.nearClipPlane;

        this.transform.transform.SetPositionAndRotation(new Vector3(followCamera.transform.position.x,-followCamera.transform.position.y, followCamera.transform.position.z), followCamera.transform.rotation);
        this.transform.transform.localEulerAngles = new Vector3(-followCamera.transform.localEulerAngles.x, followCamera.transform.localEulerAngles.y, followCamera.transform.localEulerAngles.z);
    }
}
