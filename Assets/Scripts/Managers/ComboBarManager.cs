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
    [SerializeField] private Color comboTextColor    = new Color(1f, 0.85f, 0.1f, 1f);

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

    // -----------------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (barFill != null) barFill.fillAmount = 0f;
        UpdateComboLabel();
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
            SpawnComboText(fillPosition);
        }
        else
        {
            CurrentCombo = 1;
        }

        UpdateComboLabel();

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
    private void StartDraining()
    {
        if (barFill == null) return;
        _drainTween = barFill
            .DOFillAmount(0f, drainDuration)
            .SetEase(Ease.Linear)
            .OnComplete(OnBarDrained);
    }

    private void OnBarDrained()
    {
        CurrentCombo = 1;
        UpdateComboLabel();
    }

    // -----------------------------------------------------------------------
    private void UpdateComboLabel()
    {
        if (comboLabel == null) return;
        comboLabel.text = CurrentCombo > 1 ? $"{CurrentCombo}x" : "";

        if (CurrentCombo > 1)
        {
            // Küçükten büyüyüp oturan "pop" efekti
            comboLabel.transform.localScale = Vector3.one * 0.5f;
            comboLabel.transform
                .DOScale(Vector3.one, 0.25f)
                .SetEase(Ease.OutBack);
        }
    }

    // -----------------------------------------------------------------------
    private void SpawnComboText(Vector3 worldPos)
    {
        if (floatingTextPrefab == null) return;

        Vector3 spawnPos = worldPos == default
            ? Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.6f, 10f))
            : worldPos + new Vector3(0f, 0.5f, -2f);

        GameObject obj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
        FloatingText ft = obj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.floatSpeed   = comboFloatSpeed;
            ft.fadeDuration = comboFadeDuration;
            ft.Initialize($"{CurrentCombo}x COMBO!", comboTextColor);
        }
    }

    // -----------------------------------------------------------------------
    public void ResetCombo()
    {
        _drainTween?.Kill();
        _fillTween?.Kill();
        CurrentCombo = 1;
        if (barFill != null) barFill.DOFillAmount(0f, 0.2f);
        UpdateComboLabel();
    }
}
