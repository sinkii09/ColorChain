using ColorChain.Core;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace ColorChain.UI
{
    public class GameOverPanel : BaseUIPanel
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private AnimatedButton _restartButton;
        [SerializeField] private AnimatedButton _exitButton;
        [SerializeField] private RectTransform _containerRect;

        [Header("Animation Settings")]
        [SerializeField] private float _transitionDuration = 0.3f;
        [SerializeField] private Ease _showEase = Ease.OutBack;
        [SerializeField] private Ease _hideEase = Ease.InBack;
        [SerializeField] private float _scoreCountDuration = 1f;
        [SerializeField] private float _slideDistance = 1000f; // Distance to slide from top

        private Sequence _containerSequence;
        private Tween _scoreCountTween;
        private Vector2 _originalPosition;

        protected override void OnInitialize()
        {
            if (_containerRect != null)
            {
                _originalPosition = _containerRect.anchoredPosition;
                _containerRect.gameObject.SetActive(false);
            }
        }

        protected override void SubscribeToEvents()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);
            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExitClicked);
        }

        protected override void UnsubscribeFromEvents()
        {
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_exitButton != null)
                _exitButton.onClick.RemoveListener(OnExitClicked);
        }

        protected override void OnCleanup()
        {
            _containerSequence?.Kill();
            _scoreCountTween?.Kill();
            base.OnCleanup();
        }

        protected override void OnShow()
        {
            if (_containerRect == null) return;

            _containerRect.gameObject.SetActive(true);

            // Update score display
            UpdateScoreDisplay();

            _containerSequence?.Kill();

            // Start from top (above screen)
            _containerRect.anchoredPosition = _originalPosition + new Vector2(0, _slideDistance);

            _containerSequence = DOTween.Sequence();
            _containerSequence.Append(_containerRect.DOAnchorPos(_originalPosition, _transitionDuration)
                .SetEase(_showEase));
        }

        protected override void OnHide()
        {
            if (_containerRect == null) return;

            _containerSequence?.Kill();

            // Slide back up to top
            Vector2 topPosition = _originalPosition + new Vector2(0, _slideDistance);

            _containerSequence = DOTween.Sequence();
            _containerSequence.Append(_containerRect.DOAnchorPos(topPosition, _transitionDuration)
                .SetEase(_hideEase))
                .OnComplete(() => _containerRect.gameObject.SetActive(false));
        }

        public override float GetTransitionDuration()
        {
            return _transitionDuration;
        }

        private void UpdateScoreDisplay()
        {
            // Animate score counting up
            if (_scoreText != null)
            {
                _scoreCountTween?.Kill();
                _scoreCountTween = DOTween.To(
                    () => 0,
                    x => _scoreText.text = x.ToString(),
                    ScoreManager.CurrentScore,
                    _scoreCountDuration
                ).SetEase(Ease.OutQuad);
            }
        }

        private void OnRestartClicked()
        {
            if (isActive)
                GameStateManager.StartGame();
        }

        private void OnExitClicked()
        {
            if (isActive)
                GameStateManager.ToMainMenu();
        }
    }
}
