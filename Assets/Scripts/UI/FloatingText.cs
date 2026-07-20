using UnityEngine;
using TMPro;
using System.Collections;

// Ekranda belirip yavaşça yukarı kayarak solan kutlama yazıları için (Örn: "Nice!", "Perfect!")
public class FloatingText : MonoBehaviour
{
    [Header("Animasyon Ayarları")]
    public float floatSpeed = 1.5f;     // Yazının yukarı kayma hızı
    public float fadeDuration = 1.2f;   // Yazının ekranda kalma ve solma süresi

    private TextMeshPro textMesh;

    // Yazı içeriğini ve rengini dışarıdan (Manager'lardan) ayarlamak için fonksiyon
    public void Initialize(string text, Color color)
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            
            // Başlar başlamaz yukarı kayıp solma işlemini başlat
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            Debug.LogWarning("FloatingText objesinde TextMeshPro bileşeni bulunamadı!");
            Destroy(gameObject); // Hatalıysa sahnede çöp bırakmamak için yok et
        }
    }

    void Update()
    {
        // Her karede yazıyı yavaşça yukarı taşı (Frame rate bağımsız olması için Time.deltaTime ile çarpıyoruz)
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }

    IEnumerator FadeOutAndDestroy()
    {
        if (textMesh == null) yield break;

        Color startColor = textMesh.color;
        float elapsedTime = 0f;

        // Belirlenen süre boyunca şeffaflığı (Alpha) 1'den 0'a doğru azalt
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Animasyon bitince objeyi sahnede kalabalık etmemesi için tamamen yok et
        Destroy(gameObject);
    }
}
