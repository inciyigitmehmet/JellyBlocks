using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Ana Menü Yöneticisi: Oyun açıldığında "Kakuzen" başlığını, 
/// rastgele Japon atasözünü, Daily Streak'i ve Level butonunu gösterir.
/// Play butonuna basıldığında kapı açılma animasyonu ile oyuna geçiş yapar.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private GameObject mainMenuPanel;       // Tüm ana menü paneli
    [SerializeField] private TextMeshProUGUI titleText;       // "KAKUZEN" başlığı
    [SerializeField] private TextMeshProUGUI proverbText;     // Rastgele atasözü
    [SerializeField] private TextMeshProUGUI levelButtonText; // Level butonundaki "Level X" yazısı
    [SerializeField] private Button playButton;               // Play (Level) butonu
    [SerializeField] private Button marketButton;             // Market butonu
    [SerializeField] private Button settingsButton;           // Ayarlar butonu (YENİ)
    [SerializeField] private TextMeshProUGUI streakText;      // Streak sayısı (x1, x2...)
    [SerializeField] private TextMeshProUGUI settingsButtonText; // Ayarlar (şimdilik placeholder)

    [Header("Kapı Geçiş Animasyonu")]
    [SerializeField] private GameObject doorTransitionPrefab; // ZenLevelCompleteScreen prefab'ı (kapı animasyonu için)

    // Japon Atasözleri (Her açılışta rastgele biri seçilir)
    private string[] proverbs = {
        "\"Fall seven times, stand up eight.\"\n- Japanese Proverb",
        "\"The bamboo that bends is stronger than the oak that resists.\"\n- Japanese Proverb",
        "\"Vision without action is a daydream.\"\n- Japanese Proverb",
        "\"Even monkeys fall from trees.\"\n- Japanese Proverb",
        "\"Beginning is easy, continuing is hard.\"\n- Japanese Proverb",
        "\"The nail that sticks out gets hammered down.\"\n- Japanese Proverb",
        "\"One kind word can warm three winter months.\"\n- Japanese Proverb",
        "\"If you believe everything you read, better not read.\"\n- Japanese Proverb",
        "\"A frog in a well does not know the great sea.\"\n- Japanese Proverb",
        "\"The flower that blooms in adversity is the rarest and most beautiful of all.\"\n- Japanese Proverb",
        "\"Adversity is the foundation of virtue.\"\n- Japanese Proverb",
        "\"Knowledge without wisdom is a load of books on the back of an ass.\"\n- Japanese Proverb"
    };

    private int dailyStreak = 0;

    private void Start()
    {
        // Daily Streak hesapla
        CalculateDailyStreak();

        // UI Elementlerini Doldur
        if (titleText != null)
        {
            titleText.text = "KAKUZEN";
        }

        if (proverbText != null)
        {
            proverbText.text = proverbs[Random.Range(0, proverbs.Length)];
        }

        if (streakText != null)
        {
            streakText.text = $"x{dailyStreak}";
        }

        // Level butonundaki yazıyı güncelle
        UpdateLevelButtonText();

        // Play butonuna tıklama olayını bağla
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        // Market butonuna tıklama (şimdilik sadece log)
        if (marketButton != null)
        {
            marketButton.onClick.AddListener(OnMarketClicked);
        }

        // Ayarlar butonuna tıklama
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        // Ana menü panelini göster, oyun arka planını hazırla
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    private void CalculateDailyStreak()
    {
        // Bugünün tarihini al (Yıl + Gün sırası = benzersiz gün ID'si)
        string todayKey = System.DateTime.Now.ToString("yyyyMMdd");
        string lastLoginDay = PlayerPrefs.GetString("LastLoginDay", "");
        dailyStreak = PlayerPrefs.GetInt("DailyStreak", 0);

        if (lastLoginDay == todayKey)
        {
            // Bugün zaten giriş yapmış, streak'i değiştirme
            return;
        }

        // Dünün tarihini hesapla
        string yesterdayKey = System.DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

        if (lastLoginDay == yesterdayKey)
        {
            // Dün giriş yapmış, streak devam ediyor!
            dailyStreak++;
        }
        else
        {
            // Birden fazla gün atlamış, streak sıfırlanıyor
            dailyStreak = 1;
        }

        // Kaydet
        PlayerPrefs.SetString("LastLoginDay", todayKey);
        PlayerPrefs.SetInt("DailyStreak", dailyStreak);
        PlayerPrefs.Save();
    }

    private void UpdateLevelButtonText()
    {
        if (levelButtonText != null)
        {
            LevelManager levelManager = FindAnyObjectByType<LevelManager>();
            int levelNum = 1;
            if (levelManager != null)
            {
                levelNum = levelManager.CurrentIndex + 1;
            }
            levelButtonText.text = $"Level {levelNum}";
        }
    }

    private void OnPlayClicked()
    {
        // Butona basıldığında kapı açılma animasyonu ile oyuna geç
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        // Oyun başlasın!
        // Kapıların kapalı gelip iki yana açılması (Slide Out) efekti
        if (doorTransitionPrefab != null)
        {
            GameObject transitionObj = Instantiate(doorTransitionPrefab);
            ZenLevelCompleteScreen transition = transitionObj.GetComponent<ZenLevelCompleteScreen>();
            if (transition != null)
            {
                transition.ShowOpenDoorsOnly();
            }
        }
        
        Debug.Log("[MainMenu] Oyun başlıyor!");
    }

    private void OnMarketClicked()
    {
        Debug.Log("[MainMenu] Market butonu tıklandı! (Henüz market eklenmedi)");
        // İleride burada market paneli açılacak
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[MainMenu] Ayarlar butonu tıklandı! (Henüz ayarlar menüsü eklenmedi)");
    }

    // Dışarıdan erişim: Menüyü tekrar göstermek için
    public void ShowMainMenu()
    {
        UpdateLevelButtonText();

        if (proverbText != null)
        {
            proverbText.text = proverbs[Random.Range(0, proverbs.Length)];
        }

        if (streakText != null)
        {
            streakText.text = $"x{dailyStreak}";
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    // Dışarıdan erişim: Menüyü gizlemek için
    public void HideMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }
}
