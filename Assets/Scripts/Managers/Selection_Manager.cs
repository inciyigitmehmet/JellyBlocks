using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Selection_Manager : MonoBehaviour
{
    // GridManager'a erişim sağlamak için referans tutuyoruz.
    [SerializeField] private GridManager gridManager;

    // Dokunuyor mu şu an?
    private bool isSelecting = false;

    //Matrisde nereye dokunuyor?
    private Vector2Int startCoords;

    //Matriste elini kaldırdığında nerede?
    private Vector2Int currentCoords;

    //GameManager.cs ile Selection_Manager'ı birbirine tanıtma.
    public GameManager gameManager;

    //Görsel Ayarlar
    public GameObject selectionPrefab; //Unityde oluşturduğum jelly karesini burada atıyoruz.
    private GameObject currentSelectionObj; //Oyuncunun sürüklediği kare ( geçici obje buraya atandı.)

    //Geçici objenin renk ve çizgi özelliklerine buradan ulaşabiliyoruz.
    private SpriteRenderer currentSpriteRenderer;
    private LineRenderer currentLineRenderer;

    public Color[] randomColors; // Bu kısımda rastgele renkleri gireceğim.

    public float dragAlpha = 0.3f; // Bu kısım sürüklerken iç dolgunun şeffaflık oranı.
    public float lockedAlpha = 0.7f; // Burası ise parmak kalkınca kalıcı iç dolgunun şeffaflık oranı

    //Kalıcı ve geçici tüm çizim objelerini takip eden listeler.
    private List<GameObject> permanentSelections = new List<GameObject>();
    private List<GameObject> fadingSelections = new List<GameObject>(); //Sönme animasyonundaki objeler.

    [Header("Effects")]
    // Doğru hamlede objeden dökülecek küçük taş kırıkları efekti
    public GameObject rockChipEffectPrefab;

    [Header("UI Effects")]
    // Ekranda belirip solan yazı prefab'ı
    public GameObject floatingTextPrefab;
    // Blok konduğunda çıkacak rastgele kelimeler
    private string[] celebrationWords = { "Ronin!", "Samurai!", "Katana!", "Shogun!", "Zen!", "Perfect!" };

    [Header("Hint System")]
    public bool isHintModeActive = false; // İpucu modu aktif mi?

    private Color ParseHexColor(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        return Color.white;
    }

    void Start()
    {
        // 2. Resimdeki o tatlı telefon oyunu pastel renkleri (Hardcoded)
        randomColors = new Color[] {
            ParseHexColor("#F2777A"), // Kırmızı/Somon
            ParseHexColor("#7C95F2"), // Mavi
            ParseHexColor("#69C971"), // Yeşil
            ParseHexColor("#F79446"), // Turuncu
            ParseHexColor("#F2C861"), // Sarı
            ParseHexColor("#6DC9C3"), // Turkuaz
            ParseHexColor("#AC90E4")  // Mor
        };

        //Referans oyundaki gibi tamamlanan jöleleri tamamen opak yapıyoruz.
        lockedAlpha = 1f;

        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
        //Eğer editörde selectionPrefab atanmadıysa, dosya yolundan otomatik yüklüyoruz.
        #if UNITY_EDITOR
        if (selectionPrefab == null)
        {
            selectionPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SelectionPrefab.prefab");
        }
        #endif
    }

    void Update()
    {
        //Mouse basıldığında VE şu an aktif bir sürükleme yoksa yeni sürükleme başlatıyoruz.
        if (Input.GetMouseButtonDown(0) && !isSelecting)
        {
            HandleTouchDown();//ekrana dokunma anı.
        }

        if (Input.GetMouseButton(0) && isSelecting)
        {
            HandleTouchDrag();//Basılı tutup sürükleme anı
        }

        if (Input.GetMouseButtonUp(0) && isSelecting)
        {
            HandleTouchUp();//Ekrandan elini çektiği an
        }
    }

    void HandleTouchDown()
    {
        Vector2Int clickedCoords = GetCurrentTileCoords();

        // --- İPUCU (HINT) MODU KONTROLÜ ---
        if (isHintModeActive)
        {
            if (gridManager != null && clickedCoords.x >= 0 && clickedCoords.x < gridManager.width && clickedCoords.y >= 0 && clickedCoords.y < gridManager.height)
            {
                GameObject tileObj = gridManager.GetTileAt(clickedCoords.x, clickedCoords.y);
                if (tileObj != null)
                {
                    Tile tileComp = tileObj.GetComponent<Tile>();
                    if (tileComp != null && tileComp.targetNumber > 0 && !tileComp.isFilled)
                    {
                        // Sayıya tıklandı! Çözümü otomatik uygula.
                        ApplyKatanaHint(clickedCoords.x, clickedCoords.y);
                    }
                }
            }
            // Başarılı veya başarısız, bir yere tıklayınca hint modundan çık
            isHintModeActive = false;
            return;
        }

        // Tıklanılan yer grid sınırları içerisindeyse seçime başla.
        if (gridManager != null && clickedCoords.x >= 0 && clickedCoords.x < gridManager.width && clickedCoords.y >= 0 && clickedCoords.y < gridManager.height)
        {
            //Önceki sürüklemeden kalan sahipsiz obje varsa yok ediyoruz.
            if (currentSelectionObj != null)
            {
                Destroy(currentSelectionObj);
                currentSelectionObj = null;
            }

            isSelecting = true;
            startCoords = clickedCoords;
            currentCoords = startCoords;

            currentSelectionObj = Instantiate(selectionPrefab);
            // Yeni çizilen jölenin sprite renderer'ını alıyoruz.
            currentSpriteRenderer = currentSelectionObj.GetComponent<SpriteRenderer>();
            
            // SÜRÜKLERKEN DİĞER BLOKLARIN ÜSTÜNDE GÖRÜNMESİ İÇİN (20 YAPTIK)
            currentSpriteRenderer.sortingOrder = 20; 
            
            currentLineRenderer = currentSelectionObj.GetComponent<LineRenderer>();
            currentLineRenderer.sortingOrder = 21; // Çizgiyi en üstüne alıyoruz.
            currentLineRenderer.useWorldSpace = false; // Obje scale olduğunda çizginin de otomatik scale olması için!

            // Unity'nin LineRenderer materyalsiz kaldığında verdiği pembe (magenta) hatasını önlemek için materyal atıyoruz.
            // Böylece LineRenderer jöle dolgusunun koyu tonunu kendi renginde düzgünce boyayabilir.
            if (currentSpriteRenderer != null && currentLineRenderer != null)
            {
                currentLineRenderer.material = currentSpriteRenderer.sharedMaterial;
            }

            //Klon oluşturulduğu anı ve çağrı kaynağını konsola yazdırıyoruz.
            Debug.Log($"[DEBUG] CLONE OLUŞTURULDU - Frame: {Time.frameCount}, Coords: {clickedCoords}, Obje: {currentSelectionObj.name}");

            Color pickedColor = Color.white;

            if (randomColors.Length > 0)
            {
                int randomIndex = Random.Range(0, randomColors.Length);
                pickedColor = randomColors[randomIndex];
            }

            if (currentSpriteRenderer != null)
            {
                Color fillColor = pickedColor;
                fillColor.a = dragAlpha;
                currentSpriteRenderer.color = fillColor;
            }

            if (currentLineRenderer != null)
            {
                //Kenarlık rengini jöle dolgusuyla uyumlu ama bir tık daha koyu/belirgin yapıyoruz.
                Color lineColor = pickedColor * 0.85f;
                lineColor.a = 1f;
                currentLineRenderer.startColor = lineColor;
                currentLineRenderer.endColor = lineColor;
                currentLineRenderer.positionCount = 5;

                // Kenarlıkları biraz inceltiyoruz.
                currentLineRenderer.startWidth = 0.04f;
                currentLineRenderer.endWidth = 0.04f;
                
                // 3 Boyut efekti için alt kısıma bir gölge Sprite'ı ekliyoruz
                GameObject shadowObj = new GameObject("JellyShadow");
                shadowObj.transform.SetParent(currentSelectionObj.transform);
                SpriteRenderer shadowSr = shadowObj.AddComponent<SpriteRenderer>();
                shadowSr.sprite = currentSpriteRenderer.sprite;
                shadowSr.sortingOrder = 19; // Sürüklerken gölge de üstte olsun
                
                // Sürüklerken gölgeyi (ikinci katmanı) tamamen Kapatıyoruz!
                // Böylece şeffaflıklar üst üste binip çirkin bir görüntü oluşturmaz.
                shadowSr.enabled = false; 

                // Gölgenin rengini kenarlık ile aynı ve saydamlığını dragAlpha yapıyoruz
                Color sColor = lineColor;
                sColor.a = dragAlpha;
                shadowSr.color = sColor;

                UpdateSelectionVisual();
            }
        }
        else
        {
            //Bu kısım grid dışına tıklanınca renk değişimini engeller.
            isSelecting = false;
        }
    }

    bool HasOverlap(Vector2Int start, Vector2Int end)
    {
        int minX = Mathf.Min(start.x, end.x);
        int maxX = Mathf.Max(start.x, end.x);
        int minY = Mathf.Min(start.y, end.y);
        int maxY = Mathf.Max(start.y, end.y);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                GameObject tileObj = gridManager.GetTileAt(x, y);
                if (tileObj != null)
                {
                    Tile tileComponent = tileObj.GetComponent<Tile>();
                    if (tileComponent != null && tileComponent.isFilled)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    Vector2Int GetMaxValidCoords(Vector2Int start, Vector2Int target)
    {
        int stepX = target.x >= start.x ? 1 : -1;
        int stepY = target.y >= start.y ? 1 : -1;
        
        int bestArea = -1;
        Vector2Int bestCoord = start;
        
        for (int x = start.x; x != target.x + stepX; x += stepX)
        {
            for (int y = start.y; y != target.y + stepY; y += stepY)
            {
                Vector2Int testCoord = new Vector2Int(x, y);
                if (!HasOverlap(start, testCoord))
                {
                    int area = (Mathf.Abs(x - start.x) + 1) * (Mathf.Abs(y - start.y) + 1);
                    if (area > bestArea)
                    {
                        bestArea = area;
                        bestCoord = testCoord;
                    }
                    else if (area == bestArea)
                    {
                        float distOld = Vector2Int.Distance(bestCoord, target);
                        float distNew = Vector2Int.Distance(testCoord, target);
                        if (distNew < distOld)
                        {
                            bestCoord = testCoord;
                        }
                    }
                }
                else
                {
                    break; // Bu (x) sütununda daha fazla (y) yönüne gidemeyiz, tıkandık.
                }
            }
        }
        return bestCoord;
    }

    // Elini basılı tutup sürüklediğin sürece (Sadece geçerli bir seçim başladıysa çalışır)
    void HandleTouchDrag()
    {
        // Sürüklerken de grid dışına taşmayı engellemek için sadece grid içindeyse güncelliyoruz.
        Vector2Int hooverCoords = GetCurrentTileCoords();
        if (gridManager != null && hooverCoords.x >= 0 && hooverCoords.x < gridManager.width && hooverCoords.y >= 0 && hooverCoords.y < gridManager.height)
        {
            // ENGEL KONTROLÜ: Dolu hücrelerin üzerinden geçilmesini engelle
            currentCoords = GetMaxValidCoords(startCoords, hooverCoords);
        }

        UpdateSelectionVisual();
    }

    //ekrana dokunuyor mu dokunmuyorsa false yap.
    void HandleTouchUp()
    {
        if (gridManager == null || !isSelecting) return;
        
        // Sürükleme işlemini bitiriyoruz
        isSelecting = false;

        // 1. SEÇİLEN ALANIN KÖŞELERİNİ VE TOPLAM BOYUTUNU HESAPLAMA
        int minX = Mathf.Min(startCoords.x, currentCoords.x);
        int maxX = Mathf.Max(startCoords.x, currentCoords.x);
        int minY = Mathf.Min(startCoords.y, currentCoords.y);
        int maxY = Mathf.Max(startCoords.y, currentCoords.y);

        // Dikdörtgenin alan formülü: (Genişlik) * (Yükseklik)
        int selectedArea = (maxX - minX + 1) * (maxY - minY + 1);

        // 2. KONTROL DEĞİŞKENLERİ
        bool hasOverlap = false; // Başka bir jölenin üstüne bindi mi?
        int numberCount = 0;     // Seçilen alanda kaç tane sayı var?
        int foundNumber = 0;     // Bulunan sayı kaç?

        // 3. SEÇİLEN ALANIN İÇİNDEKİ TÜM HÜCRELERİ (TILE) TARAMA
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                // GridManager'dan o koordinattaki hücreyi istiyorum.
                GameObject tileObj = gridManager.GetTileAt(x, y);

                if (tileObj != null)
                {
                    Tile tileComponent = tileObj.GetComponent<Tile>();

                    // KURAL 1: Seçilen kare daha öneden doldurulmuş mu?
                    if (tileComponent.isFilled)
                    {
                        hasOverlap = true;
                    }

                    // KURAL 2: Karenin içinde ipucu (sayı) var mı?
                    if (tileComponent.targetNumber > 0)
                    {
                        numberCount++; // Sayı bulduk, sayacı artır.
                        foundNumber = tileComponent.targetNumber; // Sayıyı hafızaya al.
                    }
                }
            }
        }
        Debug.Log($"[DEBUG] HandleTouchUp - Frame: {Time.frameCount}, Area: {selectedArea}, NumCount: {numberCount}, Found: {foundNumber}, Overlap: {hasOverlap}");

        // 4. SHIKAKU KURALLARINI VE TEKİL ÇÖZÜMÜ DEĞERLENDİRME
        bool isMoveValid = false;

        //Zaten doldurulmuş karelerin üstüne çizim yapılamaz.
        if (hasOverlap)
        {
            isMoveValid = false;
        }
        else
        {
            //Seviyedeki tekil çözümü kontrol etmek için LevelManager'a ulaşıyoruz.
            LevelManager levelManager = FindAnyObjectByType<LevelManager>();
            if (levelManager != null && levelManager.CurrentSolution != null)
            {
                //Çizilen kutunun çözüme uygun olup olmadığını kontrol ediyoruz.
                foreach (var rect in levelManager.CurrentSolution)
                {
                    //Çizilen alanın koordinatları ve boyutları çözümdekiyle eşleşiyor mu?
                    if (rect.x == minX && rect.y == minY && rect.w == (maxX - minX + 1) && rect.h == (maxY - minY + 1))
                    {
                        isMoveValid = true;
                        break;
                    }
                }
            }
        }

        // 5. SONUÇ
        if (isMoveValid)
        {
            Debug.Log($"[DEBUG] BAŞARILI HAMLE - Frame: {Time.frameCount}, Obje: {currentSelectionObj?.name}");
            
            // --- SES EFEKTİ (BAŞARILI) ---
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySlashSound();
            }

            // --- BAŞARILI HAMLE ---
            if (currentSpriteRenderer != null)
            {
                Color finalColor = currentSpriteRenderer.color;
                finalColor.a = lockedAlpha;
                currentSpriteRenderer.color = finalColor;
                
                // KALICI OLDUĞUNDA SEVİYESİNİ DÜŞÜRÜYORUZ
                currentSpriteRenderer.sortingOrder = 10;
            }

            Transform shadowTransform = currentSelectionObj.transform.Find("JellyShadow");
            if (shadowTransform != null)
            {
                SpriteRenderer shadowSr = shadowTransform.GetComponent<SpriteRenderer>();
                if (shadowSr != null)
                {
                    shadowSr.enabled = true; // Bırakıldığında gölgeyi aktif et!
                    Color finalSColor = shadowSr.color;
                    finalSColor.a = lockedAlpha;
                    shadowSr.color = finalSColor;
                    
                    shadowSr.sortingOrder = 9;
                }
            }
            
            // --- ETKİLEŞİM VE ANİMASYON ---
            // Taşın yerine oturduğu anki vurma/sıkışma animasyonunu başlatıyoruz.
            StartCoroutine(SquashAndStretchPlace(currentSelectionObj));

            permanentSelections.Add(currentSelectionObj);
            //Objeyi kalıcı listeye verdik, referansı sıfırlıyoruz ki tekrar kullanılmasın.
            currentSelectionObj = null;

            // Hamle geçerli olduğu için, seçilen altındaki tüm kareleri "DOLU" olarak işaretliyoruz
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    GameObject tileObj = gridManager.GetTileAt(x, y);
                    if (tileObj != null)
                    {
                        Tile tileComp = tileObj.GetComponent<Tile>();
                        if (tileComp != null)
                        {
                            tileComp.isFilled = true;
                            tileComp.UpdateTextDisplay(); // Sayının kaybolması için metin görünümünü güncelliyoruz.
                        }

                        // Hücre dolduğunda arkadaki gri ızgara çizgilerini görmemek için görselini kapatıyoruz.
                        SpriteRenderer tileSr = tileObj.GetComponent<SpriteRenderer>();
                        if (tileSr != null)
                        {
                            tileSr.enabled = false;
                        }
                    }
                }
            }

            // --- COMBO + SKOR (Sıra önemli: önce Combo, sonra Skor!) ---
            // Dünya pozisyonunu hesapla (Floating text için)
            Vector3 blockWorldCenter = new Vector3(
                (minX + maxX) / 2f - (gridManager.width  - 1) / 2f,
                (minY + maxY) / 2f - (gridManager.height - 1) / 2f,
                0f
            );
            Color placedColor = currentSpriteRenderer != null ? currentSpriteRenderer.color : Color.white;
            // 1) Combo bar güncelle (renk + multiplier)
            if (ComboBarManager.Instance != null)
                ComboBarManager.Instance.OnSuccessfulFill(placedColor, blockWorldCenter);
            // 2) Skoru hesapla (güncel multiplier kullanır)
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(selectedArea);

            if (gameManager != null)
            {
                gameManager.CheckWinCondition();//Bu kısımda dolu olan kare sayısı gridi tamamladıysa yani win conditiona uyuyorsa win alıyoruz.
            }
        }
        else
        {
            Debug.Log($"[DEBUG] HATALI HAMLE - Frame: {Time.frameCount}, Obje: {currentSelectionObj?.name}");
            
            // --- HATA SESİ (Can Kaybı İptal Edildi) ---
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayErrorSound(); // Sadece metal çınlama sesi çalsın
            }

            // --- HATALI HAMLE GÖRSELİ (KIRMIZI YANIP SÖNME) ---
            fadingSelections.Add(currentSelectionObj);
            StartCoroutine(FlashRedAndDestroy(currentSelectionObj, currentSpriteRenderer, currentLineRenderer));
            currentSelectionObj = null;
        }
    }

    Vector2Int GetCurrentTileCoords()
    {
        //oyuncunun mouseunu oyun ekranında 2 boyutlu alanda nerede olduğunu anlamamızı sağlar.
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Grid'i ortaladığımız için artık offsetX ve offsetY değerlerini hesaba katmalıyız.
        float offsetX = (gridManager.width - 1) / 2f;
        float offsetY = (gridManager.height - 1) / 2f;

        // Dünya pozisyonuna offset ekleyerek yuvarlıyoruz ki matris indeksini (0,1,2...) doğru bulalım.
        int x = Mathf.RoundToInt(mouseWorldPos.x + offsetX);
        int y = Mathf.RoundToInt(mouseWorldPos.y + offsetY);

        // elde ettiğimiz yeni x ve y yi Vector2Int olarak geri döndürür.
        return new Vector2Int(x, y);
    }

    void UpdateSelectionVisual()
    {
        if (currentSelectionObj == null) return;

        // --- DİNAMİK SAYDAMLIK (OPAKLIK) KONTROLÜ ---
        int minX = Mathf.Min(startCoords.x, currentCoords.x);
        int maxX = Mathf.Max(startCoords.x, currentCoords.x);
        int minY = Mathf.Min(startCoords.y, currentCoords.y);
        int maxY = Mathf.Max(startCoords.y, currentCoords.y);

        int selectedArea = (maxX - minX + 1) * (maxY - minY + 1);
        int numberCount = 0;
        int foundNumber = 0;
        bool hasOverlap = false;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                GameObject tileObj = gridManager.GetTileAt(x, y);
                if (tileObj != null)
                {
                    Tile tileComponent = tileObj.GetComponent<Tile>();
                    if (tileComponent.isFilled) hasOverlap = true;
                    if (tileComponent.targetNumber > 0)
                    {
                        numberCount++;
                        foundNumber = tileComponent.targetNumber;
                    }
                }
            }
        }

        // Eğer tek sayı varsa, çakışma yoksa ve alan o sayıya tam eşitse belirgin (0.8f), aksi halde silik (0.3f)
        float targetAlpha = (numberCount == 1 && selectedArea == foundNumber && !hasOverlap) ? 0.85f : dragAlpha;

        if (currentSpriteRenderer != null)
        {
            Color c = currentSpriteRenderer.color;
            c.a = targetAlpha;
            currentSpriteRenderer.color = c;
        }

        float offsetX = (gridManager.width - 1) / 2f;
        float offsetY = (gridManager.height - 1) / 2f;

        float centerX = ((startCoords.x + currentCoords.x) / 2f) - offsetX;
        float centerY = ((startCoords.y + currentCoords.y) / 2f) - offsetY;

        float width = Mathf.Abs(startCoords.x - currentCoords.x) + 1;
        float height = Mathf.Abs(startCoords.y - currentCoords.y) + 1;

        float shadowThickness = 0.08f; 
        float paddingX = 0.06f;
        float paddingY = 0.12f;

        float visualCenterY = centerY + (shadowThickness / 2f); 

        // Z eksenini -0.1f yapıyoruz ki gridin önünde dursun.
        currentSelectionObj.transform.position = new Vector3(centerX, visualCenterY, -0.1f);

        float visualWidth = width - paddingX;
        float visualHeight = height - paddingY;
        
        // --- TELEFON UYGULAMASI (IMAGE 2) GÖRÜNÜMÜ İÇİN 9-SLICE SİSTEMİ ---
        currentSelectionObj.transform.localScale = Vector3.one; 
        
        if (currentSpriteRenderer != null)
        {
            currentSpriteRenderer.drawMode = SpriteDrawMode.Sliced;
            currentSpriteRenderer.size = new Vector2(visualWidth, visualHeight);
        }

        Transform shadowTransform = currentSelectionObj.transform.Find("JellyShadow");
        if (shadowTransform != null)
        {
            SpriteRenderer shadowSr = shadowTransform.GetComponent<SpriteRenderer>();
            if (shadowSr != null)
            {
                shadowSr.drawMode = SpriteDrawMode.Sliced;
                shadowSr.size = new Vector2(visualWidth, visualHeight);
                Color sc = shadowSr.color;
                sc.a = targetAlpha;
                shadowSr.color = sc;
            }
            shadowTransform.localPosition = new Vector3(0, -shadowThickness, 0.05f);
        }

        // TELEFON OYUNU GÖRÜNÜMÜ (SOLID) İÇİN ÇİZGİLER TAMAMEN KAPATILDI
        if (currentLineRenderer != null)
        {
            currentLineRenderer.enabled = false;
        }
    }

    // IEnumerator: Zamanlayıcı (bekleme) kullanabildiğimiz özel fonksiyon tipi
    IEnumerator FlashRedAndDestroy(GameObject obj, SpriteRenderer sr, LineRenderer lr)
    {
        // Sadece ana obje null ise hiç çalıştırma (Bileşenlerin eksik olması coroutine'i baltalamasın)
        if (obj == null) yield break;

        //1. anında kırmızı parlama (Bileşenler varsa renklerini değiştiriyoruz)
        if (sr != null)
        {
            sr.color = new Color(1f, 0f, 0f, 0.5f);
            Transform shadowT = obj.transform.Find("JellyShadow");
            if (shadowT != null)
            {
                SpriteRenderer sSr = shadowT.GetComponent<SpriteRenderer>();
                if (sSr != null) sSr.color = new Color(0.5f, 0f, 0f, 0.5f);
            }
        }
        if (lr != null)
        {
            lr.startColor = Color.red;
            lr.endColor = Color.red;
        }

        //Kısa bir bekleme
        yield return new WaitForSeconds(0.2f);

        //Yavaşça Sönme Fade Out
        float fadeDuration = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // Bekleme esnasında obje bir şekilde silinirse hata vermemesi için güvenlik önlemi
            if (obj == null) yield break;

            elapsedTime += Time.deltaTime;

            //Şeffaflığı (alpha) 0.5'ten 0'a doğru yavaşça düşürüyoruz
            float alpha = Mathf.Lerp(0.5f, 0f, elapsedTime / fadeDuration);

            // Yeni şeffaflığı uyguluyoruz
            if (sr != null)
            {
                sr.color = new Color(1f, 0f, 0f, alpha);
                Transform shadowT = obj.transform.Find("JellyShadow");
                if (shadowT != null)
                {
                    SpriteRenderer sSr = shadowT.GetComponent<SpriteRenderer>();
                    if (sSr != null) sSr.color = new Color(0.5f, 0f, 0f, alpha);
                }
            }
            if (lr != null)
            {
                lr.startColor = new Color(1f, 0f, 0f, alpha);
                lr.endColor = new Color(1f, 0f, 0f, alpha);
            }

            //Bir sonraki kareyi belirle.
            yield return null;
        }

        //hatalıysa yok et ve sönme listesinden çıkar.
        fadingSelections.Remove(obj);
        Destroy(obj);
    }

    public void ClearAllSelections()
    {
        Debug.Log($"[DEBUG] ClearAllSelections çağrıldı - Frame: {Time.frameCount}, Kalıcı: {permanentSelections.Count}, Sönen: {fadingSelections.Count}, Geçici: {currentSelectionObj != null}");
        //Aktif çalışan tüm coroutine'leri durduruyoruz ki sönme animasyonu ölü objeye erişmesin.
        StopAllCoroutines();

        //Kalıcı çizimleri yok ediyoruz.
        foreach (var sel in permanentSelections)
        {
            if (sel != null) Destroy(sel);
        }
        permanentSelections.Clear();

        //Sönme animasyonundaki objeleri de yok ediyoruz ki sahnede sahipsiz kalmasın.
        foreach (var sel in fadingSelections)
        {
            if (sel != null) Destroy(sel);
        }
        fadingSelections.Clear();

        //Geçici çizim objesini de yok ediyoruz ki ekran aralarında kalmasın.
        if (currentSelectionObj != null)
        {
            Destroy(currentSelectionObj);
            currentSelectionObj = null;
        }

        isSelecting = false;
    }

    // Taş blok (dikdörtgen) yerine oturduğunda çalışacak Squash & Stretch animasyonu ve Particle efekti
    IEnumerator SquashAndStretchPlace(GameObject obj)
    {
        // Güvenlik kontrolü: Obje yoksa (veya silinmişse) coroutine'i durdur
        if (obj == null) yield break;

        // --- KATANA KESİK (SLASH) EFEKTİ TETİKLEME ---
        // Taştan parçacık dökülmesi (Particle) yerine çok daha asil ve temiz bir "Kılıç Kesiği" atıyoruz.
        GameObject katanaSlashObj = new GameObject("KatanaSlash");
        katanaSlashObj.transform.position = obj.transform.position;
        KatanaSlash katanaSlash = katanaSlashObj.AddComponent<KatanaSlash>();
        
        // Bloğun gerçek boyutunu veriyoruz ki kılıç izi bloğu tamamen boydan boya kessin
        // Sliced mode kullandığımız için gerçek boyut localScale'de değil, SpriteRenderer'ın size özelliğindedir!
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Vector2 blockSize = sr != null ? sr.size : new Vector2(1, 1);
        
        katanaSlash.PlaySlash(blockSize);

        // --- FLOATING TEXT (KUTLAMA YAZISI) ---
        if (floatingTextPrefab != null)
        {
            // Yazıyı bloğun ortasından veya çok az üzerinden doğuruyoruz ki çok yüksekten uçmasın
            Vector3 textSpawnPos = obj.transform.position + new Vector3(0, obj.transform.localScale.y / 4f, -2f);
            GameObject floatTextObj = Instantiate(floatingTextPrefab, textSpawnPos, Quaternion.identity);
            
            FloatingText floatingText = floatTextObj.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                // Rastgele bir kutlama kelimesi seç
                string randomWord = celebrationWords[Random.Range(0, celebrationWords.Length)];
                // Bloğun kendi rengini al
                Color blockColor = obj.GetComponent<SpriteRenderer>().color;
                // Şeffaflığını tam (1) yap ki yazı belirgin olsun
                blockColor.a = 1f; 
                
                floatingText.Initialize(randomWord, blockColor);
            }
        }

        // --- SQUASH & STRETCH ANİMASYONU ---
        Vector3 originalScale = obj.transform.localScale;
        
        // Sıkışma oranları: X ekseninde yayvanlaşacak (1.15x), Y ekseninde basılacak (0.85x)
        Vector3 squashScale = new Vector3(originalScale.x * 1.15f, originalScale.y * 0.85f, originalScale.z);

        // 1. AŞAMA: TAŞIN YERE ÇARPIP SIKIŞMA ANI (0.12 saniye)
        // Yere hızla oturan ağır bir taşın enerjiyi dışa vurması hissi
        float squashDuration = 0.12f;
        float elapsedTime = 0f;

        while (elapsedTime < squashDuration)
        {
            if (obj == null) yield break; // Obje silinirse güvenlice çık

            elapsedTime += Time.deltaTime;
            // Orijinal boyuttan basılmış boyuta doğru yumuşak (Lerp) geçiş
            obj.transform.localScale = Vector3.Lerp(originalScale, squashScale, elapsedTime / squashDuration);
            yield return null; // Bir sonraki frame'e geç
        }

        // 2. AŞAMA: GERİ SEKİP NORMALE DÖNME - OTURMA HİSSİ (0.12 saniye)
        // Sıkışan taşın kinetik enerjisini atıp kendi katı formuna geri dönmesi
        float stretchDuration = 0.12f;
        elapsedTime = 0f;

        while (elapsedTime < stretchDuration)
        {
            if (obj == null) yield break; // Obje silinirse güvenlice çık

            elapsedTime += Time.deltaTime;
            // Basılmış boyuttan tekrar orijinal (tasarlanan) boyutuna geri dönüş
            obj.transform.localScale = Vector3.Lerp(squashScale, originalScale, elapsedTime / stretchDuration);
            yield return null;
        }

        // Animasyon bitiminde tam olarak orijinal boyutta ve pozisyonda kalmasını garantiye al
        if (obj != null)
        {
            obj.transform.localScale = originalScale;
        }
    }

    // Bölüm bittiğinde tüm kalıcı taşlara katana darbesi yemiş gibi bir titreme (Tremble) efekti verir
    public void TriggerWinTremble()
    {
        foreach (GameObject obj in permanentSelections)
        {
            if (obj != null)
            {
                StartCoroutine(TrembleRoutine(obj));
            }
        }
    }

    private IEnumerator TrembleRoutine(GameObject obj)
    {
        Vector3 startPos = obj.transform.localPosition;
        float duration = 1.2f; // Çok daha uzun ve dingin bir geri dönüş süresi (Titreme uzatıldı)
        float elapsed = 0f;

        // Orijinal rengi kaydet (Parlama efekti için)
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        if (sr != null) originalColor = sr.color;

        // Kılıç dalgası (ripple) için ufak gecikme
        yield return new WaitForSeconds(Random.Range(0f, 0.15f));

        if (obj == null) yield break;

        // 1. Kılıcın vuruş açısı (Rastgele bir yön)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        // Kılıcın darbesiyle taşın aniden kaydığı nokta (Barbarca titremek yerine tek bir keskin darbe)
        Vector3 strikeOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 0.12f;
        
        // Taş darbeyi yediği an milisaniyede sarsıntıyla kayar
        obj.transform.localPosition = startPos + strikeOffset;

        // 2. Dingin bir şekilde (Ease-out) yerine geri kayması
        while (elapsed < duration)
        {
            if (obj == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease-out (hızlı başlar, yumuşak biter)
            
            obj.transform.localPosition = Vector3.Lerp(startPos + strikeOffset, startPos, smoothT);

            // Kesik anında tam beyaz/şeffaf olur, sonra yumuşakça normale döner
            if (sr != null)
            {
                float flashAmount = 1f - smoothT; 
                Color flashColor = Color.Lerp(originalColor, Color.white, flashAmount * 0.6f);
                flashColor.a = Mathf.Lerp(originalColor.a, 0.4f, flashAmount); 
                
                sr.color = flashColor;
            }
            
            yield return null;
        }

        // Efekt bitince milimetrik olarak eski yerine oturt ve orijinal rengi geri ver
        if (obj != null)
        {
            obj.transform.localPosition = startPos;
            if (sr != null) sr.color = originalColor;
        }
    }

    // --- İPUCU (HINT) SİSTEMİ METOTLARI ---

    // UI'daki Katana butonuna basınca çağrılır
    public void ToggleHintMode()
    {
        isHintModeActive = !isHintModeActive;
        if (isHintModeActive)
        {
            Debug.Log("Katana Hint Modu Aktif! Ekranda çözemediğiniz bir sayıya tıklayın.");
        }
        else
        {
            Debug.Log("Katana Hint Modu İptal Edildi.");
        }
    }

    // Sayıya tıklandığında çözümü otomatik olarak yerleştirir
    private void ApplyKatanaHint(int tx, int ty)
    {
        LevelManager levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager == null || levelManager.CurrentSolution == null) return;

        Shikaku_Solver.SolutionRect correctRect = new Shikaku_Solver.SolutionRect { w = 0 };
        
        // Çözüm listesinden tıklanan sayının ait olduğu dikdörtgeni bul
        foreach (var rect in levelManager.CurrentSolution)
        {
            if (tx >= rect.x && tx < rect.x + rect.w && ty >= rect.y && ty < rect.y + rect.h)
            {
                correctRect = rect;
                break;
            }
        }

        // Çözüm bulunduysa otomatik çiz
        if (correctRect.w > 0)
        {
            // Orijinal çizim mantığını taklit ediyoruz
            startCoords = new Vector2Int(correctRect.x, correctRect.y);
            currentCoords = new Vector2Int(correctRect.x + correctRect.w - 1, correctRect.y + correctRect.h - 1);
            
            currentSelectionObj = Instantiate(selectionPrefab);
            currentSpriteRenderer = currentSelectionObj.GetComponent<SpriteRenderer>();
            currentLineRenderer = currentSelectionObj.GetComponent<LineRenderer>();

            Color pickedColor = Color.white;
            if (randomColors.Length > 0)
                pickedColor = randomColors[Random.Range(0, randomColors.Length)];

            if (currentSpriteRenderer != null)
            {
                Color fillColor = pickedColor;
                fillColor.a = 1f; // Tam opak
                currentSpriteRenderer.color = fillColor;
                currentSpriteRenderer.sortingOrder = 10;
            }

            if (currentLineRenderer != null)
            {
                Color lineColor = pickedColor * 0.85f;
                lineColor.a = 1f;
                currentLineRenderer.startColor = lineColor;
                currentLineRenderer.endColor = lineColor;
                currentLineRenderer.positionCount = 5;
                currentLineRenderer.startWidth = 0.04f;
                currentLineRenderer.endWidth = 0.04f;
                currentLineRenderer.useWorldSpace = false;
                
                GameObject shadowObj = new GameObject("JellyShadow");
                shadowObj.transform.SetParent(currentSelectionObj.transform);
                SpriteRenderer shadowSr = shadowObj.AddComponent<SpriteRenderer>();
                shadowSr.sprite = currentSpriteRenderer.sprite;
                shadowSr.sortingOrder = currentSpriteRenderer.sortingOrder - 1;
                shadowSr.color = lineColor;
            }

            UpdateSelectionVisual();

            // UpdateSelectionVisual saydamlığı (alpha) 0.85'e çektiği için, 
            // kalıcı olan bu ipucu bloğunu tam opak (1.0f) hale getirmeliyiz.
            if (currentSpriteRenderer != null)
            {
                Color c = currentSpriteRenderer.color;
                c.a = lockedAlpha;
                currentSpriteRenderer.color = c;
                currentSpriteRenderer.sortingOrder = 10;
            }

            Transform shadowTransform = currentSelectionObj.transform.Find("JellyShadow");
            if (shadowTransform != null)
            {
                SpriteRenderer shadowSr = shadowTransform.GetComponent<SpriteRenderer>();
                if (shadowSr != null)
                {
                    shadowSr.enabled = true;
                    Color sc = shadowSr.color;
                    sc.a = lockedAlpha;
                    shadowSr.color = sc;
                    shadowSr.sortingOrder = 9;
                }
            }

            BoxCollider2D bc = currentSelectionObj.GetComponent<BoxCollider2D>();
            if (bc != null) bc.enabled = true;

            permanentSelections.Add(currentSelectionObj);
            
            // Hücreleri dolu olarak işaretle ve görsellerini gizle
            for (int x = correctRect.x; x < correctRect.x + correctRect.w; x++)
            {
                for (int y = correctRect.y; y < correctRect.y + correctRect.h; y++)
                {
                    GameObject tObj = gridManager.GetTileAt(x, y);
                    if (tObj != null)
                    {
                        Tile tComp = tObj.GetComponent<Tile>();
                        tComp.isFilled = true;
                        tComp.UpdateTextDisplay();

                        SpriteRenderer tileSr = tObj.GetComponent<SpriteRenderer>();
                        if (tileSr != null) tileSr.enabled = false;
                    }
                }
            }

            GameObject placedHintObj = currentSelectionObj;
            
            // Hint objesini korumak için referansı boşaltıyoruz ki TouchUp onu yok etmesin
            currentSelectionObj = null;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySlashSound();
            }

            // Kılıç efekti ve sarsıntılar için Squash rutinini başlat
            StartCoroutine(SquashAndStretchPlace(placedHintObj));

            // --- COMBO + SKOR (Hint yerleştirmeleri için de aynı mantık) ---
            int hintArea = correctRect.w * correctRect.h;
            Vector3 hintCenter = new Vector3(
                correctRect.x + correctRect.w / 2f - (gridManager.width  - 1) / 2f,
                correctRect.y + correctRect.h / 2f - (gridManager.height - 1) / 2f,
                0f
            );
            Color hintColor = placedHintObj != null
                ? (placedHintObj.GetComponent<SpriteRenderer>()?.color ?? Color.white)
                : Color.white;

            // 1) Combo bar güncelle
            if (ComboBarManager.Instance != null)
                ComboBarManager.Instance.OnSuccessfulFill(hintColor, hintCenter);
            // 2) Skoru hesapla (güncel multiplier kullanır)
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(hintArea);

            // Kazanma durumu kontrolü
            GameManager gameManager = FindAnyObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.CheckWinCondition();
            }
        }
    }
}