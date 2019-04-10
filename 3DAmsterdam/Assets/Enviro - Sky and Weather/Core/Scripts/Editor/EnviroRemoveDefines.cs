using UnityEditor;

public class EnviroRemoveDefines : UnityEditor.AssetModificationProcessor
{
        static string symbols;

        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions rao)
        {

            if (assetPath.Contains("Enviro Standard"))
            {
                symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

                if (symbols.Contains("ENVIRO_HD"))
                {
                    symbols = symbols.Replace("ENVIRO_HD;", "");
                    symbols = symbols.Replace("ENVIRO_HD", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
                }
            }

        if (assetPath.Contains("Enviro Lite"))
        {
            symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (symbols.Contains("ENVIRO_LW"))
            {
                symbols = symbols.Replace("ENVIRO_LW;", "");
                symbols = symbols.Replace("ENVIRO_LW", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
        }

        if (assetPath.Contains("Enviro Pro"))
        {
            symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (symbols.Contains("ENVIRO_PRO"))
            {
                symbols = symbols.Replace("ENVIRO_PRO;", "");
                symbols = symbols.Replace("ENVIRO_PRO", "");
                symbols = symbols.Replace("ENVIRO_LWRP", "");
                symbols = symbols.Replace("ENVIRO_LWRP;", "");
                symbols = symbols.Replace("ENVIRO_HDRP", "");
                symbols = symbols.Replace("ENVIRO_HDRP;", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
            }
        }

        return AssetDeleteResult.DidNotDelete;
        }
}
