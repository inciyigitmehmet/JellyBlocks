using UnityEditor;
using UnityEngine;

public class SetSpriteBorder : EditorWindow
{
    // Varsayılan olarak belirttiğin yol ve border değerleri
    private string spritePath = "Assets/Art/Textures/ZenMaterials/WashiPaper.png";
    private Vector4 borderValues = new Vector4(100, 100, 100, 100); // Sol(X), Alt(Y), Sağ(Z), Üst(W)

    // Unity menüsünde "Tools -> Set Sprite Border" sekmesi oluşturur
    [MenuItem("Tools/Set Sprite Border")]
    public static void ShowWindow()
    {
        // Custom Editor penceresini oluştur ve ekranda göster
        GetWindow<SetSpriteBorder>("Set Sprite Border");
    }

    // Pencerenin içindeki kullanıcı arayüzünü (UI) çizen fonksiyon
    private void OnGUI()
    {
        GUILayout.Label("Sprite Border (9-Slice) Ayarlayıcı", EditorStyles.boldLabel);
        GUILayout.Label("Unity 6.5 Sprite Editor Bug Bypass Aracı", EditorStyles.helpBox);
        GUILayout.Space(10);

        // Kullanıcının yolu ve border değerlerini girebileceği alanlar
        spritePath = EditorGUILayout.TextField("Sprite Yolu:", spritePath);
        borderValues = EditorGUILayout.Vector4Field("Border (Sol, Alt, Sağ, Üst):", borderValues);

        GUILayout.Space(20);

        // Ayarla butonuna basıldığında işlemi başlat
        if (GUILayout.Button("Uygula (Save and Reimport)", GUILayout.Height(30)))
        {
            ApplyBorderToSprite();
        }
    }

    private void ApplyBorderToSprite()
    {
        // Verilen yoldaki dosyanın Texture Importer ayarlarını çek
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;

        if (importer != null)
        {
            // Sprite Type ayarının Sprite olduğundan emin olalım (Garanti olsun)
            importer.textureType = TextureImporterType.Sprite;

            // İlgili sprite'ın border değerlerini set et
            importer.spriteBorder = borderValues;
            
            // Değişiklikleri kaydet ve asset'i yeniden derle (Reimport)
            importer.SaveAndReimport();
            
            Debug.Log($"[SetSpriteBorder] Başarılı! '{spritePath}' için yeni border değerleri uygulandı: {borderValues}");
        }
        else
        {
            // Eğer importer bulunamazsa (yol yanlışsa veya dosya yoksa) açıkça hata ver
            Debug.LogError($"[SetSpriteBorder] HATA: '{spritePath}' yolunda bir Texture bulunamadı! Lütfen dosya yolunun doğru olduğundan ve sonunun '.png' gibi bir uzantıyla bittiğinden emin olun.");
        }
    }
}
