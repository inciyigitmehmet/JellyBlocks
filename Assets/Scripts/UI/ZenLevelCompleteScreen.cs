using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ZenLevelCompleteScreen : MonoBehaviour
{
    private string[] titles = { 
        "Immaculate!", "Flawless!", "Perfect!", "Zen Mastery!", 
        "Brilliant!", "Outstanding!", "Exceptional!", "Spotless!", "Pure Genius!" 
    };
    private string[] proverbs = {
        "You trusted your logic and it paid off perfectly.",
        "A clear mind leads to flawless execution.",
        "Patience and focus are the keys to victory.",
        "Every puzzle solved is a step towards enlightenment.",
        "Precision in thought brings perfection in action.",
        "The quiet mind absorbs all challenges effortlessly.",
        "Order emerges when the mind is at peace.",
        "True mastery is found in the simplest moves."
    };

    private System.Action onCompleteCallback;
    private bool isTransitioning = false;
    private Image bgImg;
    private CanvasGroup canvasGroup;

    public void ShowScreen(System.Action onComplete)
    {
        onCompleteCallback = onComplete;
        StartCoroutine(ScreenRoutine());
    }

    private IEnumerator ScreenRoutine()
    {
        // 1. KANVAS VE ARKA PLAN
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>(); // Buton tıklaması için gerekli!
        
        // EventSystem yoksa ekle (Buton tıklaması için zorunlu)
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.96f, 0.95f, 0.90f, 0.85f); // Arkadaki grid hafifçe görünsün

        // 2. BAŞLIK
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.6f); // Biraz daha geniş ve aşağıda
        titleRect.anchorMax = new Vector2(0.95f, 0.85f);
        titleRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = titles[Random.Range(0, titles.Length)];
        titleText.fontSize = 130;
        titleText.enableAutoSizing = true; // Taştığında otomatik küçült
        titleText.fontSizeMin = 60;
        titleText.fontSizeMax = 150;
        titleText.fontStyle = FontStyles.Italic | FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.9f, 0.3f, 0.3f, 1f); // Kırmızı
        
        // Başlık Gölgesi
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0, 0, 0, 0.2f);
        titleShadow.effectDistance = new Vector2(3, -3);

        // 3. ALT YAZI (ATASÖZÜ)
        GameObject provObj = new GameObject("Proverb");
        provObj.transform.SetParent(transform, false);
        RectTransform provRect = provObj.AddComponent<RectTransform>();
        provRect.anchorMin = new Vector2(0.05f, 0.35f); // Daha geniş alan
        provRect.anchorMax = new Vector2(0.95f, 0.55f);
        provRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI provText = provObj.AddComponent<TextMeshProUGUI>();
        provText.text = proverbs[Random.Range(0, proverbs.Length)];
        provText.fontSize = 55;
        provText.enableAutoSizing = true; // Taştığında otomatik küçült
        provText.fontSizeMin = 30;
        provText.fontSizeMax = 70;
        provText.alignment = TextAlignmentOptions.Center;
        provText.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Ekranı yavaşça belirginleştir
        float elapsed = 0;
        while(elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed/0.5f);
            yield return null;
        }

        // Yazıyı okuması için bekleyiş
        yield return new WaitForSeconds(2.0f);

        // Otomatik olarak kılıç kesiği ile yeni bölüme geçiş yap
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // UI Katana Kesiği Efekti
        GameObject slashObj = new GameObject("UIKatanaSlash");
        slashObj.transform.SetParent(transform, false);
        RectTransform rt = slashObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(0, 30); // Kalın, devasa bir kesik
        rt.localRotation = Quaternion.Euler(0, 0, Random.Range(15f, 45f) * (Random.value > 0.5f ? 1 : -1));
        
        Image img = slashObj.AddComponent<Image>();
        img.color = Color.white; // Bembeyaz parlayan bir kılıç izi

        // 1. Şimşek gibi uzama
        float duration = 0.1f;
        float elapsed = 0f;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rt.sizeDelta = new Vector2(Mathf.Lerp(0, 4000, elapsed/duration), 30);
            yield return null;
        }

        // Kesik atılır atılmaz arka plan yüklemesi başlasın
        if (onCompleteCallback != null) onCompleteCallback.Invoke();

        // 2. Solarla kaybolma ve tüm ekranın kararması
        duration = 0.3f;
        elapsed = 0f;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            img.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, t));
            rt.sizeDelta = new Vector2(4000, Mathf.Lerp(30, 0, t));
            
            // Ekran da kılıç darbesiyle yavaşça kararıp silinsin
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
