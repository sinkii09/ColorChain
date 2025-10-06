using ColorChain.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ColorChain.UI
{
    public class Menu : BaseUIPanel
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private AnimatedButton startGameButton;

        [Header("Animation Settings")]
        [SerializeField] private RectTransform containerRect;
        [SerializeField] private float transitionDuration = 0.5f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;
        [SerializeField] private AnimationType animationType = AnimationType.Scale;

        public enum AnimationType
        {
            Scale,
            Fade,
            SlideUp,
            SlideDown,
            ScaleAndFade
        }

        private Sequence animationSequence;
        private CanvasGroup canvasGroup;

        public void ShowMainMenuUI()
        {
            if (_titleText != null)
            {
                _titleText.gameObject.SetActive(true);
            }

            SetButtonActive(startGameButton, true);
        }

        public void HideMainMenuUI()
        {
            if (_titleText != null)
            {
                _titleText.gameObject.SetActive(false);
            }

            SetButtonActive(startGameButton, false);
        }
        private void SetButtonActive(AnimatedButton button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        #region Button Handlers

        private void OnStartGameClicked()
        {
            GameStateManager.StartGame();
        }

        protected override void OnInitialize()
        {
            // Get or add CanvasGroup for fade animations
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && (animationType == AnimationType.Fade || animationType == AnimationType.ScaleAndFade))
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Use this GameObject's RectTransform if containerRect not assigned
            if (containerRect == null)
            {
                containerRect = GetComponent<RectTransform>();
            }
        }

        protected override void SubscribeToEvents()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);
        }

        protected override void UnsubscribeFromEvents()
        {
            if (startGameButton != null)
                startGameButton.onClick.RemoveListener(OnStartGameClicked);
        }

        protected override void OnShow()
        {
            base.OnShow();
            PlayShowAnimation();
        }

        protected override void OnHide()
        {
            PlayHideAnimation();
        }

        protected override void OnCleanup()
        {
            animationSequence?.Kill();
            base.OnCleanup();
        }

        private void PlayShowAnimation()
        {
            if (containerRect == null) return;

            animationSequence?.Kill();
            animationSequence = DOTween.Sequence();

            switch (animationType)
            {
                case AnimationType.Scale:
                    containerRect.localScale = Vector3.zero;
                    animationSequence.Append(containerRect.DOScale(Vector3.one, transitionDuration).SetEase(showEase));
                    break;

                case AnimationType.Fade:
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = 0f;
                        animationSequence.Append(canvasGroup.DOFade(1f, transitionDuration).SetEase(showEase));
                    }
                    break;

                case AnimationType.SlideUp:
                    Vector2 startPosUp = containerRect.anchoredPosition;
                    containerRect.anchoredPosition = startPosUp - new Vector2(0, 500f);
                    animationSequence.Append(containerRect.DOAnchorPos(startPosUp, transitionDuration).SetEase(showEase));
                    break;

                case AnimationType.SlideDown:
                    Vector2 startPosDown = containerRect.anchoredPosition;
                    containerRect.anchoredPosition = startPosDown + new Vector2(0, 500f);
                    animationSequence.Append(containerRect.DOAnchorPos(startPosDown, transitionDuration).SetEase(showEase));
                    break;

                case AnimationType.ScaleAndFade:
                    containerRect.localScale = Vector3.zero;
                    if (canvasGroup != null) canvasGroup.alpha = 0f;

                    animationSequence.Append(containerRect.DOScale(Vector3.one, transitionDuration).SetEase(showEase));
                    if (canvasGroup != null)
                    {
                        animationSequence.Join(canvasGroup.DOFade(1f, transitionDuration).SetEase(showEase));
                    }
                    break;
            }
        }

        private void PlayHideAnimation()
        {
            if (containerRect == null) return;

            animationSequence?.Kill();
            animationSequence = DOTween.Sequence();

            switch (animationType)
            {
                case AnimationType.Scale:
                    animationSequence.Append(containerRect.DOScale(Vector3.zero, transitionDuration).SetEase(hideEase))
                        .OnComplete(() => base.OnHide());
                    break;

                case AnimationType.Fade:
                    if (canvasGroup != null)
                    {
                        animationSequence.Append(canvasGroup.DOFade(0f, transitionDuration).SetEase(hideEase))
                            .OnComplete(() => base.OnHide());
                    }
                    else
                    {
                        base.OnHide();
                    }
                    break;

                case AnimationType.SlideUp:
                    Vector2 targetPosUp = containerRect.anchoredPosition + new Vector2(0, 500f);
                    animationSequence.Append(containerRect.DOAnchorPos(targetPosUp, transitionDuration).SetEase(hideEase))
                        .OnComplete(() => base.OnHide());
                    break;

                case AnimationType.SlideDown:
                    Vector2 targetPosDown = containerRect.anchoredPosition - new Vector2(0, 500f);
                    animationSequence.Append(containerRect.DOAnchorPos(targetPosDown, transitionDuration).SetEase(hideEase))
                        .OnComplete(() => base.OnHide());
                    break;

                case AnimationType.ScaleAndFade:
                    animationSequence.Append(containerRect.DOScale(Vector3.zero, transitionDuration).SetEase(hideEase));
                    if (canvasGroup != null)
                    {
                        animationSequence.Join(canvasGroup.DOFade(0f, transitionDuration).SetEase(hideEase));
                    }
                    animationSequence.OnComplete(() => base.OnHide());
                    break;
            }
        }

        public override float GetTransitionDuration()
        {
            return transitionDuration;
        }

        #endregion
    }
}