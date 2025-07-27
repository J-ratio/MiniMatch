using UnityEngine;
using System.Collections.Generic;
using MiniMatch.Core.Events;
using MiniMatch.Core.Interfaces;

namespace MiniMatch.Infrastructure.Audio
{
    /// <summary>
    /// Audio service that handles all game sound effects and music
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioSource musicAudioSource;
        
        [Header("Sound Effects")]
        [SerializeField] private AudioClip cardFlipSound;
        [SerializeField] private AudioClip cardMatchSound;
        // [SerializeField] private AudioClip cardMismatchSound;
        [SerializeField] private AudioClip levelCompleteSound;
        [SerializeField] private AudioClip gameCompleteSound;
        [SerializeField] private AudioClip buttonClickSound;
        
        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip menuMusic;
        
        [Header("Settings")]
        [SerializeField] private float sfxVolume = 1.0f;
        [SerializeField] private float musicVolume = 0.7f;
        
        private GameEventBus _eventBus;
        private Dictionary<string, AudioClip> _soundLibrary;
        
        private void Awake()
        {
            _eventBus = GameEventBus.Instance;
            InitializeAudioSources();
            BuildSoundLibrary();
        }
        
        private void Start()
        {
            SubscribeToEvents();
            PlayBackgroundMusic();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void InitializeAudioSources()
        {
            // Create audio sources if not assigned
            if (sfxAudioSource == null)
            {
                var sfxGO = new GameObject("SFX AudioSource");
                sfxGO.transform.SetParent(transform);
                sfxAudioSource = sfxGO.AddComponent<AudioSource>();
                sfxAudioSource.playOnAwake = false;
            }
            
            if (musicAudioSource == null)
            {
                var musicGO = new GameObject("Music AudioSource");
                musicGO.transform.SetParent(transform);
                musicAudioSource = musicGO.AddComponent<AudioSource>();
                musicAudioSource.loop = true;
                musicAudioSource.playOnAwake = false;
            }
            
            // Set volumes
            sfxAudioSource.volume = sfxVolume;
            musicAudioSource.volume = musicVolume;
        }
        
        private void BuildSoundLibrary()
        {
            _soundLibrary = new Dictionary<string, AudioClip>
            {
                { "card_flip", cardFlipSound },
                { "card_match", cardMatchSound },
                // { "card_mismatch", cardMismatchSound },
                { "level_complete", levelCompleteSound },
                { "game_complete", gameCompleteSound },
                { "button_click", buttonClickSound }
            };
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<CardFlippedEvent>(OnCardFlipped);
            _eventBus.Subscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus.Subscribe<CardsMismatchedEvent>(OnCardsMismatched);
            _eventBus.Subscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus.Subscribe<GamePhaseChangedEvent>(OnGamePhaseChanged);
            _eventBus.Subscribe<PlaySoundEvent>(OnPlaySoundRequested);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<CardFlippedEvent>(OnCardFlipped);
            _eventBus.Unsubscribe<CardsMatchedEvent>(OnCardsMatched);
            _eventBus.Unsubscribe<CardsMismatchedEvent>(OnCardsMismatched);
            _eventBus.Unsubscribe<LevelCompletedEvent>(OnLevelCompleted);
            _eventBus.Unsubscribe<GamePhaseChangedEvent>(OnGamePhaseChanged);
            _eventBus.Unsubscribe<PlaySoundEvent>(OnPlaySoundRequested);
        }
        
        // Event Handlers
        private void OnCardFlipped(CardFlippedEvent flipEvent)
        {
            PlaySound("card_flip");
        }
        
        private void OnCardsMatched(CardsMatchedEvent matchEvent)
        {
            PlaySound("card_match");
        }
        
        private void OnCardsMismatched(CardsMismatchedEvent mismatchEvent)
        {
            PlaySound("card_mismatch");
        }
        
        private void OnLevelCompleted(LevelCompletedEvent completedEvent)
        {
            PlaySound("level_complete");
        }
        
        private void OnGamePhaseChanged(GamePhaseChangedEvent phaseEvent)
        {
            switch (phaseEvent.NewPhase)
            {
                case GamePhase.GameComplete:
                    PlaySound("game_complete");
                    break;
                case GamePhase.Playing:
                    // Could change music or add ambient sounds
                    break;
                case GamePhase.Paused:
                    // Could pause/lower music
                    break;
            }
        }
        
        private void OnPlaySoundRequested(PlaySoundEvent soundEvent)
        {
            PlaySound(soundEvent.SoundId, soundEvent.Volume);
        }
        
        // Public Methods
        public void PlaySound(string soundId, float volume = 1.0f)
        {
            if (sfxAudioSource == null) return;
            
            if (_soundLibrary.TryGetValue(soundId, out AudioClip clip) && clip != null)
            {
                sfxAudioSource.PlayOneShot(clip, volume * sfxVolume);
            }
            else
            {
                Debug.LogWarning($"Sound '{soundId}' not found in audio library");
            }
        }
        
        public void PlayBackgroundMusic()
        {
            if (musicAudioSource != null && backgroundMusic != null)
            {
                musicAudioSource.clip = backgroundMusic;
                musicAudioSource.Play();
            }
        }
        
        public void PlayMenuMusic()
        {
            if (musicAudioSource != null && menuMusic != null)
            {
                musicAudioSource.clip = menuMusic;
                musicAudioSource.Play();
            }
        }
        
        public void StopMusic()
        {
            if (musicAudioSource != null)
            {
                musicAudioSource.Stop();
            }
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (sfxAudioSource != null)
                sfxAudioSource.volume = sfxVolume;
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicAudioSource != null)
                musicAudioSource.volume = musicVolume;
        }
        
        public void MuteAll(bool mute)
        {
            if (sfxAudioSource != null)
                sfxAudioSource.mute = mute;
                
            if (musicAudioSource != null)
                musicAudioSource.mute = mute;
        }
        
        // Helper method to add sounds at runtime
        public void RegisterSound(string soundId, AudioClip clip)
        {
            if (_soundLibrary == null)
                _soundLibrary = new Dictionary<string, AudioClip>();
                
            _soundLibrary[soundId] = clip;
        }
    }
}