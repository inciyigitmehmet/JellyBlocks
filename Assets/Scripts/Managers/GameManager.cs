using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Gridmanagere soruyoruz tahtanın durumu ne diye.
    public GridManager gridManager;

    //LevelManager referansı.
    [SerializeField] private LevelManager levelManager;
    
    //SelectionManager referansı (Kazanma efektleri için)
    private Selection_Manager selectionManager;

    //Seviyenin kazanılıp kazanılmadığını tutan kilit değişkeni.
    private bool isLevelWon = false;

    //Aktif WinSequence coroutine'ini takip eden referans.
    private Coroutine winCoroutine = null;

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
    }

    [Header("UI Effects")]
    public GameObject zenCompleteScreenPrefab;

    //Kazanılan an sonrası sonraki seviyeye geçişi geciktiren coroutine.
    private System.Collections.IEnumerator WinSequence()
    {
        //Her şeyin bittiği an kazanılan an. Tahta tamamen doldu.
        Debug.Log("Zen Complete!");

        // 1. ADIM: Tüm blokları sars (Kılıç titreşimi - Tremble efekti)
        if (selectionManager != null)
        {
            selectionManager.TriggerWinTremble();
        }
        
        // Titreme efektinin bitmesini bekle (yaklaşık 0.4 saniye)
        yield return new WaitForSeconds(0.4f);
        
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
}
