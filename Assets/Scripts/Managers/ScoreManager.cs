using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Puan sistemini yönetir.
/// DOTween Free ile çalışır — DOTween.To kullanılır.
/// GELECEK: GetFinalScore() → LeaderboardManager / SaveManager entegrasyonuna hazır.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // --- Singleton ---
    public static ScoreManager Instance { get; private set; }

    // --- Inspector ---
    [Header("Puan Ayarları")]
    [SerializeField] private int pointsPerCell = 10;

    [Header("UI Referansları")]
    [SerializeField] private TextMeshProUGUI scoreLabel;

    [Header("Animasyon")]
    [SerializeField] private float countDuration = 0.4f;
    [SerializeField] private Ease  countEase     = Ease.OutCubic;

    // --- Private ---
    private int     _totalScore     = 0;
    private int     _displayedScore = 0;
    private Tweener _countTween;

    // -----------------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => RefreshDisplay();

    // -----------------------------------------------------------------------
    /// <summary>
    /// Selection_Manager başarılı hamle sonrası çağırır.
    /// ComboBarManager.OnSuccessfulFill zaten çağrılmış olmalı ki doğru multiplier gelsin.
    /// </summary>
    public void AddScore(int cellCount, Color blockColor)
    {
        int combo  = ComboBarManager.Instance != null ? ComboBarManager.Instance.CurrentCombo : 1;
        int gained = cellCount * pointsPerCell * combo;
        _totalScore += gained;
        
        // Puan yazısının rengini jölenin rengine boyuyoruz
        if (scoreLabel != null)
        {
            blockColor.a = 1f;
            scoreLabel.color = blockColor;
        }

        AnimateScore();
    }

    // -----------------------------------------------------------------------
    public void ResetScore()
    {
        _countTween?.Kill();
        _totalScore     = 0;
        _displayedScore = 0;
        RefreshDisplay();
    }

    // -----------------------------------------------------------------------
    /// <summary>
    /// Leaderboard / SaveManager entegrasyonu için.
    /// Gelecekte: SaveManager.Save(ScoreManager.Instance.GetFinalScore())
    /// </summary>
    public int GetFinalScore() => _totalScore;

    // -----------------------------------------------------------------------
    private void AnimateScore()
    {
        _countTween?.Kill();

        // DOTween.To ile _displayedScore'u hedef puana animasyonlu çek
        _countTween = DOTween
            .To(() => _displayedScore, x => _displayedScore = x, _totalScore, countDuration)
            .SetEase(countEase)
            .OnUpdate(RefreshDisplay);
    }

    private void RefreshDisplay()
    {
        if (scoreLabel != null)
            scoreLabel.text = _displayedScore.ToString("N0");
    }
}
