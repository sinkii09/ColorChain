using UnityEngine;

namespace ColorChain.Core
{
    public static class AudioManager
    {
        private static AudioSource[] _sfxPool;
        private static AudioSource _musicSource;
        private static AudioConfig _config;
        private static Transform _poolParent;

        private static float _sfxVolume = 1f;
        private static float _musicVolume = 1f;

        private static bool _isInitialized = false;

        public static float SFXVolume => _sfxVolume;
        public static float MusicVolume => _musicVolume;

        public static void Initialize(AudioConfig config, Transform poolParent)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("AudioManager already initialized");
                return;
            }

            _config = config;
            _poolParent = poolParent;

            CreateAudioPool();
            LoadVolumeSettings();

            _isInitialized = true;
        }

        public static void Terminate()
        {
            if (_poolParent != null)
            {
                Object.Destroy(_poolParent.gameObject);
            }

            _sfxPool = null;
            _musicSource = null;
            _config = null;
            _poolParent = null;
            _isInitialized = false;
        }

        private static void CreateAudioPool()
        {
            int poolSize = _config.audioSourcePoolSize;
            _sfxPool = new AudioSource[poolSize];

            for (int i = 0; i < poolSize; i++)
            {
                GameObject sfxObject = new GameObject($"SFX_AudioSource_{i}");
                sfxObject.transform.SetParent(_poolParent);
                _sfxPool[i] = sfxObject.AddComponent<AudioSource>();
                _sfxPool[i].playOnAwake = false;
            }

            // Create music source
            GameObject musicObject = new GameObject("Music_AudioSource");
            musicObject.transform.SetParent(_poolParent);
            _musicSource = musicObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
        }

        private static void LoadVolumeSettings()
        {
            _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", _config.defaultSFXVolume);
            _musicVolume = PlayerPrefs.GetFloat("MusicVolume", _config.defaultMusicVolume);

            UpdateVolumes();
        }

        public static void PlaySFX(AudioClip clip, float pitch = 1f)
        {
            if (!_isInitialized || clip == null) return;

            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.pitch = pitch;
                source.PlayOneShot(clip, _sfxVolume);
            }
        }

        public static void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (!_isInitialized || clip == null) return;

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public static void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }

        public static void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
            PlayerPrefs.Save();
        }

        public static void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
            PlayerPrefs.Save();

            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume;
            }
        }

        private static void UpdateVolumes()
        {
            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume;
            }

            // SFX volume is applied per-clip in PlayOneShot
        }

        private static AudioSource GetAvailableSFXSource()
        {
            // Find first non-playing source
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // If all busy, return first one (will overlap)
            return _sfxPool[0];
        }

        // Helper methods to play common sounds
        public static void PlayTileClick()
        {
            if (_config != null && _config.tileClickSound != null)
            {
                PlaySFX(_config.tileClickSound);
            }
        }

        public static void PlayChainReaction(float pitch = 1f)
        {
            if (_config != null && _config.chainReactionSound != null)
            {
                PlaySFX(_config.chainReactionSound, pitch);
            }
        }

        public static void PlayPowerUp()
        {
            if (_config != null && _config.powerUpSound != null)
            {
                PlaySFX(_config.powerUpSound);
            }
        }

        public static void PlayPanelShow()
        {
            if (_config != null && _config.panelShowSound != null)
            {
                PlaySFX(_config.panelShowSound);
            }
        }

        public static void PlayPanelHide()
        {
            if (_config != null && _config.panelHideSound != null)
            {
                PlaySFX(_config.panelHideSound);
            }
        }

        public static void PlayButtonClick()
        {
            if (_config != null && _config.buttonClickSound != null)
            {
                PlaySFX(_config.buttonClickSound);
            }
        }

        public static void PlayButtonHover()
        {
            if (_config != null && _config.buttonHoverSound != null)
            {
                PlaySFX(_config.buttonHoverSound);
            }
        }

        public static void PlayMenuMusic()
        {
            if (_config != null && _config.menuMusic != null)
            {
                PlayMusic(_config.menuMusic);
            }
        }
        public static void PlayBackgroundMusic()
        {
            if (_config != null && _config.backgroundMusic != null)
            {
                PlayMusic(_config.backgroundMusic);
            }
        }
    }
}
