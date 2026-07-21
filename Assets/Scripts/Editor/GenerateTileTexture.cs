using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateTileTexture : EditorWindow
{
    [MenuItem("Tools/Generate Rounded Tile")]
    public static void GenerateTexture()
    {
        int size = 1024;
        int radius = 160;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        Color transparent = new Color(1, 1, 1, 0);
        Color solidWhite = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Uzaklık hesaplama (Yuvarlak köşeler için)
                float dx = Mathf.Max(0, Mathf.Max(radius - x, x - (size - 1 - radius)));
                float dy = Mathf.Max(0, Mathf.Max(radius - y, y - (size - 1 - radius)));
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                if (distance < radius)
                {
                    // Yumuşak kenar (Anti-aliasing)
                    float alpha = Mathf.Clamp01(radius - distance);
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, transparent);
                }
            }
        }

        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        string dirPath = "Assets/Art/Textures";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        string path = dirPath + "/RoundedTile_HD.png";
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();

        // Otomatik Sprite ayarları
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteBorder = new Vector4(radius, radius, radius, radius);
            importer.SaveAndReimport();
        }

        Debug.Log("[RoundedTile] Yüksek çözünürlüklü Tile başarıyla oluşturuldu: " + path);
    }
}
