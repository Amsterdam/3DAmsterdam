using Newtonsoft.Json;
using Serialize;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
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

    public struct Building
    {
        public string name;
        public Vec3 pos, scale;
        public Quat rot;

        [JsonIgnore]
        public GameObject go;

        public Building(GameObject _go)
        {
            var t = _go.transform;
            pos = new Vec3(t.position);
            scale = new Vec3(t.localScale);
            rot = new Quat(t.rotation);
            go = _go;
            name = _go.name;
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

        EnviWeerType = EnviroSkyMgr.instance.Weather.currentActiveWeatherPreset.Name;

        var cam = GameObject.FindGameObjectWithTag("MainCamera");
        if ( cam != null )
        {
            CamPos = new Vec3(cam.transform.position);
            CamRot = new Quat(cam.transform.rotation);
        }
        else Debug.LogWarning("Cannot find MainCamera");

        var timeNowToggle = GameObject.Find("TijdNu");
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

        CustomStaticBuildings = GameObject.FindGameObjectsWithTag("CustomPlaced").Select(go => new Building(go)).ToArray();
        CustomSizableBuildings = GameObject.FindGameObjectsWithTag("Sizeable").Select(go => new Building(go)).ToArray();

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
            if (ss.StaticBuildings != null && ss.StaticBuildings.Length != 0)
            {
                for (int i = 0; i < ss.StaticBuildings.Length; i++)
                {
                    var prefab = ss.StaticBuildings[i];
                    if (prefab != null && cb.name.Contains(prefab.name))
                    {
                        GameObject go = (GameObject)GameObject.Instantiate(prefab, cb.pos.ToUnity(), cb.rot.ToUnity());
                        go.transform.localScale = cb.scale.ToUnity();
                        go.layer = 11;
                        go.tag = "CustomPlaced";
                        break;
                    }
                }
            }
            else Debug.LogWarning("No static buildings set.");
        }

        if (TimeNow)
            EnviroSkyMgr.instance.SetTime(DateTime.Now);
        else
            EnviroSkyMgr.instance.SetTime(CurrentYear, CurrentDay, Hours, Minutes, 0);
        EnviroSkyMgr.instance.ChangeWeather(EnviWeerType); // TODO for some reason you have to load scene twice to have make weather change effect


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
                var textures = MtlExporter.GetUniqueTextures(mfs).ToArray();
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
                    go.AddComponent<PijlenPrefab>();
                    bd.Deserialize(b, meshMats.ToArray(), mats);

                    var scaleScript = GameObject.Find("Manager").GetComponent<ScaleUploads>();
                    if (scaleScript != null)
                        scaleScript.gameObjects.Add(go);
                });
            });
        }
    }
}


public class SceneSaver : MonoBehaviour
{
    public GameObject[] StaticBuildings;
    public UnityEngine.UI.InputField SceneInput;

    public void Save()
    {
        SceneInstance si = new SceneInstance();
        si.PopulateFromScene();
        string output = JsonConvert.SerializeObject(si, Formatting.Indented);
        var guid = Guid.NewGuid().ToString();
        Uploader.StartUploadScene(guid, output, null);
        si.UploadBuildingData();
        //File.WriteAllText("Scene.json", output);
    }


    public void Load()
    {
        if (SceneInput == null)
            return;

        string sceneId = SceneInput.text;
        if (string.IsNullOrEmpty(sceneId))
            return;

        var scaleScript = GameObject.Find("Manager").GetComponent<ScaleUploads>();
        if (scaleScript != null)
            scaleScript.ClearGameObjectsList();

        MatchCollection mc = Regex.Matches(sceneId, "name=[a-f0-9-]*.json");
        bool bValid = false;
        foreach (var m in mc)
        {
            string s = m.ToString();
            s = s.Replace("name=", "");
            s = s.Replace(".json", "");
            sceneId = s;
            bValid = true;            
        }

        if (!bValid)
            return;

        Uploader.StartDownloadScene(sceneId, (string json, bool bSucces) =>
        {
            if (!bSucces) return;
            SceneInstance si = JsonConvert.DeserializeObject<SceneInstance>(json);
            if (si != null)
            {
                ClearScene();
                si.PopulateToScene(this);
                si.DownloadBuildingData();
            }
        });
    }

    public void ClearScene()
    {
        GameObject.FindGameObjectsWithTag("CustomPlaced").ToList().ForEach(g => Destroy(g));
        GameObject.FindGameObjectsWithTag("Sizeable").ToList().ForEach(g => Destroy(g));
    }
}

