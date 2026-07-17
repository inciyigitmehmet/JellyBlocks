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


    void Start()
    {
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

        //Oyuncu Grid dışında bir yere dokunulmadığı zaman tıklandı sayar ve ona göre yerleri belirler.
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
            currentSpriteRenderer = currentSelectionObj.GetComponent<SpriteRenderer>();
            currentLineRenderer = currentSelectionObj.GetComponent<LineRenderer>();

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
                shadowSr.sortingOrder = currentSpriteRenderer.sortingOrder - 1; // Arkada kalması için
                
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

    // Elini basılı tutup sürüklediğin sürece (Sadece geçerli bir seçim başladıysa çalışır)
    void HandleTouchDrag()
    {
        // Sürüklerken de grid dışına taşmayı engellemek için sadece grid içindeyse güncelliyoruz.
        Vector2Int hooverCoords = GetCurrentTileCoords();
        if (gridManager != null && hooverCoords.x >= 0 && hooverCoords.x < gridManager.width && hooverCoords.y >= 0 && hooverCoords.y < gridManager.height)
        {
            currentCoords = hooverCoords;
        }

        UpdateSelectionVisual();
    }

    //ekrana dokunuyor mu dokunmuyorsa false yap.
    void HandleTouchUp()
    {
        if (gridManager == null) return;
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
            // --- BAŞARILI HAMLE ---
            if (currentSpriteRenderer != null)
            {
                Color finalColor = currentSpriteRenderer.color;
                finalColor.a = lockedAlpha;
                currentSpriteRenderer.color = finalColor;

                Transform shadowTransform = currentSelectionObj.transform.Find("JellyShadow");
                if (shadowTransform != null)
                {
                    SpriteRenderer shadowSr = shadowTransform.GetComponent<SpriteRenderer>();
                    if (shadowSr != null)
                    {
                        Color sColor = shadowSr.color;
                        sColor.a = lockedAlpha;
                        shadowSr.color = sColor;
                    }
                }
            }
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

            if (gameManager != null)
            {
                gameManager.CheckWinCondition();//Bu kısımda dolu olan kare sayısı gridi tamamladıysa yani win conditiona uyuyorsa win alıyoruz.
            }
        }
        else
        {
            Debug.Log($"[DEBUG] HATALI HAMLE - Frame: {Time.frameCount}, Obje: {currentSelectionObj?.name}");
            // --- HATALı HAMLE ---
            // Kurallardan herhangi biri ihlal edildiyse animasyonu başlat
            //Sönme listesine ekliyoruz ki seviye geçişinde temizlenebilsin.
            fadingSelections.Add(currentSelectionObj);
            StartCoroutine(FlashRedAndDestroy(currentSelectionObj, currentSpriteRenderer, currentLineRenderer));
            //Hatalı obje coroutine'e devredildi, referansı sıfırlıyoruz.
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
        // Eğer sahnede seçili bir obje yoksa boşuna çalışma
        if (currentSelectionObj == null) return;

        // Grid'in ortalama offset değerlerini Selection_Manager içinde de hesaba katıyoruz
        float offsetX = (gridManager.width - 1) / 2f;
        float offsetY = (gridManager.height - 1) / 2f;

        // 1. İÇ DOLGU (SPRITE) MERKEZİNİ VE BOYUTUNU AYARLAMA
        // Matris koordinatlarından tekrar dünya pozisyonuna dönüştürmek için offsetleri çıkartıyoruz
        float centerX = ((startCoords.x + currentCoords.x) / 2f) - offsetX;
        float centerY = ((startCoords.y + currentCoords.y) / 2f) - offsetY;

        float width = Mathf.Abs(startCoords.x - currentCoords.x) + 1;
        float height = Mathf.Abs(startCoords.y - currentCoords.y) + 1;

        // --- PADDING (BOŞLUK) HESAPLAMALARI ---
        // Gölgelerin dışarı taşmasını engellemek ve hücreler arasında zarif bir boşluk bırakmak için
        // Jöle bloklarını tam 1x1 hücre boyutu yerine biraz küçültüyoruz.
        float shadowThickness = 0.06f; // Daha ince ve zarif bir gölge
        float paddingX = 0.08f;
        float paddingY = 0.14f;

        // Gölgeyle birlikte bloğun hücreyi dikeyde tam ortalaması için ana objeyi yukarı kaydırıyoruz.
        float visualCenterY = centerY + (shadowThickness / 2f); 

        // Z eksenini -0.1f yapıyoruz ki gridin önünde dursun.
        currentSelectionObj.transform.position = new Vector3(centerX, visualCenterY, -0.1f);

        // Objeyi hücrelerden biraz küçük (boşluklu) olacak şekilde ölçeklendiriyoruz
        float visualWidth = width - paddingX;
        float visualHeight = height - paddingY;
        currentSelectionObj.transform.localScale = new Vector3(visualWidth, visualHeight, 1);

        // Gölge (3D efekti) pozisyonunu sabit tutmak için visualHeight'a bölerek ayarlıyoruz
        Transform shadowTransform = currentSelectionObj.transform.Find("JellyShadow");
        if (shadowTransform != null)
        {
            // Obje 'visualHeight' kadar scale edildiği için local offset'i bölmeliyiz.
            shadowTransform.localPosition = new Vector3(0, -shadowThickness / visualHeight, 0.05f);
        }

        // 2. KENARLIKLARI (LINE RENDERER) KUTUNUN ETRAFINA ÇİZME
        if (currentLineRenderer != null)
        {
            // Kenarlıklar tüm şekli (Gölge dahil!) tam dışından saracak şekilde çizilecek
            float minX = centerX - (visualWidth / 2f);
            float maxX = centerX + (visualWidth / 2f);
            float maxY = visualCenterY + (visualHeight / 2f);
            // Alt kenarlık gölgenin bittiği yere kadar iniyor! Bu sayede içeriden çizgi geçmiyor.
            float minY = visualCenterY - (visualHeight / 2f) - shadowThickness;

            // Çizgi, iç dolgunun (Sprite'ın) bir tık daha önünde dursun diye -0.2f veriyoruz.
            float zPos = -0.2f;

            currentLineRenderer.SetPosition(0, new Vector3(minX, minY, zPos)); // Sol Alt
            currentLineRenderer.SetPosition(1, new Vector3(minX, maxY, zPos)); // Sol Üst
            currentLineRenderer.SetPosition(2, new Vector3(maxX, maxY, zPos)); // Sağ Üst
            currentLineRenderer.SetPosition(3, new Vector3(maxX, minY, zPos)); // Sağ Alt
            currentLineRenderer.SetPosition(4, new Vector3(minX, minY, zPos)); // Kareyi kapatmak için başa dön (Sol Alt)
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
}