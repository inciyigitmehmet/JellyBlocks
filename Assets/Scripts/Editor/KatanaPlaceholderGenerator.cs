using UnityEngine;
using UnityEditor;
using System.IO;

public class KatanaPlaceholderGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Katana Placeholders")]
    public static void GeneratePlaceholders()
    {
        string dirPath = "Assets/Art/Textures";
        if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
        if (!AssetDatabase.IsValidFolder("Assets/Art/Textures")) AssetDatabase.CreateFolder("Assets/Art", "Textures");

        string maskPath = dirPath + "/KatanaMask_Placeholder.png";
        string outlinePath = dirPath + "/KatanaOutline_Placeholder.png";

        // 1. Mask (Solid White)
        Texture2D maskTex = new Texture2D(128, 512, TextureFormat.RGBA32, false);
        Color[] maskColors = new Color[128 * 512];
        for (int i = 0; i < maskColors.Length; i++) maskColors[i] = Color.white;
        maskTex.SetPixels(maskColors);
        maskTex.Apply();
        File.WriteAllBytes(maskPath, maskTex.EncodeToPNG());

        // 2. Outline (Hollow, Black Border)
        Texture2D outTex = new Texture2D(128, 512, TextureFormat.RGBA32, false);
        Color[] outColors = new Color[128 * 512];
        for (int x = 0; x < 128; x++)
        {
            for (int y = 0; y < 512; y++)
            {
                // Simple 8-pixel border
                if (x < 8 || x > 120 || y < 8 || y > 504) outColors[x + y * 128] = Color.black;
                else outColors[x + y * 128] = Color.clear;
            }
        }
        outTex.SetPixels(outColors);
        outTex.Apply();
        File.WriteAllBytes(outlinePath, outTex.EncodeToPNG());

        AssetDatabase.Refresh();

        // 3. Set Import Settings (Sprite, Full Rect)
        SetupTexture(maskPath);
        SetupTexture(outlinePath);
        
        Debug.Log("Katana Placeholders generated successfully with Full Rect Mesh Type!");
    }

    private static void SetupTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // Kesinlikle Full Rect olacak
            importer.SetTextureSettings(settings);
            
            importer.SaveAndReimport();
        }
    }
}
