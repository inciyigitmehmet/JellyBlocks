using UnityEngine;
using System.Collections.Generic;
using System;

//Bu kısım editörde çalışıyor bu sayede build'de derlenmez.
#if UNITY_EDITOR
using UnityEditor;
#endif


//Level üretip Levels kısmına kaydeder.
public static class LevelGenerator
{
    //partition sırasında kullanılan dikdörtgen bölme
    private struct Rect { public int x, y, w, h; }

    //width x height boyutunda çözülebilir ve TEKİL bir seviye üretir.
    // Partition yöntemi : önce tahtayı dikdörtgenlere böler.
    // sonra her bölgenin alanını sayı olarak o bölge içine yerleştirir.
    // maxAttempts: üretemezsek kaç kere tekrar denesin (performans için).
    public static LevelData GenerateLevel(int width, int height, int maxAttempts = 50)
    {
        //Rastgele üretim için güçlü seed (eski: DateTime.Millisecond dar aralıktaydı, aynı seed gelirdi)
        //Guid ile benzersiz bir sayı üretip Random'a veriyoruz.
        var rng = new System.Random(Guid.NewGuid().GetHashCode());

        //Belirlenen deneme sayısı kadar dön (eğer ilk seferde düzgün çıkmazsa tekrar deneriz)
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            //Bölünen bölgeleri tutan liste
            List<Rect> regions = new List<Rect>();

            //Recursive bölme fonksiyonunu başlat (tüm tahtayı böl.)
            Split(0, 0, width, height, regions, rng, maxLeafArea: 8);

            //Her bölge için bir TileHint (sayı) oluştur.
            List<TileHint> hints = new List<TileHint>();
            foreach (var r in regions)
            {
                //Bölge içinde rastgele bir hücre seç (sayının konumu)
                int hx = r.x + rng.Next(0, r.w);
                int hy = r.y + rng.Next(0, r.h);

                //Sayı = bölgenin alanı (w * h) -> Shikaku kuralına uygun
                hints.Add(new TileHint { x = hx, y = hy, number = r.w * r.h });
            }

            //Yeni bir LevelData ScriptableObject örneği oluştur.
            var data = ScriptableObject.CreateInstance<LevelData>();
            data.width = width;
            data.height = height;
            data.hints = hints.ToArray();

            //GDD de 6.2 Adım 3: Çözülebilirlik ve TEKİL çözüm kontrolü (bizim solver ile)
            //Solve != null -> en az bir çözüm var
            //CountSolutions == 1 -> sadece bir tane çözüm var (birden fazla olursa o level atılmalı)
            bool isSolvable = Shikaku_Solver.Solve(data) != null;
            bool isUnique = Shikaku_Solver.CountSolutions(data, 2) == 1;

            //Hem çözülebilir hem tekilse bu leveli döndür (kaydedilsin diye)
            if (isSolvable && isUnique)
            {
                return data;
            }
            //Yoksa döngü bir sonrakini dener (retry)
        }

        //Hiçbir deneme geçerli çıkmadıysa hata bas ve null döndür (böylece kaydedilmez)
        Debug.LogError($"Üretilen {width} x {height} seviye {maxAttempts} denemede de geçerli/tekil çıkamadı!");
        return null;
    }

    //Özyinelemeli dikdörtgen bölme (recursive rectangle splitting)
    private static void Split(int x, int y, int w, int h, List<Rect> rects, System.Random rng, int maxLeafArea)
    {
        int area = w * h;
        //Alan küçükse veya 1x1 ise artık bölme (yaprak bölme)
        if (area <= maxLeafArea || (w <= 1 && h <= 1))
        {
            rects.Add(new Rect { x = x, y = y, w = w, h = h });
            return;

        }

        //Dikey mi yatay mı böleceğimize karar verdiğimiz yer.
        bool splitVertically = (w > h) ? true : (h > w ? false : rng.NextDouble() > 0.5);

        if (splitVertically && w > 1)
        {
            //Genişliği rastgele bir yerden böl
            int splitAt = rng.Next(1, w);
            Split(x, y, splitAt, h, rects, rng, maxLeafArea);
            Split(x + splitAt, y, w - splitAt, h, rects, rng, maxLeafArea);

        }

        else if (h > 1)
        {
            //Yüksekliği rastgele bir yerden böl
            int splitAt = rng.Next(1, h);
            Split(x, y, w, splitAt, rects, rng, maxLeafArea);
            Split(x, y + splitAt, w, h - splitAt, rects, rng, maxLeafArea);

        }
        else
        {
            rects.Add(new Rect { x = x, y = y, w = w, h = h });
        }

    }

    //Bu kısım editörde çalışıyor bu sayede build'de derlenmez.
