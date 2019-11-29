using Newtonsoft.Json;
using Serialize;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Serialize
{
    public struct Vec3
    {
        public float x, y, z;
        public Vec3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
        public Vector3 ToUnity() { return new Vector3(x, y, z); }
    }

    public struct Quat
    {
        public float x, y, z, w;
        public Quat(Color c) { x = c.r; y = c.g; z = c.b; w = c.a; }
        public Quat(Quaternion q) { x = q.x; y = q.y; z = q.z; w = q.w; }
        public Quaternion ToUnity() { return new Quaternion(x, y, z, w); }
        public Color ToColor() { return new Color(x, y, z, w); }
    }

    public class Building
    {
        public string name;
        public Vec3 pos, scale;
        public Quat rot;

        [JsonIgnore]
        public GameObject go;

        // Attempt to fix target invocation exception in webGL
        public void SetBuildingInfo(GameObject _go)
        {
            var t = _go.transform;
            pos = new Vec3(t.position);
            scale = new Vec3(t.localScale);
            rot = new Quat(t.rotation);
            go = _go;
            name = _go.name;
        }

        //public Building(GameObject _go)
        //{
        //    var t = _go.transform;
        //    pos = new Vec3(t.position);
        //    scale = new Vec3(t.localScale);
        //    rot = new Quat(t.rotation);
        //    go = _go;
        //    name = _go.name;
        //}
    }
}


public class PlaceBuildingFromAssetBundle : MonoBehaviour
{
    static GameObject PlaceBuildingsObject = null;
    static string MeubilairUrl = "https://3d.amsterdam.nl/web/AssetBundles/Meubilair/";

    public static void StartPlace(Building b)
    {
        if (PlaceBuildingsObject == null)
        {
            PlaceBuildingsObject = new GameObject("PlaceBuildingsHelper");
            PlaceBuildingsObject.AddComponent<PlaceBuildingFromAssetBundle>();
        }
        PlaceBuildingsObject.GetComponent<PlaceBuildingFromAssetBundle>().StartCoroutine(LoadMeubilair(b));
    }

    static IEnumerator LoadMeubilair(Building b)
    {
        string n = b.name.Replace("(Clone)", "");
        string pad = MeubilairUrl + n;
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(pad))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("kan niet bij de assetbundles");
            }
            else
            {
                // Get downloaded asset bundle
                AssetBundle myLoadedAssetBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                GameObject[] prefabs = myLoadedAssetBundle.LoadAllAssets<GameObject>();
                GameObject prefab = prefabs[0];
                prefab.name = n;
                MeshCollider[] mcs = prefab.GetComponentsInChildren<MeshCollider>();
                foreach (MeshCollider mc in mcs)
                {
                    mc.enabled = false;
                }
                GameObject buildingIns = (GameObject)Instantiate(prefab, b.pos.ToUnity(), b.rot.ToUnity());
                buildingIns.transform.localScale = b.scale.ToUnity();
                buildingIns.AddComponent<ColliderCheck>();

                buildingIns.tag = "CustomPlaced";
                buildingIns.gameObject.layer = 11;
                myLoadedAssetBundle.Unload(false);

            }
        }
    }
}

public class SceneInstance
{
    // Cam
    public Vec3 CamPos;
    public Quat CamRot;

    // Tijd/weer
    public int Hours, Minutes, CurrentDay, CurrentMonth, CurrentYear, CurrentWeer;
    public bool TimeNow;
    public string EnviWeerType;

    // Options
    public bool[] Options;

    // Visualization
    public Quat Color;
    public float SliderA, SliderR, SliderG, SliderB;

    // Custom placed prefabs
    public Building[] CustomStaticBuildings;
    public Building[] CustomSizableBuildings;

    [JsonIgnore]
    GameObject wasActiveMenu;

    MenuFunctions EnableMenus(bool enable)
    {
        var mf = GameObject.FindObjectOfType<MenuFunctions>();
        if (mf != null)
        {
            if (enable)
            {
                foreach (var m in mf.allMenus)
                {
                    if (m.activeSelf) wasActiveMenu = m;
                    m.SetActive(true);
                }
            }
            else
            {
                foreach (var m in mf.allMenus)
                {
                    if (m != wasActiveMenu)
                        m.SetActive(false);
                }
            }
        }
        return mf;
    }

