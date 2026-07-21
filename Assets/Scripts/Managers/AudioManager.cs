using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip katanaSlashSound;
    public AudioClip errorStoneSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySlashSound()
    {
        if (katanaSlashSound != null && sfxSource != null)
        {
            // Pitch'i hafif değiştirerek her kesişin farklı duyulmasını sağla (Game Juice)
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(katanaSlashSound);
        }
        else
        {
            Debug.LogWarning("[AudioManager] Katana Slash sound or source is missing!");
        }
    }

    public void PlayErrorSound()
    {
        if (errorStoneSound != null && sfxSource != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.0f);
            sfxSource.PlayOneShot(errorStoneSound);
        }
        else
        {
            Debug.LogWarning("[AudioManager] Error Stone sound or source is missing!");
        }
    }
}
