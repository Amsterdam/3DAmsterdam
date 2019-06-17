using System.Collections.Generic;
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

    public static PostTextureRenderer Begin()
    {
          foreach (var o in RTT.GetComponent<EnableGameObjects>().Objects)
            o.SetActive(true);

        Camera rtt = RTT.GetComponentInChildren<Camera>();
        var ptr = rtt.GetComponent<PostTextureRenderer>();
        if (ptr == null)
            ptr = rtt.gameObject.AddComponent<PostTextureRenderer>();
        ptr.BeginCapture();
        return ptr;
    }

    // This function is a workaround to get the texture contents from a texture and makes
    // it encodable to PNG for upload or saving on disk.
    public static void Capture(Texture2D tex)
    {
        Camera rtt = RTT.GetComponentInChildren<Camera>();
        RawImage ri = RTT.GetComponentInChildren<RawImage>();
        var script = RTT.GetComponentInChildren<PostTextureRenderer>();

        ri.texture = tex;

        if (rtt.targetTexture != null)
        {
            var t = rtt.targetTexture;
            rtt.targetTexture = null;
            RenderTexture.ReleaseTemporary(t);
        }

        var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.Default);
        if (!rt.IsCreated())
            rt.Create();

        rtt.orthographicSize = tex.height;
        rtt.aspect = tex.width / (float)tex.height;
        rtt.targetTexture = rt;
      //  rtt.forceIntoRenderTexture = true;

        script.PreRender(tex.imageContentsHash.ToString(), tex.width, tex.height);
        rtt.Render();
    }

    public static Dictionary<string, Texture2D> End()
    {
        var script = RTT.GetComponentInChildren<PostTextureRenderer>();
        Camera rtt = RTT.GetComponentInChildren<Camera>();
        if (rtt.targetTexture != null)
        {
            var t = rtt.targetTexture;
            rtt.targetTexture = null;
            RenderTexture.ReleaseTemporary(t);
        }

        foreach (var o in RTT.GetComponent<EnableGameObjects>().Objects)
            o.SetActive(false);

        return script.EndCapture();
    }
}