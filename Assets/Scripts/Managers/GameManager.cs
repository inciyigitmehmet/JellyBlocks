using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    //Gridmanagere soruyoruz tahtanın durumu ne diye.
    public GridManager gridManager;

    //LevelManager referansı.
    [SerializeField] private LevelManager levelManager;
    
    //SelectionManager referansı (Kazanma efektleri için)
    private Selection_Manager selectionManager;

    //Seviyenin kazanılıp kazanılmadığını tutan kilit değişkeni.
    public bool isLevelWon = false;

    //Aktif WinSequence coroutine'ini takip eden referans.
    //Aktif WinSequence coroutine'ini takip eden referans.
    private Coroutine winCoroutine = null;

    [Header("Hint System")]
    public int currentHints = 45;
    public TextMeshProUGUI hintCountText;
    public RectTransform hintButtonRect;
    public GameObject floatingHintPlusPrefab;
    private int levelsCompletedSinceLastHint = 0;

    [Header("Gold System")]
    public int currentGold = 0;
    public TextMeshProUGUI goldCountText;
    public UnityEngine.UI.Image goldIconImage; // Altın ikon öğesi (Coin sprite)

    [Header("Level Info UI")]
    public TextMeshProUGUI levelText; // Üst ortada "Level X" yazan metin

    private void Start()
    {
        //Editörde atanmamışsa GridManager'ı otomatik olarak buluyoruz.
        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }

        //Editörde atanmamışsa LevelManager'ı otomatik olarak buluyoruz.
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }

        selectionManager = FindAnyObjectByType<Selection_Manager>();

        // İpucu sistemini kayıttan yükle
        currentHints = PlayerPrefs.GetInt("HintCount", 45);
        levelsCompletedSinceLastHint = PlayerPrefs.GetInt("LevelsSinceLastHint", 0);
        UpdateHintUI();

        // Altın sistemini kayıttan yükle
        currentGold = PlayerPrefs.GetInt("GoldCount", 0);
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldCountText != null)
        {
            goldCountText.text = currentGold.ToString();
        }
    }

    public void UpdateLevelText(int levelNumber)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {levelNumber}";
        }
    }

    private void UpdateHintUI()
    {
        if (hintCountText != null)
        {
            hintCountText.text = currentHints.ToString();
        }
    }

    //Kazanıp kazanmadığımızı belli eden fonksiyon.
    public void CheckWinCondition()
    {
        //Eğer hatalıysa yani grid boşsa sıkıntılıysa geri döndürüyorum ki donup kalmasın ekran.
        if (gridManager == null) return;

        //Eğer seviye zaten kazanıldıysa mükerrer tetiklemeyi önlemek için çıkıyoruz.
        if (isLevelWon) return;

        int filledCount = 0;
        int totalCount = 0;

        //Bütün tilellara bu şekilde bakabiliriz.
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                //Bu sayede tileObj kısmına şu anki kordinatları eklemiş oluyoruz.
                GameObject tileObj = gridManager.GetTileAt(x, y);

                //Hücrenin varlığından emin olmak istiyoruz.
                if(tileObj != null)
                {
                    totalCount++;
                    //Hücrenin doluluk durumuna bakıyoruz.
                    if(tileObj.GetComponent<Tile>().isFilled)
                    {
                        filledCount++;
                    }
                }
            }
        }

        //Mevcut doluluk durumunu takip etmek için konsola log basıyoruz.
        Debug.Log($"[DEBUG] Seviye Durumu - Doldurulan: {filledCount} / Toplam: {totalCount}");

        //Doldurulan hücre sayısı toplam hücre sayısından küçükse kazanma tetiklenmez.
        if (filledCount < totalCount || totalCount == 0)
        {
            return;
        }

        //Kazanma durumunu kilitleyip geçiş sürecini başlatıyoruz.
        isLevelWon = true;

        // ALTIN ÖDÜLÜ: Grid alanı x 2 (5x5 = 50 Gold)
        int goldReward = (gridManager.width * gridManager.height) * 2;
        AddGold(goldReward);
        
        // KOMBO BARINI DONDUR (Geçiş ekranında boşalmasın)
        if (ComboBarManager.Instance != null)
        {
            ComboBarManager.Instance.PauseDrain();
        }

        // HINT (İPUCU) ÖDÜL SİSTEMİ: Her 2 bölümde bir +1 İpucu kazan.
        levelsCompletedSinceLastHint++;
        PlayerPrefs.SetInt("LevelsSinceLastHint", levelsCompletedSinceLastHint);
        
        if (levelsCompletedSinceLastHint >= 2)
        {
            levelsCompletedSinceLastHint = 0;
            PlayerPrefs.SetInt("LevelsSinceLastHint", 0);
            StartCoroutine(RewardHintSequence());
        }

        winCoroutine = StartCoroutine(WinSequence());
    }

    //Yeni seviyeye geçildiğinde kazanma kilidini açan ve eski coroutine'i durduran fonksiyon.
    public void ResetWinState()
    {
        //Aktif kazanma coroutine'i varsa durduruyoruz ki eski seviye geçişi tetiklenmesin.
        if (winCoroutine != null)
        {
            StopCoroutine(winCoroutine);
            winCoroutine = null;
        }
        isLevelWon = false;
        
        // KOMBO BARINI TEKRAR AKMAYA BAŞLAT
        if (ComboBarManager.Instance != null)
        {
            ComboBarManager.Instance.ResumeDrain();
        }
        
        // Yeni bölüme geçildiğinde canları sıfırla (İptal edildi)
        // currentLives = maxLives;
        // if (LifeUIManager.Instance != null)
        // {
        //     LifeUIManager.Instance.UpdateLifeUI(currentLives, maxLives);
        // }
    }

    [Header("Life System (ASKIYA ALINDI)")]
    public int maxLives = 5;
    public int currentLives = 5;

    public void LoseLife()
    {
        // Can sistemi şimdilik askıya alındı.
    }

    [Header("UI Effects")]
    public GameObject zenCompleteScreenPrefab;

    //Kazanılan an sonrası sonraki seviyeye geçişi geciktiren coroutine.
    private System.Collections.IEnumerator WinSequence()
    {
        //Her şeyin bittiği an kazanılan an. Tahta tamamen doldu.
        Debug.Log("Zen Complete!");

        // 1. ADIM: Tüm blokları sars ve aşağı dök (Pop & Drop efekti)
        if (selectionManager != null)
        {
            selectionManager.TriggerWinAnimation();
        }
        
        // Blokların havaya kalkıp aşağı dökülmesi için bekle (yaklaşık 2.5 saniye)
        // Dökülme süresini uzattığımız için geçiş ekranının da ona göre daha geç girmesi gerekiyor.
        yield return new WaitForSeconds(2.5f);
        
        // 2. ADIM: Zen Bölüm Sonu Ekranını Başlat
        if (zenCompleteScreenPrefab != null)
        {
            // Geçiş ekranını oluştur
            GameObject transitionObj = Instantiate(zenCompleteScreenPrefab);
            ZenLevelCompleteScreen transition = transitionObj.GetComponent<ZenLevelCompleteScreen>();
            
            if (transition != null)
            {
                // Ekran kapanırken diğer bölümü yükle
                transition.ShowScreen(() => 
                {
                    if (levelManager != null)
                    {
                        levelManager.LoadNextLevel();
                    }
                });
            }
        }
        else
        {
            // Eğer prefab atanmamışsa hata almamak için klasik bekleme
            yield return new WaitForSeconds(1.5f);
            if (levelManager != null)
            {
                levelManager.LoadNextLevel();
            }
        }
        
        winCoroutine = null;
    }

    private System.Collections.IEnumerator RewardHintSequence()
    {
        // Kapının açılmasına tam denk gelmesi için süreyi güncelledik (5.6s)
        yield return new WaitForSeconds(5.6f);

        // Uçma animasyonu başlarken sayıyı hemen artır (Yazı yok olana kadar bekleme)
        currentHints++;
        PlayerPrefs.SetInt("HintCount", currentHints);
        PlayerPrefs.Save();
        
        UpdateHintUI();

        if (floatingHintPlusPrefab != null && hintButtonRect != null)
        {
            // Yazıyı direkt Hint Butonunun İÇİNE (child) koyuyoruz. 
            // Böylece anchor (çapa) sorunları yüzünden ekranın dışına veya yanlış yere gitmesi imkansızlaşır!
            GameObject floatingObj = Instantiate(floatingHintPlusPrefab, hintButtonRect);
            floatingObj.SetActive(true); // Sahnede gizli/açık olmasına karşı her zaman aktif et

            RectTransform floatRt = floatingObj.GetComponent<RectTransform>();
            TextMeshProUGUI floatingText = floatingObj.GetComponent<TextMeshProUGUI>();

            if (floatRt != null && floatingText != null)
            {
                // Butonun tam ortasından başla
                floatRt.anchoredPosition = Vector2.zero; 
                floatingText.text = "+1";
                
                // Rengini prefabdan (senin ayarladığın renkten) alması için
                // koddaki sabit renk atamasını sildim!
                
                float animDuration = 1.5f;
                float elapsed = 0f;
                Vector2 startPos = floatRt.anchoredPosition;
                Vector2 endPos = startPos + new Vector2(0, 150f); // 150 piksel yukarı uçar

                while (elapsed < animDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / animDuration;
                    
                    // Yumuşak uçuş ve Alpha (şeffaflaşma) UI için anchoredPosition kullanılmalı!
                    floatRt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                    
                    Color c = floatingText.color;
                    c.a = 1f - t;
                    floatingText.color = c;
                    
                    yield return null;
                }
            }
            Destroy(floatingObj);
        }
    }

    public void ConsumeHint()
    {
        if (currentHints > 0)
        {
            currentHints--;
            PlayerPrefs.SetInt("HintCount", currentHints);
            PlayerPrefs.Save();
            UpdateHintUI();
        }
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        PlayerPrefs.SetInt("GoldCount", currentGold);
        PlayerPrefs.Save();
        UpdateGoldUI();

        // Altın yazısında Pop efekti (DOTween ile)
        if (goldCountText != null)
        {
            goldCountText.transform.localScale = Vector3.one;
            goldCountText.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
            StartCoroutine(GoldPopAnimation());
        }
    }

    private System.Collections.IEnumerator GoldPopAnimation()
    {
        if (goldCountText == null) yield break;
        
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 bigScale = new Vector3(1.3f, 1.3f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease Out Elastic benzeri geri sekme
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            goldCountText.transform.localScale = Vector3.Lerp(bigScale, Vector3.one, easeT);
            yield return null;
        }
        goldCountText.transform.localScale = Vector3.one;
    }
}