    public void PopulateFromScene()
    {
        // Enable all menus as otherwise the state of the buttons cannot be retrieved
        var mf = EnableMenus(true);

        if (mf != null)
        {
            Hours = mf._hours;
            Minutes = mf._minutes;
            CurrentDay = mf._currentDay;
            CurrentMonth = mf._currentMonth;
            CurrentYear = mf._currentYear;
            CurrentWeer = mf._currentWeer;

            Options = mf.optionsToggles.Select(to => to.isOn).ToArray();
        }
        else Debug.LogWarning("Cannot find menu functions");

        //EnviWeerType = EnviroSkyMgr.instance.Weather.currentActiveWeatherPreset.Name;

        var cam = GameObject.FindGameObjectWithTag("MainCamera");
        if ( cam != null )
        {
            CamPos = new Vec3(cam.transform.position);
            CamRot = new Quat(cam.transform.rotation);
        }
        else Debug.LogWarning("Cannot find MainCamera");

        var timeNowToggle = GameObject.Find("HuidigMoment");
        if ( timeNowToggle != null)
        {
            var toggle = timeNowToggle.GetComponent<Toggle>();
            if ( toggle != null )
            {
                TimeNow = toggle.isOn;
            }
        }
        else Debug.LogWarning("Cannot find TijdNu gameObject");

        var colorPicker = GameObject.Find("CUIColorPicker");
        if (colorPicker != null)
        {
            var pc = colorPicker.GetComponent<CUIColorPicker>();
            Color = new Quat(pc.Color);
            SliderA = pc.sliderA.value;
            SliderR = pc.sliderR.value;
            SliderG = pc.sliderG.value;
            SliderB = pc.sliderB.value;
        }
        else Debug.LogWarning("Cannot find CUIColorPicker");

        CustomStaticBuildings = GameObject.FindGameObjectsWithTag("CustomPlaced").Select(go =>
        {
            var b = new Building();
            b.SetBuildingInfo(go);
            return b;
            }).ToArray();
        CustomSizableBuildings = GameObject.FindGameObjectsWithTag("Sizeable").Select(go =>
        {
            var b = new Building();
            b.SetBuildingInfo(go);
            return b;
            }).ToArray();

        // Disable menus again now that data is retreived
        EnableMenus(false);
    }

    public void PopulateToScene(SceneSaver ss)
    {
        // Enable all menus as otherwise the state of the buttons cannot be retrieved
        var mf = EnableMenus(true);
        if (mf)
        {
            mf._hours = Hours;
            mf._minutes = Minutes;
            mf._currentDay = CurrentDay;
            mf._currentMonth = CurrentMonth;
            mf._currentYear = CurrentYear;
            mf._currentWeer = CurrentWeer;
            if (Options != null)
            {
                for (int i = 0; i < Options.Length; i++)
                    mf.optionsToggles[i].isOn = Options[i];
            }
        }
        else Debug.LogWarning("Cannot find menu functions");

        var cam = GameObject.FindGameObjectWithTag("MainCamera");
        if (cam != null)
        {
            cam.transform.position = CamPos.ToUnity();
            cam.transform.rotation = CamRot.ToUnity();
        }
        else Debug.LogWarning("Cannot find MainCamera");

        var timeNowToggle = GameObject.Find("TijdNu");
        if (timeNowToggle != null)
        {
            var toggle = timeNowToggle.GetComponent<Toggle>();
            if (toggle != null)
            {
                toggle.isOn = TimeNow;
            }
        }
        else Debug.LogWarning("Cannot find TijdNu gameObject");

        var colorPicker = GameObject.Find("CUIColorPicker");
        if (colorPicker != null)
        {
            var pc = colorPicker.GetComponent<CUIColorPicker>();
            pc.Color = new Color(Color.x, Color.y, Color.z, Color.w);
            pc.sliderA.value = SliderA;
            pc.sliderR.value = SliderR;
            pc.sliderG.value = SliderG;
            pc.sliderB.value = SliderB;
        }
        else Debug.LogWarning("Cannot find CUIColorPicker");

        foreach (var cb in CustomStaticBuildings)
        {
            PlaceBuildingFromAssetBundle.StartPlace(cb);
        }


        // TODO for some reason this does not set the 'correct' weather type eventhough it has the right value.
        //EnviroSkyMgr.instance.ChangeWeather(EnviWeerType);

        //if (TimeNow)
            //EnviroSkyMgr.instance.SetTime(DateTime.Now);
        //else
         //   EnviroSkyMgr.instance.SetTime(CurrentYear, CurrentDay, Hours, Minutes, 0);


        // Disable menus again now that data is retreived
        EnableMenus(false);
    }

