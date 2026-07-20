using UnityEngine;
using System.Collections;

// Katana Kesiği efekti: Kılıç çekilmiş gibi bir parıltı çıkar, uzar ve solar.
public class KatanaSlash : MonoBehaviour
{
    public void PlaySlash(Vector2 size)
    {
        StartCoroutine(SlashRoutine(size));
    }

    private IEnumerator SlashRoutine(Vector2 size)
    {
        GameObject slashObj = new GameObject("SlashLine");
        slashObj.transform.position = transform.position;
        slashObj.transform.SetParent(transform);

        LineRenderer lr = slashObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        lr.positionCount = 2;
        
        // Unity'nin default materyali ile çok parlak bir çizgi
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(1f, 1f, 1f, 1f);
        lr.endColor = new Color(1f, 1f, 1f, 1f);
        lr.sortingOrder = 50; // Jölelerin ve her şeyin üstünde

        // Rastgele çapraz kılıç açısı (20 ile 70 derece arası)
        float angle = Random.Range(20f, 70f);
        if (Random.value > 0.5f) angle = -angle; // Bazen diğer tarafa çapraz
        
        // Bloğun köşegen uzunluğunu bul, kesik bloktan biraz daha uzun olsun
        float diagonal = Mathf.Sqrt(size.x * size.x + size.y * size.y);
        float targetLength = diagonal * 1.5f;

        Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;

        // 1. AŞAMA: Şimşek hızında uzama (0.05 saniye)
        float duration = 0.05f;
        float elapsed = 0f;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentLength = Mathf.Lerp(0, targetLength, elapsed / duration);
            lr.SetPosition(0, -dir * (currentLength / 2f));
            lr.SetPosition(1, dir * (currentLength / 2f));
            yield return null;
        }

        // 2. AŞAMA: Yavaşça solarak kaybolma (0.2 saniye)
        duration = 0.2f;
        elapsed = 0f;
        Color startColor = lr.startColor;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            startColor.a = alpha;
            lr.startColor = startColor;
            lr.endColor = startColor;
            
            // Solarken kılıç izi hafifçe incelsin
            float currentWidth = Mathf.Lerp(0.08f, 0f, elapsed / duration);
            lr.startWidth = currentWidth;
            lr.endWidth = currentWidth;
            yield return null;
        }

        Destroy(gameObject); // Efekt bitince kendini yok et
    }
}
