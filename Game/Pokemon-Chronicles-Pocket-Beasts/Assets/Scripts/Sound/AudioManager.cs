using UnityEngine;
using System.Collections;

/// <summary>
/// Gestor centralizado de audio que controla música de fondo y efectos de sonido.
/// Implementa el patrón Singleton para acceso global desde cualquier parte del juego.
/// Gestiona las transiciones suaves entre pistas musicales y la reproducción de efectos.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip townMusic;
    [SerializeField] private AudioClip battleMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip selectionSound;
    [SerializeField] private AudioClip grassStepSound;

    [Header("Settings")]
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float sfxVolume = 0.7f;
    [SerializeField] private float fadeDuration = 1.0f;

    private Coroutine fadeCoroutine;

    /// <summary>
    /// Inicializa el singleton y configura los volúmenes iniciales.
    /// Persiste entre escenas para mantener la música continua.
    /// </summary>
    void Awake()
    {
        // Implementar Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Configurar volúmenes iniciales
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            musicSource.loop = true;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    /// <summary>
    /// Reproduce la música del menú principal con transición suave.
    /// </summary>
    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    /// <summary>
    /// Reproduce la música del pueblo con transición suave.
    /// </summary>
    public void PlayTownMusic()
    {
        PlayMusic(townMusic);
    }

    /// <summary>
    /// Reproduce la música de batalla con transición suave.
    /// </summary>
    public void PlayBattleMusic()
    {
        PlayMusic(battleMusic);
    }

    /// <summary>
    /// Cambia la pista musical actual con un fade suave.
    /// Detiene cualquier transición en curso antes de iniciar una nueva.
    /// </summary>
    /// <param name="clip">Clip de audio a reproducir</param>
    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;

        // Si ya está sonando esta música, no hacer nada
        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        // Detener fade anterior si existe
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeMusic(clip));
    }

    /// <summary>
    /// Corrutina que realiza un fade out de la música actual y fade in de la nueva.
    /// Crea transiciones suaves entre diferentes temas musicales.
    /// </summary>
    /// <param name="newClip">Nueva pista musical a reproducir</param>
    private IEnumerator FadeMusic(AudioClip newClip)
    {
        // Fade out de la música actual
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                yield return null;
            }

            musicSource.Stop();
        }

        // Cambiar clip y hacer fade in
        musicSource.clip = newClip;
        musicSource.Play();

        float targetVolume = musicVolume;
        float elapsedIn = 0f;

        while (elapsedIn < fadeDuration)
        {
            elapsedIn += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsedIn / fadeDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        fadeCoroutine = null;
    }

    /// <summary>
    /// Reproduce el sonido de selección en menús.
    /// Utilizado tanto en el menú principal como en las interfaces del juego.
    /// </summary>
    public void PlaySelectionSound()
    {
        PlaySFX(selectionSound);
    }

    /// <summary>
    /// Reproduce el sonido de pasos sobre hierba.
    /// Debe invocarse cuando el jugador camina por zonas de hierba alta.
    /// </summary>
    public void PlayGrassStepSound()
    {
        PlaySFX(grassStepSound);
    }

    /// <summary>
    /// Método genérico para reproducir efectos de sonido.
    /// Utiliza un AudioSource dedicado para no interrumpir la música.
    /// </summary>
    /// <param name="clip">Clip de audio a reproducir</param>
    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Detiene toda la música con fade out suave.
    /// </summary>
    public void StopMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutMusic());
    }

    /// <summary>
    /// Corrutina que reduce gradualmente el volumen hasta detener la música.
    /// </summary>
    private IEnumerator FadeOutMusic()
    {
        if (!musicSource.isPlaying)
            yield break;

        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume;
        fadeCoroutine = null;
    }

    /// <summary>
    /// Ajusta el volumen de la música.
    /// </summary>
    /// <param name="volume">Volumen entre 0 y 1</param>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Ajusta el volumen de los efectos de sonido.
    /// </summary>
    /// <param name="volume">Volumen entre 0 y 1</param>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
}