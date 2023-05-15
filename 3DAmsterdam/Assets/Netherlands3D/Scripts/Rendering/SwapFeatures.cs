using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SwapFeatures : MonoBehaviour
{
    private const string fieldName = "m_RendererDataList";


    public void EnableStencilByLayer(string layerName)
    {
        EnableStencilByLayerName(layerName, true);
    }
    public void DisableStencilByLayer(string layerName)
    {
        EnableStencilByLayerName(layerName, false);
    }


    private void EnableStencilByLayerName(string layerName, bool enable)
    {
        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
        FieldInfo propertyInfo = pipeline.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        var _scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];

        foreach (var renderObjSetting in _scriptableRendererData.rendererFeatures.OfType<UnityEngine.Experimental.Rendering.Universal.RenderObjects>())
        {
            int buildingLayermask = LayerMask.NameToLayer(layerName);
            LayerMask layermask = renderObjSetting.settings.filterSettings.LayerMask;

            if ((layermask.value & 1 << buildingLayermask) > 0)
            {
                renderObjSetting.settings.stencilSettings.stencilReference = (enable) ? 1 : 0;
                _scriptableRendererData.SetDirty();
#if UNITY_EDITOR
                Debug.Log(renderObjSetting.name + " set stencil set to " + renderObjSetting.settings.stencilSettings.stencilReference);
#endif
            }
        }
    }
}
