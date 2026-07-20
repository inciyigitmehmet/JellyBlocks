using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Samurai kılıç kesiği (Slice) ekran geçişi
public class SamuraiTransition : MonoBehaviour
{
    [Header("Transition Ayarları")]
    public Color screenColor = new Color(0.96f, 0.95f, 0.90f, 1f); // Dingin Krem Rengi
    public float textWaitTime = 1.5f; // Yazının ekranda kalma süresi
    public float sliceSpeed = 2000f; // Ekranın ikiye ayrılıp uçma hızı

    private RectTransform topHalf;
    private RectTransform bottomHalf;
    private TextMeshProUGUI centerText;
    private GameObject slashLine;

    public void StartTransition(System.Action onScreenCovered)
    {
        StartCoroutine(TransitionRoutine(onScreenCovered));
    }

    private IEnumerator TransitionRoutine(System.Action onScreenCovered)
    {
        // 1. KANVAS VE PANELLERİ OLUŞTUR
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Her şeyin üstünde olsun

        gameObject.AddComponent<CanvasScaler>();

        // Üst Yarı
        GameObject topObj = new GameObject("TopHalf");
        topObj.transform.SetParent(transform, false);
        topHalf = topObj.AddComponent<RectTransform>();
        topHalf.anchorMin = new Vector2(0, 0.5f);
        topHalf.anchorMax = new Vector2(1, 1);
        topHalf.offsetMin = Vector2.zero;
        topHalf.offsetMax = Vector2.zero;
        Image topImg = topObj.AddComponent<Image>();
        topImg.color = screenColor;

        // Alt Yarı
        GameObject botObj = new GameObject("BottomHalf");
        botObj.transform.SetParent(transform, false);
        bottomHalf = botObj.AddComponent<RectTransform>();
        bottomHalf.anchorMin = new Vector2(0, 0);
        bottomHalf.anchorMax = new Vector2(1, 0.5f);
        bottomHalf.offsetMin = Vector2.zero;
        bottomHalf.offsetMax = Vector2.zero;
        Image botImg = botObj.AddComponent<Image>();
        botImg.color = screenColor;

        // 2. ORTA YAZI (LEVEL COMPLETE)
        GameObject textObj = new GameObject("LevelText");
        textObj.transform.SetParent(transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        centerText = textObj.AddComponent<TextMeshProUGUI>();
        centerText.text = "LEVEL CLEARED";
        centerText.fontSize = 80;
        centerText.alignment = TextAlignmentOptions.Center;
        centerText.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Koyu gri/siyah zen yazı
        // Font varsayılan olarak Arial gelir, istersen Unity'de Inspector'dan Fredoka yapabilirsin.

        // Ekran tamamen kapandı, arkaplanda diğer bölümü yükle
        if (onScreenCovered != null)
            onScreenCovered.Invoke();

        // Yazıyı oku ve bekle
        yield return new WaitForSeconds(textWaitTime);

        // 3. KILIÇ KESİĞİ (SLASH) EFEKTİ
        Destroy(textObj); // Yazıyı yokediyoruz ki kesik açılsın

        // Bembeyaz bir çizgi (kılıç parlaması)
        slashLine = new GameObject("SlashLine");
        slashLine.transform.SetParent(transform, false);
        RectTransform slashRect = slashLine.AddComponent<RectTransform>();
        slashRect.anchorMin = new Vector2(0, 0.5f);
        slashRect.anchorMax = new Vector2(1, 0.5f);
        slashRect.sizeDelta = new Vector2(0, 20f); // 20 piksel kalınlığında beyaz kılıç izi
        Image slashImg = slashLine.AddComponent<Image>();
        slashImg.color = Color.white;

        // Çizgi parlasın (1 kare beklesin)
        yield return new WaitForSeconds(0.1f);
        Destroy(slashLine);

        // 4. İKİYE AYRILMA VE UÇMA (SLICE)
        float distance = 0;
        while (distance < Screen.height)
        {
            distance += sliceSpeed * Time.deltaTime;
            topHalf.anchoredPosition = new Vector2(0, distance);
            bottomHalf.anchoredPosition = new Vector2(0, -distance);
            yield return null;
        }

        // Geçiş bitti, kendini imha et
        Destroy(gameObject);
    }
}
