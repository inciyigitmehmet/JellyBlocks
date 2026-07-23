using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip katanaSlashSound;
    public AudioClip errorStoneSound;
    public AudioClip doorCloseSound; 
    public AudioClip doorOpenSound; // YENİ: Shoji Kapısı açılma sesi
    public AudioClip levelCompleteSound; // YENİ: Tek "Level Complete" Zen sesi

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

    public void PlayDoorCloseSound()
    {
        if (doorCloseSound != null && sfxSource != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(doorCloseSound);
        }
        else
        {
            PlayErrorSound(); 
            Debug.LogWarning("[AudioManager] Door Close sound is missing! Falling back to Error Stone sound.");
        }
    }

    public void PlayDoorOpenSound()
    {
        if (doorOpenSound != null && sfxSource != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(doorOpenSound);
        }
        else
        {
            Debug.LogWarning("[AudioManager] Door Open sound is missing!");
        }
    }

    public void PlayLevelCompleteSound()
    {
        if (levelCompleteSound != null && sfxSource != null)
        {
            sfxSource.pitch = 1f; // Zen sesi bozulmasın
            sfxSource.PlayOneShot(levelCompleteSound);
        }
        else
        {
            // Henüz ses atanmadıysa eski kapı kapanma sesine geri dön
            PlayDoorCloseSound();
        }
    }
}
