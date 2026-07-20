using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Bölüm sonu Atasözlü ve Daktilo Efektli Zen Geçiş Ekranı
public class ZenLevelCompleteScreen : MonoBehaviour
{
    private string[] titles = { 
        "Immaculate!", 
        "Flawless!", 
        "On Fire!", 
        "Zen Mastery!", 
        "Lightning Fast!", 
        "Perfect Slice!" 
    };
    
    private string[] proverbs = {
        "Fall seven times, stand up eight.",
        "The bamboo that bends is stronger than the oak that resists.",
        "One kind word can warm three winter months.",
        "Continuous improvement is better than delayed perfection.",
        "Even a journey of a thousand miles begins with a single step."
    };

    public void ShowScreen(System.Action onComplete)
    {
        StartCoroutine(ScreenRoutine(onComplete));
    }

    private IEnumerator ScreenRoutine(System.Action onComplete)
    {
        // 1. KANVAS VE ARKA PLAN
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        gameObject.AddComponent<CanvasScaler>();

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.96f, 0.95f, 0.90f, 0f); // Krem, başlangıçta saydam

        // Arka planı yumuşakça karart (Fade in)
        float elapsed = 0;
        while(elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            bgImg.color = new Color(0.96f, 0.95f, 0.90f, Mathf.Lerp(0f, 0.95f, elapsed/0.5f));
            yield return null;
        }

        // 2. BAŞLIK (DAKTİLO EFEKTİ)
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        // Kırmızı yazıyı biraz daha yukarı aldık
        titleRect.anchorMin = new Vector2(0f, 0.6f);
        titleRect.anchorMax = new Vector2(1f, 0.75f);
        titleRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        string selectedTitle = titles[Random.Range(0, titles.Length)];
        titleText.text = "";
        titleText.fontSize = 110;
        titleText.fontStyle = FontStyles.Italic | FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Bottom; // Alta yaslı ki alttaki yazıyla grup dursun
        titleText.color = new Color(0.9f, 0.3f, 0.3f, 1f); // Mercan / Kırmızı

        // Harf harf yazma (Typewriter effect)
        WaitForSeconds letterWait = new WaitForSeconds(0.05f); // Her harf arası 50 milisaniye
        for(int i = 0; i <= selectedTitle.Length; i++)
        {
            titleText.text = selectedTitle.Substring(0, i);
            yield return letterWait;
        }

        yield return new WaitForSeconds(0.2f); // Atasözünden önce ufak bir dramatik nefes

        // 3. ATASÖZÜ (YAVAŞÇA BELİRME / FADE IN)
        GameObject provObj = new GameObject("Proverb");
        provObj.transform.SetParent(transform, false);
        RectTransform provRect = provObj.AddComponent<RectTransform>();
        // Atasözünü biraz daha aşağı alarak araya boşluk (nefes) ekledik
        provRect.anchorMin = new Vector2(0.1f, 0.35f);
        provRect.anchorMax = new Vector2(0.9f, 0.5f);
        provRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI provText = provObj.AddComponent<TextMeshProUGUI>();
        provText.text = proverbs[Random.Range(0, proverbs.Length)];
        provText.fontSize = 50;
        provText.alignment = TextAlignmentOptions.Top; // Üste yaslı ki başlığa yakın dursun
        provText.color = new Color(0.2f, 0.2f, 0.2f, 0f); // Başlangıçta saydam

        elapsed = 0;
        while(elapsed < 0.6f)
        {
            elapsed += Time.deltaTime;
            provText.color = new Color(0.2f, 0.2f, 0.2f, Mathf.Lerp(0f, 1f, elapsed/0.6f));
            yield return null;
        }

        // Ekranı okuması için huzurlu bir bekleyiş (2.5 saniye)
        yield return new WaitForSeconds(2.5f);

        // 4. EKRANI YAVAŞÇA SİL VE DİĞER BÖLÜMÜ YÜKLE
        // Ekran silinmeye başlamadan hemen önce arkaplanda yeni bölümü yüklüyoruz ki geçiş pürüzsüz olsun
        if (onComplete != null) onComplete.Invoke();

        elapsed = 0;
        while(elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed/0.5f);
            bgImg.color = new Color(0.96f, 0.95f, 0.90f, alpha * 0.95f);
            titleText.color = new Color(0.9f, 0.3f, 0.3f, alpha);
            provText.color = new Color(0.2f, 0.2f, 0.2f, alpha);
            yield return null;
        }

        Destroy(gameObject); // Kendimizi yok edelim
    }
}
