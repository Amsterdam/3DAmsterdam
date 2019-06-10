using UnityEngine;
using UnityEngine.UI;

public class TextureRenderer
{
    static GameObject _rtt;
    public static GameObject RTT
    {
        get
        {
            if (_rtt == null) _rtt = GameObject.Find("RTT");
            return _rtt;
        }
    }

    public static OnPostTextureRender2 Begin()
    {
        GameObject goRtt = RTT;
        if (goRtt == null) return null;

        foreach (var o in goRtt.GetComponent<EnableGameObjects>().Objects)
            o.SetActive(true);

        var res = goRtt.GetComponentInChildren<OnPostTextureRender2>();
        res.Begin();
        return res;
    }

    // This function is a workaround to get the texture contents from a texture and makes
    // it encodible to PNG for upload or saving on disk.
    public static void RenderTexture(Texture2D tex)
    {
        var goRtt = RTT;
        if (goRtt == null) return;

        Camera rtt = goRtt.GetComponentInChildren<Camera>();
        RawImage ri = goRtt.GetComponentInChildren<RawImage>();
        var script = goRtt.GetComponentInChildren<OnPostTextureRender2>();

        ri.texture = tex;

        RenderTexture rt = new RenderTexture(tex.width, tex.height, 0, RenderTextureFormat.Default);
        rtt.orthographicSize = tex.height;
        rtt.aspect = tex.width / (float)tex.height;
        rtt.targetTexture = rt;
        rtt.forceIntoRenderTexture = true;

        script.TextureName = tex.imageContentsHash.ToString();
        script.Width = tex.width;
        script.Height = tex.height;
        rtt.Render();
    }

    public static void End()
    {
        var goRtt = RTT;
        if (goRtt == null) return;

        var res = goRtt.GetComponentInChildren<OnPostTextureRender2>();
        res.End();

        foreach (var o in goRtt.GetComponent<EnableGameObjects>().Objects)
            o.SetActive(false);
    }
}