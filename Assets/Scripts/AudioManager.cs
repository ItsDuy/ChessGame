using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioSource backgroundMusic;
    public AudioSource sfx;
    public AudioClip moveSound;
    public AudioClip checkSound;
    public AudioClip captureSound;
    public AudioClip winSound;
    public AudioClip promotionSound;
    public AudioClip castlingSound;
    public AudioClip buttonClickSound;
    private void Awake()
    {
        // Singleton pattern implementation
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
    private void Start()
    {
        // Optional: Automatically start background music when AudioManager is created
        PlayBackgroundMusic();
    }

    private bool VerifyAudioSources()
    {
        if (sfx == null)
        {
            Debug.LogWarning("SFX Audio is null, recreating it");
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
        }
        if (backgroundMusic == null)
        {
            Debug.LogWarning("Background music AudioSource is null, recreating it");
            backgroundMusic = gameObject.AddComponent<AudioSource>();
            backgroundMusic.loop = true;
            backgroundMusic.playOnAwake = false;
        }

        return sfx != null && backgroundMusic != null;
    }
    public void PlayMoveSound()
    {
        if (!VerifyAudioSources() || moveSound == null) return;
        sfx.clip = moveSound;
        sfx.Play();
    }

    public void PlayCaptureSound()
    {
        if (!VerifyAudioSources() || captureSound == null) return;
        sfx.clip = captureSound;
        sfx.Play();
    }

    public void PlayCheckSound()
    {
        if (!VerifyAudioSources() || checkSound == null) return;
        sfx.clip = checkSound;
        sfx.Play();
    }

    public void PlayPromotionSound()
    {
        if (!VerifyAudioSources() || promotionSound == null) return;
        sfx.clip = promotionSound;
        sfx.Play();
    }
    public void PlayWinSound()
    {
        if (!VerifyAudioSources() || winSound == null) return;
        sfx.clip = winSound;
        sfx.Play();
    }

    public void PlayCastlingSound()
    {
        if (!VerifyAudioSources() || castlingSound == null) return;
        sfx.clip = castlingSound;
        sfx.Play();
    }
    public void PlayBackgroundMusic()
    {
        if (!VerifyAudioSources()) return;

        // Only start if not already playing
        if (backgroundMusic.isPlaying) return;
        backgroundMusic.Play();
    }
    public void PlayButtonClickSound()
    {
        if (!VerifyAudioSources() || buttonClickSound == null) return;
        sfx.PlayOneShot(buttonClickSound);
    }
    // Add to AudioManager class
    public void ResetAudioState()
    {
        if (!VerifyAudioSources()) return;
        // Stop any ongoing sounds if needed
        if (sfx != null && sfx.isPlaying)
        {
            sfx.Stop();
        }

        // Make sure background music is playing
        PlayBackgroundMusic();
    }
}