#if UNITY_EDITOR
    //Unity menüsüne "JellyBlocks > Generate ALl Levels (5 to 10) " ekler
    [MenuItem("JellyBlocks/Generate All Levels (5 to 10)")]
    private static void GenerateAllAssets()
    {
        //Hedefimizdeki klasör : Assets/Resources/Levels
        string folder = "Assets/Resources/Levels";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateFolder("Assets/Resources", "Levels");
        }

        //5x5'ten 10x10'a kadar örnek üretim
        //Bu kısımda belirliyoruz KAÇ LEVEL OLACAĞINI
        for (int size = 5; size <= 10; size++)
        {
            //GenerateLevel artık null döndürebilir, o yüzden kontrol ediyoruz
            var data = GenerateLevel(size, size);
            if (data != null)
            {
                AssetDatabase.CreateAsset(data, $"{folder}/Level_{size}x{size}.asset");
            }
            else
            {
                Debug.LogWarning($"Level_{size}x{size} üretilemedi, atlandı.");
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Seviyeler Assets/Resources/Levels altına üretildi!");
    }

    //Unity menüsüne "JellyBlocks > Generate 100 Levels (5x5 to 10x10)" ekler
    [MenuItem("JellyBlocks/Generate 100 Levels (5x5 to 10x10)")]
    private static void Generate100Levels()
    {
        //Hedefimizdeki klasör : Assets/Resources/Levels
        string folder = "Assets/Resources/Levels";

        //Klasörün varlığından emin oluyoruz
        if (!AssetDatabase.IsValidFolder(folder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateFolder("Assets/Resources", "Levels");
        }

        //Eski seviye dosyalarını temizliyoruz ki çakışma olmasın
        string[] oldGuids = AssetDatabase.FindAssets("t:LevelData", new[] { folder });
        foreach (string guid in oldGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
        }

        Debug.Log("Eski seviyeler temizlendi. 100 adet seviye üretimi başlıyor...");

        //100 adet seviye üretmek için döngüyü başlatıyoruz
        for (int i = 1; i <= 100; i++)
        {
            int size = 5;

            //Kolaydan zora doğru seviye boyutlarını ekiyoruz
            if (i <= 20) size = 5;       //1-20 arası: 5x5
            else if (i <= 40) size = 6;  //21-40 arası: 6x6
            else if (i <= 60) size = 7;  //41-60 arası: 7x7
            else if (i <= 75) size = 8;  //61-75 arası: 8x8
            else if (i <= 90) size = 9;  //76-90 arası: 9x9
            else size = 10;              //91-100 arası: 10x10

            //Çözümü tekil ve geçerli olan bir seviye üretiyoruz
            var data = GenerateLevel(size, size, maxAttempts: 200);
            if (data != null)
            {
                //İsimlerin alfabetik sıralanması için 3 haneli indeks yapısı kullanıyoruz
                string fileName = $"Level_{i:000}_{size}x{size}";
                AssetDatabase.CreateAsset(data, $"{folder}/{fileName}.asset");
            }
            else
            {
                Debug.LogWarning($"Level_{i:000} ({size}x{size}) üretilemedi, atlandı.");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("100 adet seviye başarıyla üretildi!");
    }

    //Unity menüsüne "JellyBlocks > Validate All Levels" ekler
    [MenuItem("JellyBlocks/Validate All Levels")]
    private static void ValidateAllLevels()
    {
        //Sistemdeki tüm kayıtlı seviyeleri yüklüyoruz.
        var levels = LevelRepository.LoadAllLevels();
        if (levels == null || levels.Length == 0)
        {
            Debug.LogWarning("[LevelValidator] Doğrulanacak hiçbir seviye bulunamadı!");
            return;
        }

        int validCount = 0;
        int invalidCount = 0;

        //Her seviyeyi tek tek çözülebilirlik ve tekillik testine sokuyoruz.
        foreach (var level in levels)
        {
            if (level == null) continue;

            bool isSolvable = Shikaku_Solver.Solve(level) != null;
            int solutionCount = Shikaku_Solver.CountSolutions(level, 2);

            if (isSolvable && solutionCount == 1)
            {
                Debug.Log($"[LevelValidator] Seviye '{level.name}' GEÇERLİ ve TEKİL çözüme sahip.");
                validCount++;
            }
            else
            {
                Debug.LogError($"[LevelValidator] Seviye '{level.name}' GEÇERSİZ! Çözüm sayısı: {solutionCount} (1 olmalı)");
                invalidCount++;
            }
        }

        Debug.Log($"[LevelValidator] Doğrulama tamamlandı. Geçerli: {validCount}, Geçersiz: {invalidCount}");
    }
#endif //bitirdi

}