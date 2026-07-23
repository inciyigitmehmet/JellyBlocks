using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ZenLevelCompleteScreen : MonoBehaviour
{
    private string[] titles = { 
        "Immaculate!", "Flawless!", "Perfect!", "Zen Mastery!", 
        "Brilliant!", "Outstanding!", "Exceptional!", "Spotless!", "Pure Genius!",
        "Flow State", "Clarity", "Harmony", "Serenity"
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

    public void ShowScreen(System.Action onComplete)
    {
        onCompleteCallback = onComplete;
        StartCoroutine(ShojiRoutine());
    }

    public void ShowOpenDoorsOnly()
    {
        StartCoroutine(OpenDoorsRoutine());
    }

    private IEnumerator OpenDoorsRoutine()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>(); 

        GameObject leftDoor = CreateShojiDoor(canvas.transform, true);
        GameObject rightDoor = CreateShojiDoor(canvas.transform, false);
        
        RectTransform leftRt = leftDoor.GetComponent<RectTransform>();
        RectTransform rightRt = rightDoor.GetComponent<RectTransform>();

        // Başlangıçta kapılar tamamen kapalı (0,0)
        leftRt.anchoredPosition = Vector2.zero;
        rightRt.anchoredPosition = Vector2.zero;

        float offscreenOffset = 2500f;
        
        // Çok kısa bekle, arka plan gizlensin kapılar görünsün
        yield return new WaitForSeconds(0.1f);

        // KAPILARIN AÇILMA (SLIDE OUT) ANİMASYONU
        float slideOutDuration = 0.5f;
        float elapsed = 0f;
        while(elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideOutDuration;
            float easeT = t * t * (3f - 2f * t); 
            
            leftRt.anchoredPosition = new Vector2(Mathf.Lerp(0, -offscreenOffset, easeT), 0);
            rightRt.anchoredPosition = new Vector2(Mathf.Lerp(0, offscreenOffset, easeT), 0);
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator ShojiRoutine()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>(); 
        
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // KAPILAR
        GameObject leftDoor = CreateShojiDoor(canvas.transform, true);
        GameObject rightDoor = CreateShojiDoor(canvas.transform, false);
        
        RectTransform leftRt = leftDoor.GetComponent<RectTransform>();
        RectTransform rightRt = rightDoor.GetComponent<RectTransform>();

        float offscreenOffset = 2500f;
        leftRt.anchoredPosition = new Vector2(-offscreenOffset, 0);
        rightRt.anchoredPosition = new Vector2(offscreenOffset, 0);

        // KAPANMA (SLIDE IN)
        float slideInDuration = 0.35f; // Çok daha hızlı ve sert kapansın
        float elapsed = 0f;
        while(elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideInDuration;
            float easeT = 1f - Mathf.Pow(1f - t, 4f); 
            
            leftRt.anchoredPosition = new Vector2(Mathf.Lerp(-offscreenOffset, 0, easeT), 0);
            rightRt.anchoredPosition = new Vector2(Mathf.Lerp(offscreenOffset, 0, easeT), 0);
            yield return null;
        }

        leftRt.anchoredPosition = Vector2.zero;
        rightRt.anchoredPosition = Vector2.zero;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelCompleteSound(); // Tek Zen sesi: Kapılar kapandığı an çalar

        // METİN GRUBU (Tüm ekranı kaplaması için RectTransform eklendi!)
        GameObject textParent = new GameObject("TextGroup");
        textParent.transform.SetParent(canvas.transform, false);
        RectTransform tpRt = textParent.AddComponent<RectTransform>();
        tpRt.anchorMin = Vector2.zero;
        tpRt.anchorMax = Vector2.one;
        tpRt.sizeDelta = Vector2.zero;
        tpRt.anchoredPosition = Vector2.zero;

        CanvasGroup textGroup = textParent.AddComponent<CanvasGroup>();
        textGroup.alpha = 1f; // Başlangıçta görünür

        // BAŞLIK (DAMGA)
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(textParent.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        // Başlığı çok daha büyümesi için alanını genişletip hafif yukarı alıyoruz
        // X ekseninde 0.15 ve 0.85 yaparak kırmızı Torii sütunlarına taşmasını KESİNLİKLE engelliyoruz
        titleRect.anchorMin = new Vector2(0.15f, 0.60f); 
        titleRect.anchorMax = new Vector2(0.85f, 0.90f);
        titleRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = titles[Random.Range(0, titles.Length)];
        titleText.fontSize = 220; // Çok daha büyük
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 100;
        titleText.fontSizeMax = 280;
        titleText.fontStyle = FontStyles.Italic | FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.85f, 0.15f, 0.15f, 0f); // Başlangıçta şeffaf
        
        Shadow titleShadow = titleObj.AddComponent<Shadow>();
        titleShadow.effectColor = new Color(0, 0, 0, 0.4f);
        titleShadow.effectDistance = new Vector2(4, -4);

        // ALT YAZI (FIRÇA / TYPEWRITER)
        GameObject provObj = new GameObject("Proverb");
        provObj.transform.SetParent(textParent.transform, false);
        RectTransform provRect = provObj.AddComponent<RectTransform>();
        // Atasözünü çok aşağıdan kurtarıp, hafif ortaya ve yukarı taşıyoruz
        provRect.anchorMin = new Vector2(0.15f, 0.30f); 
        provRect.anchorMax = new Vector2(0.85f, 0.55f);
        provRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI provText = provObj.AddComponent<TextMeshProUGUI>();
        string fullProverb = proverbs[Random.Range(0, proverbs.Length)];
        provText.text = ""; // Başlangıçta boş
        provText.fontSize = 45; // Boyutunu küçülttük ki zıtlık oluşsun
        provText.enableAutoSizing = true; 
        provText.fontSizeMin = 25;
        provText.fontSizeMax = 55;
        provText.alignment = TextAlignmentOptions.Top;
        provText.color = new Color(0.1f, 0.1f, 0.1f, 1f); 

        // DAMGA ANİMASYONU (Sadece Başlık)
        Vector3 stampStartScale = new Vector3(3f, 3f, 1f); 
        titleRect.localScale = stampStartScale;

        float stampDuration = 0.2f; // Damga çok hızlı vurulur
        elapsed = 0f;
        while(elapsed < stampDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / stampDuration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            titleRect.localScale = Vector3.Lerp(stampStartScale, Vector3.one, easeT);
            // Kırmızı renk aniden belirsin
            titleText.color = new Color(0.85f, 0.15f, 0.15f, easeT);
            yield return null;
        }
        titleRect.localScale = Vector3.one;
        titleText.color = new Color(0.85f, 0.15f, 0.15f, 1f);

        // FIRÇA EFEKTİ (Daktilo / Typewriter)
        float typingDuration = 0.8f; // Fırçayla yazılma süresi
        float timePerChar = typingDuration / fullProverb.Length;
        for (int i = 0; i < fullProverb.Length; i++)
        {
            provText.text += fullProverb[i];
            yield return new WaitForSeconds(timePerChar);
        }

        // Okumak için bekle
        yield return new WaitForSeconds(1.5f);

        // ARKADA YENİ BÖLÜMÜ YÜKLE
        if (onCompleteCallback != null) onCompleteCallback.Invoke();
        yield return new WaitForSeconds(0.2f);

        // Kapıların açılma sesi iptal edildi, artık tek ses kapanma anında çalıyor.

        // KAPILARIN AÇILMA (SLIDE OUT) ANİMASYONU
        float slideOutDuration = 0.5f;
        elapsed = 0f;
        while(elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideOutDuration;
            float easeT = t * t * (3f - 2f * t); 
            
            textGroup.alpha = 1f - (t * 4f); // Metinler anında kaybolur
            
            leftRt.anchoredPosition = new Vector2(Mathf.Lerp(0, -offscreenOffset, easeT), 0);
            rightRt.anchoredPosition = new Vector2(Mathf.Lerp(0, offscreenOffset, easeT), 0);
            yield return null;
        }

        Destroy(gameObject);
    }

    private GameObject CreateShojiDoor(Transform parent, bool isLeft)
    {
        GameObject doorObj = new GameObject(isLeft ? "ShojiLeft" : "ShojiRight");
        doorObj.transform.SetParent(parent, false);
        RectTransform rt = doorObj.AddComponent<RectTransform>();
        
        Image paper = doorObj.AddComponent<Image>();
        paper.color = new Color(0.96f, 0.94f, 0.90f); 
        
        if (isLeft)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(1, 0.5f); 
        }
        else
        {
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 0.5f); 
        }
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        
        // Torii kapısı kırmızısı
        Color woodColor = new Color(0.25f, 0.05f, 0.05f); 
        
        // ORTADAKİ DİKEY ÇİZGİYİ TAMAMEN SİLDİK! (Yazıların okunmasını engelliyordu)
        // Artık kapılar birleştiğinde ortası tamamen pürüzsüz pirinç kağıdı olacak.

        // Dış kenar direği (Torii kapısının devasa yan sütunları gibi kalınlaştırıldı)
        GameObject outFrame = new GameObject("OutFrame");
        outFrame.transform.SetParent(doorObj.transform, false);
        RectTransform outRt = outFrame.AddComponent<RectTransform>();
        Image outImg = outFrame.AddComponent<Image>();
        outImg.color = woodColor;
        
        outRt.anchorMin = isLeft ? new Vector2(0, 0) : new Vector2(1, 0);
        outRt.anchorMax = isLeft ? new Vector2(0, 1) : new Vector2(1, 1);
        outRt.pivot = isLeft ? new Vector2(0, 0.5f) : new Vector2(1, 0.5f);
        outRt.sizeDelta = new Vector2(60, 0); // Kalın devasa Torii sütunu
        outRt.anchoredPosition = Vector2.zero;

        // Üst ve Alt Çerçeveler (Torii'nin çatısı gibi üstü devasa yaptık)
        CreateHorizontalRib(doorObj.transform, woodColor, 1f, 120); // Üst tahta (Torii Çatısı)
        CreateHorizontalRib(doorObj.transform, woodColor, 0f, 40); // Alt zemin tahtası

        // Kapının ortasını tamamen BOŞ bıraktık. İç ızgara yok, orta çizgi yok.
        // Tam bir Japon kaligrafi tuvali!

        return doorObj;
    }

    private void CreateHorizontalRib(Transform parent, Color color, float yAnchor, float height)
    {
        GameObject rib = new GameObject("HorizontalRib");
        rib.transform.SetParent(parent, false);
        RectTransform ribRt = rib.AddComponent<RectTransform>();
        Image ribImg = rib.AddComponent<Image>();
        ribImg.color = color;
        
        ribRt.anchorMin = new Vector2(0, yAnchor);
        ribRt.anchorMax = new Vector2(1, yAnchor);
        ribRt.pivot = new Vector2(0.5f, yAnchor);
        ribRt.sizeDelta = new Vector2(0, height); 
        ribRt.anchoredPosition = Vector2.zero;
    }
}
