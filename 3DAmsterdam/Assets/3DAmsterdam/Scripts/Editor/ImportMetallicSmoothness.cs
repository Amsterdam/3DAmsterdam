using UnityEditor;
using UnityEngine;
using System.IO;
 
/*
This class hooks into Unity's asset import process to combine "Metallic" and "Smoothness" textures
into a single "Metallic-Smoothness" texture to be used in the "Metallic" texture slot of the standard shader.
Editing the "smoothness" part of the combined texture is very difficult to do since it lives in the alpha channel.
This lets you keep the smoothness texture separate, for ease of editing.
For example, you may want to save it as a PSD file with a UV Map layer that you turn on when working on it,
then turn back off when saving, or have multiple layers of elements that are more flexible to adjust dynamically.
 
Written by Todd Gillissie of Gilligames.
Feel free to use and distribute freely.
*/
 
public class ImportMetallicSmoothness : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!isMetallicOrSmoothness())
        {
            return;
        }
 
        // Sets some required import values for reading the texture's pixels for combining.
        TextureImporter textureImporter = (TextureImporter)assetImporter;
 
        textureImporter.isReadable = true;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.mipmapEnabled = false;
    }
 
    // We need to do the actual combining of textures in the Postprocessor, since the original texture needs to be finished processing first.
    void OnPostprocessTexture(Texture2D texture)
    {
        if (!isMetallicOrSmoothness())
        {
            return;
        }
 
        string filename = Path.GetFileNameWithoutExtension(assetPath);
        string combinedPath = "";
 
        Texture2D metallic = null;
        Texture2D smoothness = null;

        bool metallicMap = filename.EndsWith(" Metallic");
        bool roughnessMap = filename.EndsWith(" Roughness");
        bool smoothnessMap = filename.EndsWith(" Smoothness");

        if (metallicMap)
        {
            metallic = texture;
 
            string smoothnessPath = convertMetallicSmoothnessPath("Metallic", "Smoothness", out combinedPath);
            string roughnessPath = convertMetallicSmoothnessPath("Metallic", "Roughness", out combinedPath);
            if (File.Exists(smoothnessPath))
            {
                smoothness = AssetDatabase.LoadAssetAtPath<Texture2D>(smoothnessPath);
            }
            else if(File.Exists(roughnessPath))
            {
                smoothness = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessPath);    
            }
        }
        else if (smoothnessMap || roughnessMap)
        {
            smoothness = texture;
            string metallicPath = convertMetallicSmoothnessPath(roughnessMap ? "Roughness" : "Smoothness", "Metallic", out combinedPath);
 
            if (File.Exists(metallicPath))
            {
                metallic = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicPath);
            }
        }
 
        if (metallic == null)
        {
            Debug.LogWarningFormat("Associated Metallic texture not found for: {0}", filename);
            return;
        }
 
        if (smoothness == null)
        {
            Debug.LogWarningFormat("Associated Smoothness texture not found for: {0}", filename);
            return;
        }
 
        if (metallic.width != smoothness.width || metallic.height != smoothness.height)
        {
            Debug.LogWarningFormat("Metallic and Smoothness textures must be the same size in order to combine: {0}", assetPath);
            return;
        }
 
        //Make sure textures are readable
        var metallicPixels = metallic.GetPixels32();
        var smoothnessPixels = smoothness.GetPixels32();
        if(roughnessMap)
        {
            for (int i = 0; i < smoothnessPixels.Length; i++)
            {
                smoothnessPixels[i].r = (byte)(255 - smoothnessPixels[i].r);
                smoothnessPixels[i].g = (byte)(255 - smoothnessPixels[i].g);
                smoothnessPixels[i].b = (byte)(255 - smoothnessPixels[i].b);
            }
        }

        Texture2D combined = new Texture2D(metallic.width, metallic.height, TextureFormat.ARGB32, false);
 
        // Use the red channel info from smoothness for the alpha channel of the combined texture.
        // Since the smoothness should be grayscale, we just use the red channel info.
        for (int i = 0; i < metallicPixels.Length; i++)
        {
            metallicPixels[i].a = smoothnessPixels[i].r;
        }
 
        combined.SetPixels32(metallicPixels);
 
        // Save the combined data.
        byte[] png = combined.EncodeToPNG();
        File.WriteAllBytes(combinedPath, png);
 
        AssetDatabase.ImportAsset(combinedPath);
    }
 
    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Helper functions.
    ////////////////////////////////////////////////////////////////////////////////////////////////
 
    // Returns true if the texture being processed ends with " Metallic" or " Smoothness",
    // since we only want to work with those.
    private bool isMetallicOrSmoothness()
    {
        string filename = Path.GetFileNameWithoutExtension(assetPath);
 
        return filename.EndsWith(" Metallic") || filename.EndsWith(" Smoothness") || filename.EndsWith(" Roughness");
    }
 
    private string convertMetallicSmoothnessPath(string from, string to, out string combinedPath)
    {
        string filename = Path.GetFileNameWithoutExtension(assetPath);
        string extension = Path.GetExtension(assetPath);
        string pathWithoutFilename = Path.GetDirectoryName(assetPath);
        string baseFilename = filename.Substring(0, filename.Length - string.Format(" {0}", from).Length);
 
        string newPath = string.Format("{0}/{1} {2}{3}", pathWithoutFilename, baseFilename, to, extension);
 
        combinedPath = string.Format("{0}/{1} Metallic-Smoothness.png", pathWithoutFilename, baseFilename);
 
        return newPath;
    }
}