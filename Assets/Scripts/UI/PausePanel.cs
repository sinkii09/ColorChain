using ColorChain.Core;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ColorChain.UI
{
    public class PausePanel : BaseUIPanel
    {
        [SerializeField] private AnimatedButton _quitButton;
        [SerializeField] private AnimatedButton _restartButton;

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

        protected override void OnInitialize()
        {
            _containerRect.gameObject.SetActive(false);
        }

        protected override void SubscribeToEvents()
        {
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);
            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuitClicked);
        }

        protected override void UnsubscribeFromEvents()
        {
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuitClicked);
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