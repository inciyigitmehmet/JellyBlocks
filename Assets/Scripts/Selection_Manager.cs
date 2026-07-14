using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
public class Selection_Manager : MonoBehaviour

{
    // GridManager'a erişim sağlamak için referans tutuyoruz.
    private GridManager gridManager;

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

    private List<GameObject> permanentSelections = new List<GameObject>();

void Start()
     {
        gridManager = GetComponent<GridManager>();


     }
    void Update()
    {
        //ekrana dokunuyor mu dokunuyorsa true yap.
        if (Input.GetMouseButtonDown(0))

        {
            HandleTouchDown();//ekrana dokunma anı.

        }

        if (Input.GetMouseButton(0) && isSelecting)
        {
            HandleTouchDrag();//Basılı tutup sürükleme anı
        }

        if (Input.GetMouseButtonUp(0))
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
            isSelecting = true;
            startCoords = clickedCoords;
            currentCoords = startCoords;


            currentSelectionObj = Instantiate(selectionPrefab);
            currentSpriteRenderer = currentSelectionObj.GetComponent<SpriteRenderer>();
            currentLineRenderer = currentSelectionObj.GetComponent<LineRenderer>();

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
                currentLineRenderer.startColor = pickedColor;
                currentLineRenderer.endColor = pickedColor;
                currentLineRenderer.positionCount = 5;

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
            if (gridManager != null && hooverCoords.x >=0 && hooverCoords.x < gridManager.width && hooverCoords.y >= 0 && hooverCoords.y < gridManager.height)
            {
                currentCoords = hooverCoords;
            }


        UpdateSelectionVisual();

        }

    //ekrana dokunuyor mu dokunmuyorsa false yap.
    void HandleTouchUp()
    {
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

                    // KURAL 1: Seçilen kare daha önceden doldurulmuş mu?
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

        // 4. SHIKAKU KURALLARINI DEĞERLENDİRME
        bool isMoveValid = false;

        // Eğer üst üste binme YOKSA (hasOverlap false) VE
        // Tam olarak 1 tane sayı varsa (numberCount == 1) VE
        // Bulunan sayı, çizdiğimiz alana eşitse (foundNumber == selectedArea)
        if (!hasOverlap && numberCount == 1 && foundNumber == selectedArea)
        {
            isMoveValid = true; //eğer buraya kadar geliyorsa hamle geçerli
        }

        // 5. SONUÇ
        if (isMoveValid)
        {
            // --- BAŞARILI HAMLE ---
            if (currentSpriteRenderer != null)
            {
                Color finalColor = currentSpriteRenderer.color;
                finalColor.a = lockedAlpha;
                currentSpriteRenderer.color = finalColor;
            }
            permanentSelections.Add(currentSelectionObj);

            // Hamle geçerli olduğu için, seçilen altındaki tüm kareleri "DOLU" olarak işaretliyoruz
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    GameObject tileObj = gridManager.GetTileAt(x, y);
                    if (tileObj != null)
                    {
                        tileObj.GetComponent<Tile>().isFilled = true;
                    }
                }
            }

            if(gameManager != null)
            {
                gameManager.CheckWinCondition();//Bu kısımda dolu olan kare sayısı gridi tamamladıysa yani win conditiona uyuyorsa win alıyoruz.
            }
        }
        else
        {
            // --- HATALI HAMLE ---
            // Kurallardan herhangi biri ihlal edildiyse animasyonu başlat
            StartCoroutine(FlashRedAndDestroy(currentSelectionObj, currentSpriteRenderer, currentLineRenderer));
        }

        // Referansları temizle
        currentSelectionObj = null;
        currentSpriteRenderer = null;
        currentLineRenderer = null;
    }

    Vector2Int GetCurrentTileCoords()
    {
        
        //oyuncunun mouseunu oyun ekranında 2 boyutlu alanda nerede olduğunu anlamamızı sağlar.
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Ondalıklı olan pozisyonları integer değerlere yuvarlar.
        int x = Mathf.RoundToInt(mouseWorldPos.x);
        int y = Mathf.RoundToInt(mouseWorldPos.y);


        // elde ettiğimiz yeni x ve y yi Vector2Int olarak geri döndürür.
        return new Vector2Int(x, y);

    }

    void UpdateSelectionVisual()
    {
        // Eğer sahnede seçili bir obje yoksa boşuna çalışma
        if (currentSelectionObj == null) return;

        // 1. İÇ DOLGU (SPRITE) MERKEZİNİ VE BOYUTUNU AYARLAMA
        float centerX = (startCoords.x + currentCoords.x) / 2f;
        float centerY = (startCoords.y + currentCoords.y) / 2f;

        // Z eksenini -0.1f yapıyoruz ki gridin önünde dursun.
        currentSelectionObj.transform.position = new Vector3(centerX, centerY, -0.1f);

        float width = Mathf.Abs(startCoords.x - currentCoords.x) + 1;
        float height = Mathf.Abs(startCoords.y - currentCoords.y) + 1;

        // Objeyi seçilen alan kadar ölçeklendiriyoruz
        currentSelectionObj.transform.localScale = new Vector3(width, height, 1);

        // 2. KENARLIKLARI (LINE RENDERER) KUTUNUN ETRAFINA ÇİZME
        if (currentLineRenderer != null)
        {
            // Hücrelerin genişliği 1 birim olduğu için, kenarlar merkezden +/- 0.5f uzaktadır.
            float minX = Mathf.Min(startCoords.x, currentCoords.x) - 0.5f;
            float maxX = Mathf.Max(startCoords.x, currentCoords.x) + 0.5f;
            float minY = Mathf.Min(startCoords.y, currentCoords.y) - 0.5f;
            float maxY = Mathf.Max(startCoords.y, currentCoords.y) + 0.5f;

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
        //1. anında kırmızı parlama
        sr.color = new Color(1f, 0f, 0f, 0.5f);
        lr.startColor = Color.red;
        lr.endColor = Color.red;

        //Kısa bir bekleme
        yield return new WaitForSeconds(0.2f);

        //Yavaşça Sönme Fade Out
        float fadeDuration = 0.3f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            //Şeffaflığı (alpha) 0.5'ten 0'a doğru yavaşça düşürüyoruz
            float alpha = Mathf.Lerp(0.5f, 0f, elapsedTime / fadeDuration);

            // Yeni şeffaflığı uyguluyoruz
            sr.color = new Color(1f, 0f, 0f, alpha);
            lr.startColor = new Color(1f, 0f, 0f, alpha);
            lr.endColor = new Color(1f, 0f, 0f, alpha);

            //Bir sonraki kareyi belirle.
            yield return null;
        }

        //hatalıysa yok et
        Destroy(obj);
    }



}
