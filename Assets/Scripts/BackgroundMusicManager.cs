using UnityEngine;
using System.Collections.Generic;

public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Плейлист")]
    public List<AudioClip> playlist = new List<AudioClip>();

    [Header("Настройки")]
    [Range(0f, 1f)] public float volume = 0.5f;
    public bool playOnAwake = true;
    public bool shuffle = true;           // Перемешивать ли треки
    public float crossfadeTime = 1.5f;     // Плавный переход между треками (в секундах)

    private AudioSource audioSource;
    private int currentTrackIndex = 0;
    private bool isPlaying = false;

    private void Awake()
    {
        // Создаём AudioSource если его нет
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = false;
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (playlist.Count == 0)
        {
            Debug.LogWarning("BackgroundMusicManager: Плейлист пуст!");
            return;
        }

        if (shuffle)
            ShufflePlaylist();

        if (playOnAwake)
            PlayNextTrack();
    }

    private void Update()
    {
        if (isPlaying && !audioSource.isPlaying)
        {
            PlayNextTrack();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            PlayNextTrack();
        }
    }

    public void PlayNextTrack()
    {
        if (playlist.Count == 0) return;

        currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;

        audioSource.clip = playlist[currentTrackIndex];
        audioSource.Play();
        isPlaying = true;

        Debug.Log($"▶ Играет трек: {playlist[currentTrackIndex].name} ({currentTrackIndex + 1}/{playlist.Count})");
    }

    public void PlayPreviousTrack()
    {
        if (playlist.Count == 0) return;

        currentTrackIndex = (currentTrackIndex - 1 + playlist.Count) % playlist.Count;

        audioSource.clip = playlist[currentTrackIndex];
        audioSource.Play();
        isPlaying = true;
    }

    private void ShufflePlaylist()
    {
        for (int i = playlist.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (playlist[i], playlist[j]) = (playlist[j], playlist[i]);
        }
        currentTrackIndex = 0;
    }

    public void Play()
    {
        if (!audioSource.isPlaying)
            audioSource.Play();
        isPlaying = true;
    }

    public void Pause()
    {
        audioSource.Pause();
        isPlaying = false;
    }

    public void Stop()
    {
        audioSource.Stop();
        isPlaying = false;
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }

    public AudioClip GetCurrentTrack() => audioSource.clip;
    public int GetCurrentTrackIndex() => currentTrackIndex;
}