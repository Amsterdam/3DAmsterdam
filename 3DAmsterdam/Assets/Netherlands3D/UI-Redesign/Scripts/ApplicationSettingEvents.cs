using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Netherlands3D.Settings;
using Netherlands3D.Interface.Minimap;
using Netherlands3D.Interface;
using Netherlands3D.TileSystem;

public class ApplicationSettingEvents : MonoBehaviour
{
    [Header("Event Listeners")]
    [Header("Interface")]
    [SerializeField] private UnityEvent<bool> enableMinimap;
    [SerializeField] private UnityEvent<bool> enableFpsTeller;
    [SerializeField] private UnityEvent<bool> enableExperimentalFeature;

    [SerializeField] private UnityEvent<float> interfaceScale;

    [Header("Graphics")]
    [SerializeField] private UnityEvent<bool> enableAllEffects;
    [SerializeField] private UnityEvent<bool> enableEffects;
    [SerializeField] private UnityEvent<bool> enableAmbiantOcclusion;
    [SerializeField] private UnityEvent<bool> enableLiveReflections;

    [SerializeField] private UnityEvent<int> changeQuality;

    [SerializeField] private UnityEvent<float> cameraZoom;

    [SerializeField] private UnityEvent<float> renderResolution;

    //Dependencies we fetch at start
    private MapViewer minimap;
    private Fps fpsCounter;
    private TileHandler tileHandler;
    private CanvasSettings canvasSettings;
    private WaterReflectionCamera waterReflectionCamera;
    private void Awake()
    {
        tileHandler = FindObjectOfType<TileHandler>();
        minimap = FindObjectOfType<MapViewer>();
        fpsCounter = FindObjectOfType<Fps>();
        canvasSettings = FindObjectOfType<CanvasSettings>();
        waterReflectionCamera = FindObjectOfType<WaterReflectionCamera>(true);
    }

    private void Start()
    {
        enableMinimap.AddListener(EnableMiniMap);
    }

    private void EnableMiniMap(bool isOn)
    {
        ApplicationSettings.settings.drawMap = isOn;
        //minimap.gameObject.SetActive(ApplicationSettings.settings.drawMap);
    }




}
