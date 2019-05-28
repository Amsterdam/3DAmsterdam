using Newtonsoft.Json;
using Serialize;
using System;
using System.IO;
using System.Linq;
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
        public Building(string _name, Transform t)
        {
            name = _name;
            pos = new Vec3(t.position);
            scale = new Vec3(t.localScale);
            rot = new Quat(t.rotation);
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
    public Building[] CustomBuildings;
    public Building[] CustomBoxes;
      

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

        CustomBuildings = GameObject.FindGameObjectsWithTag("CustomPlaced").Select(cp => new Building(cp.name.Replace("(Clone)", ""), cp.transform)).ToArray();
        CustomBoxes = GameObject.FindGameObjectsWithTag("Sizeable").Select(b => new Building("Box", b.transform)).ToArray();

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

        foreach (var cb in CustomBuildings)
        {
            if (ss.StaticBuildings != null && ss.StaticBuildings.Length != 0)
            {
                for (int i = 0; i < ss.StaticBuildings.Length; i++)
                {
                    var prefab = ss.StaticBuildings[i];
                    if (prefab != null && prefab.name == cb.name.Replace("(Clone)", ""))
                    {
                        GameObject go = (GameObject)GameObject.Instantiate(prefab, cb.pos.ToUnity(), cb.rot.ToUnity());
                        go.transform.localScale = cb.scale.ToUnity();
                    }
                }
            }
            else Debug.LogWarning("No static buildings set.");
        }

        var pb = GameObject.FindObjectOfType<PlaatsBlokje>();
        if (pb != null)
        {
            foreach (var cb in CustomBoxes)
            {
                GameObject go = (GameObject)GameObject.Instantiate(pb.blokje, cb.pos.ToUnity(), cb.rot.ToUnity());
                go.transform.localScale = cb.scale.ToUnity();
            }
        }
        else Debug.LogWarning("Cannot find PlaatsBlokje script.");

        if (TimeNow)
            EnviroSkyMgr.instance.SetTime(DateTime.Now);
        else
            EnviroSkyMgr.instance.SetTime(CurrentYear, CurrentDay, Hours, Minutes, 0);
        EnviroSkyMgr.instance.ChangeWeather(EnviWeerType); // TODO for some reason you have to load scene twice to have make weather change effect


        // Disable menus again now that data is retreived
        EnableMenus(false);
    }
}


public class SceneSaver : MonoBehaviour
{
    public GameObject[] StaticBuildings;

    public void OnSave() { Save(); }
    public void OnLoad() { Load(); }

    public void Save()
    {
        SceneInstance si = new SceneInstance();
        si.PopulateFromScene();
        string output = JsonConvert.SerializeObject(si, Formatting.Indented);
        File.WriteAllText("Scene.json", output);
    }


    public void Load()
    {
        string json = File.ReadAllText("Scene.json");
        SceneInstance si = JsonConvert.DeserializeObject<SceneInstance>(json);
        si?.PopulateToScene(this);
    }
}