    public void UploadBuildingData()
    {
        foreach (var b in CustomSizableBuildings)
        {
            if (b.go == null) continue;
            var mfs = b.go.GetComponentsInChildren<MeshFilter>();
            if (mfs != null && mfs.Length != 0)
            {
                string objData = ObjExporter.WriteObjToString(b.name + ".mtl", mfs, b.go.transform.worldToLocalMatrix);
                string mtlData = MtlExporter.WriteMaterialToString(mfs).ToString();
                var textures = MtlExporter.GetUniqueTextures(mfs);
                Uploader.StartUploadObj(b.name, objData, null);
                Uploader.StartUploadMtl(b.name, mtlData, null);
                Uploader.StartUploadTextures(textures, null);
            }
        }
    }

    public void DownloadBuildingData()
    {
        foreach (var b in CustomSizableBuildings)
        {
            if (string.IsNullOrEmpty(b.name)) continue;
         //   if (b.name.Contains("Cube")) continue;

            Uploader.StartDownloadObj(b.name, (string objData, bool succes) =>
            {
                if (this == null) return;
                if (!succes) return;

                Uploader.StartDownloadMtl(b.name, (string mtlData, bool succes2) =>
                {
                    if (this == null) return;
                    if (!succes2) return;

                    var meshMats = ObjExporter.ObjToMesh(objData);
                    var mats = MtlExporter.MtlToMaterials(mtlData);
                    if (meshMats == null || mats == null)
                        return;

                    GameObject go = new GameObject(b.name);
                    go.tag = "Sizeable";
                    var bd = go.AddComponent<BuildingDeserializer>();
                    
                    bd.Deserialize(b, meshMats.ToArray(), mats);
                    go.AddComponent<ColliderCheck>();
                });
            });
        }
    }
}


public class SceneSaver : MonoBehaviour
{
    public UnityEngine.UI.Text uploadtekst;
    public UnityEngine.UI.InputField SceneNaamOutput;
    public UnityEngine.UI.InputField SceneInput;

    public void Save()
    {
        SceneInstance si = new SceneInstance();
        si.PopulateFromScene();
        string output = JsonConvert.SerializeObject(si, Formatting.Indented);
        var guid = Guid.NewGuid().ToString();
        Uploader.StartUploadScene(guid, output, null);
        si.UploadBuildingData();
        SceneNaamOutput.text = guid;
        SceneNaamOutput.gameObject.SetActive(true);
        uploadtekst.gameObject.SetActive(true);
    }


    public void Load()
    {
        if (SceneInput == null)
            return;

        string sceneId = SceneInput.text;
        if (string.IsNullOrEmpty(sceneId))
            return;

        Uploader.StartDownloadScene(sceneId, (string json, bool bSucces) =>
        {
            Debug.Log("SceneID: " + sceneId);
            Debug.Log(json);
            if (!bSucces) return;
            Debug.Log("!! Pre deserialize");
            SceneInstance si = JsonConvert.DeserializeObject<SceneInstance>(json);
            if (si != null)
            {
                Debug.Log("!! Pre clear");
                ClearScene();
                Debug.Log("!! Pre populate");
                si.PopulateToScene(this);
                Debug.Log("!! Pre Download buildings");
                si.DownloadBuildingData();
            }
        });
    }

    void Paste(string str)
    {
        SceneInput.text = str;
    }

    public void ClearScene()
    {
        GameObject.FindGameObjectsWithTag("CustomPlaced").ToList().ForEach(g => Destroy(g));
        GameObject.FindGameObjectsWithTag("Sizeable").ToList().ForEach(g => Destroy(g));
    }
}

