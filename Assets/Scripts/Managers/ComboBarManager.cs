using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Ekrandaki Combo Bar'ı yönetir.
/// DOTween Free ile çalışır — DOFillAmount, DOColor, DOTween.To kullanılır.
/// </summary>
public class ComboBarManager : MonoBehaviour
{
    // --- Singleton ---
    public static ComboBarManager Instance { get; private set; }

    // --- Inspector Ayarları ---
    [Header("Bar Animasyon Ayarları")]
    [SerializeField] private float fillAnimDuration = 0.25f;
    [SerializeField] private float drainDuration    = 5.0f;
    [SerializeField] private Ease  fillEase         = Ease.OutQuad;

    [Header("Combo Text Ayarları")]
    [SerializeField] private float comboFloatSpeed   = 2.8f;
    [SerializeField] private float comboFadeDuration = 1.5f;

    [Header("UI Referansları")]
    [SerializeField] private Image            barFill;
    [SerializeField] private Image            barBackground;
    [SerializeField] private TextMeshProUGUI  comboLabel;

    [Header("FloatingText Prefab")]
    [SerializeField] private GameObject floatingTextPrefab;

    // --- Public ---
    public int CurrentCombo { get; private set; } = 1;

    // --- Private ---
    private Tweener _drainTween;
    private Tweener _fillTween;
    private bool _isPaused = false;
    private Vector2 _originalComboLabelPos;
    private Vector3 _originalComboLabelScale;

    // -----------------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (barFill != null) barFill.fillAmount = 0f;
        if (comboLabel != null) 
        {
            _originalComboLabelPos = comboLabel.rectTransform.anchoredPosition;
            _originalComboLabelScale = comboLabel.transform.localScale;
            comboLabel.text = "";
        }
    }

    // -----------------------------------------------------------------------
    /// <summary>
    /// Selection_Manager başarılı doldurma sonrası çağırır.
    /// ScoreManager.AddScore'dan ÖNCE çağrılmalı.
    /// </summary>
    public void OnSuccessfulFill(Color rectangleColor, Vector3 fillPosition = default)
    {
        // Aktif tweenleri öldür
        _drainTween?.Kill();
        _fillTween?.Kill();

        bool barWasActive = barFill != null && barFill.fillAmount > 0.01f;

        if (barWasActive)
        {
            CurrentCombo++;
            SpawnComboText(fillPosition, rectangleColor);
        }
        else
        {
            CurrentCombo = 1;
        }

        UpdateComboLabel(rectangleColor);

        if (barFill != null)
        {
            SetFillColor(rectangleColor);

            // Hızlı dolum (OutQuad)
            _fillTween = barFill
                .DOFillAmount(1f, fillAnimDuration)
                .SetEase(fillEase)
                .OnComplete(StartDraining);
        }
    }

    // -----------------------------------------------------------------------
    /// <summary>
    /// KatanaFill (veya herhangi bir bar) rengini dışarıdan ayarlamak için.
    /// </summary>
    public void SetFillColor(Color c)
    {
        if (barFill != null)
        {
            barFill.color = c;
        }
    }

    // -----------------------------------------------------------------------
    public void PauseDrain()
    {
        _isPaused = true;
        _drainTween?.Pause();
    }

    public void ResumeDrain()
    {
        _isPaused = false;
        _drainTween?.Play();
    }

    private void StartDraining()
    {
        if (barFill == null) return;
        _drainTween = barFill
            .DOFillAmount(0f, drainDuration)
            .SetEase(Ease.Linear)
            .OnComplete(OnBarDrained);

        // Eğer oyun duraklatılmışsa (geçiş ekranındaysa) yeni başlayan drain'i anında dondur
        if (_isPaused)
        {
            _drainTween.Pause();
        }
    }

    private void OnBarDrained()
    {
        CurrentCombo = 1;
        UpdateComboLabel(Color.white);
    }

    // -----------------------------------------------------------------------
    private void UpdateComboLabel(Color c)
    {
        if (comboLabel == null) return;

        comboLabel.DOKill(); // Aktif animasyonları durdur
        comboLabel.text = CurrentCombo > 1 ? $"{CurrentCombo}x" : "";
        
        c.a = 1f;
        comboLabel.color = c;

        if (CurrentCombo > 1)
        {
            // Pozisyonu ve rotasyonu orijinal yerine al
            comboLabel.rectTransform.anchoredPosition = _originalComboLabelPos;
            comboLabel.transform.localRotation = Quaternion.identity;
            
            // "Pop" büyüme efekti
            comboLabel.transform.localScale = _originalComboLabelScale * 0.5f;
            comboLabel.transform.DOScale(_originalComboLabelScale, 0.3f).SetEase(Ease.OutBack);
            
            // --- YENİ: Titreme (Shake) ve Rotasyon (Punch) Efekti ---
            // Z ekseninde hafifçe sağa sola sallanarak (titreyerek) çıkar
            comboLabel.transform.DOPunchRotation(new Vector3(0, 0, 20f), 0.5f, 12, 1f);
            
            // Ayrıca pozisyon olarak da hafif titrer (punch)
            comboLabel.rectTransform.DOPunchAnchorPos(new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f)), 0.4f, 10, 1f);

            // Havaya süzülüp solma efekti!
            comboLabel.rectTransform.DOAnchorPosY(_originalComboLabelPos.y + 60f, 0.75f).SetEase(Ease.OutQuad);
            comboLabel.DOFade(0f, 0.6f).SetEase(Ease.InQuad).SetDelay(0.15f);
        }
    }

    // -----------------------------------------------------------------------
    private void SpawnComboText(Vector3 worldPos, Color c)
    {
        if (floatingTextPrefab == null) return;

        Vector3 spawnPos = worldPos == default
            ? Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.6f, 10f))
            : worldPos + new Vector3(0f, 0.5f, -2f);

        GameObject obj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);

        // --- BOYUT HESAPLAMASI (CurrentCombo'ya göre büyüyen yazılar) ---
        float scaleMult = 0.6f; // 1-4 arası (küçük)
        if (CurrentCombo >= 20) scaleMult = 2.0f; // max boyut
        else if (CurrentCombo >= 10) scaleMult = 1.5f; // büyük
        else if (CurrentCombo >= 5) scaleMult = 1.0f; // orta
        
        obj.transform.localScale = Vector3.one * scaleMult;

        FloatingText ft = obj.GetComponent<FloatingText>();
        if (ft != null)
        {
            c.a = 1f; // Opaklığı tam yapıyoruz ki silik çıkmasın
            ft.floatSpeed   = comboFloatSpeed;
            ft.fadeDuration = comboFadeDuration;
            ft.Initialize($"{CurrentCombo}x COMBO!", c);
        }
    }

    // -----------------------------------------------------------------------
    public void ResetCombo()
    {
        _drainTween?.Kill();
        _fillTween?.Kill();
        CurrentCombo = 1;
        if (barFill != null) barFill.DOFillAmount(0f, 0.2f);
        UpdateComboLabel(Color.white);
    }
}
