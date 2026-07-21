using UnityEngine;
using UnityEngine.UI;

public class LifeUIManager : MonoBehaviour
{
    public static LifeUIManager Instance;
    
    [Header("Katana Icons (Assign in Inspector)")]
    public Image[] lifeIcons;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void UpdateLifeUI(int currentLives, int maxLives)
    {
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
            {
                Color c = lifeIcons[i].color;
                if (i < currentLives)
                {
                    c.a = 1f; // Tam dolu can (Opak)
                }
                else
                {
                    c.a = 0.3f; // Kaybedilmiş can (Silik)
                }
                lifeIcons[i].color = c;
            }
        }
    }
}
