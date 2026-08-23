using System.Collections.Generic;
using UnityEngine;

namespace ArrowsPuzzle.Systems
{
    /// <summary>
    /// Manages SFX pool and background music.
    /// Attach to a persistent GameObject in the first scene.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("SFX Clips – assign in Inspector")]
        // TODO: Replace these placeholder clip references with real audio assets
        public AudioClip sfxSlide;
        public AudioClip sfxThud;
        public AudioClip sfxWin;
        public AudioClip sfxUnlock;
        public AudioClip sfxCoin;
        public AudioClip sfxStarChime;
        public AudioClip sfxButtonTap;
        public AudioClip sfxLock;
        public AudioClip sfxConfetti;
        public AudioClip sfxIceSlide;
        public AudioClip sfxTeleport;
        public AudioClip sfxBomb;

        [Header("Music Tracks – assign in Inspector (one per chapter)")]
        // TODO: Assign chapter background music clips in Inspector
        public AudioClip[] chapterMusic = new AudioClip[10];

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (!SaveSystem.Data.soundEnabled || clip == null) return;
            _sfxSource.PlayOneShot(clip, volume);
        }

        public void PlaySlide()   => PlaySFX(sfxSlide);
        public void PlayThud()    => PlaySFX(sfxThud);
        public void PlayWin()     => PlaySFX(sfxWin);
        public void PlayUnlock()  => PlaySFX(sfxUnlock);
        public void PlayCoin()    => PlaySFX(sfxCoin, 0.7f);
        public void PlayStar()    => PlaySFX(sfxStarChime);
        public void PlayButton()  => PlaySFX(sfxButtonTap, 0.8f);
        public void PlayLock()    => PlaySFX(sfxLock);
        public void PlayIce()     => PlaySFX(sfxIceSlide);
        public void PlayTeleport()=> PlaySFX(sfxTeleport);
        public void PlayBomb()    => PlaySFX(sfxBomb);

        public void PlayChapterMusic(int chapterId)
        {
            if (!SaveSystem.Data.musicEnabled) return;
            int idx = Mathf.Clamp(chapterId - 1, 0, chapterMusic.Length - 1);
            if (chapterMusic[idx] == null) return;
            if (_musicSource.clip == chapterMusic[idx] && _musicSource.isPlaying) return;
            _musicSource.clip = chapterMusic[idx];
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public void StopMusic() => _musicSource.Stop();

        public void SetMusicEnabled(bool enabled)
        {
            if (enabled) _musicSource.Play();
            else _musicSource.Pause();
        }

        public void SetSFXEnabled(bool enabled) { /* SFX just skips if disabled */ }
    }
}
