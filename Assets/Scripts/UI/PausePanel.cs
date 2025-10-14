using ColorChain.Core;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace ColorChain.UI
{
    public class PausePanel : BaseUIPanel
    {
        [Header("Buttons")]
        [SerializeField] private AnimatedButton _quitButton;
        [SerializeField] private AnimatedButton _restartButton;

        [Header("Audio Volume")]
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI _sfxVolumeText;
        [SerializeField] private TextMeshProUGUI _musicVolumeText;

        [Header("Container")]
        [SerializeField] private RectTransform _containerRect;

        [Header("Animation Settings")]
        [SerializeField] private float _transitionDuration = 0.3f;
        [SerializeField] private Ease _showEase = Ease.OutBack;
        [SerializeField] private Ease _hideEase = Ease.InBack;

        private Sequence _containerSequence;


        private void OnRestartClicked()
        {
            if (isActive)
                GameStateManager.StartGame();
        }
        private void OnQuitClicked()
        {
            if (isActive)
                GameStateManager.ToMainMenu();
        }

        private void OnSFXVolumeChanged(float value)
        {
            AudioManager.SetSFXVolume(value);
            UpdateSFXVolumeText(value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            AudioManager.SetMusicVolume(value);
            UpdateMusicVolumeText(value);
        }

        private void UpdateSFXVolumeText(float volume)
        {
            if (_sfxVolumeText != null)
            {
                int percentage = Mathf.RoundToInt(volume * 100);
                _sfxVolumeText.text = $"{percentage}%";
            }
        }

        private void UpdateMusicVolumeText(float volume)
        {
            if (_musicVolumeText != null)
            {
                int percentage = Mathf.RoundToInt(volume * 100);
                _musicVolumeText.text = $"{percentage}%";
            }
        }

        protected override void OnInitialize()
        {
            _containerRect.gameObject.SetActive(false);
            InitializeVolumeSliders();
        }

        private void InitializeVolumeSliders()
        {
            // Initialize SFX slider
            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.minValue = 0f;
                _sfxVolumeSlider.maxValue = 1f;
                _sfxVolumeSlider.value = AudioManager.SFXVolume;
                UpdateSFXVolumeText(AudioManager.SFXVolume);
            }

            // Initialize Music slider
            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.minValue = 0f;
                _musicVolumeSlider.maxValue = 1f;
                _musicVolumeSlider.value = AudioManager.MusicVolume;
                UpdateMusicVolumeText(AudioManager.MusicVolume);
            }
        }

        protected override void SubscribeToEvents()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);
            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuitClicked);

            // Subscribe to slider events
            if (_sfxVolumeSlider != null)
                _sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            if (_musicVolumeSlider != null)
                _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        protected override void UnsubscribeFromEvents()
        {
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuitClicked);

            // Unsubscribe from slider events
            if (_sfxVolumeSlider != null)
                _sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            if (_musicVolumeSlider != null)
                _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }

        protected override void OnCleanup()
        {
            _containerSequence?.Kill();
            base.OnCleanup();
        }

        protected override void OnShow()
        {
            if (_containerRect == null) return;

            _containerRect.gameObject.SetActive(true);

            _containerSequence?.Kill();

            // Start from scale 0
            _containerRect.localScale = Vector3.zero;

            _containerSequence = DOTween.Sequence();
            _containerSequence.Append(_containerRect.DOScale(Vector3.one, _transitionDuration)
                .SetEase(_showEase));
        }

        protected override void OnHide()
        {
            if (_containerRect == null) return;

            _containerSequence?.Kill();

            _containerSequence = DOTween.Sequence();
            _containerSequence.Append(_containerRect.DOScale(Vector3.zero, _transitionDuration)
                .SetEase(_hideEase)).OnComplete(() => _containerRect.gameObject.SetActive(false));

        }

        public override float GetTransitionDuration()
        {
            return _transitionDuration;
        }
    }
}