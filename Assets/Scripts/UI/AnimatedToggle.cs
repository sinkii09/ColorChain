using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace ColorChain.UI
{
    /// <summary>
    /// Animated toggle button that swaps sprites with smooth animations
    /// Perfect for pause/resume, sound on/off, etc.
    /// </summary>
    public class AnimatedToggle : Selectable, IPointerClickHandler
    {
        [Header("Toggle State")]
        [SerializeField] private bool isOn = false;

        [Header("Sprites")]
        [SerializeField] private Sprite spriteOn;
        [SerializeField] private Sprite spriteOff;
        [SerializeField] private Image targetImage;

        [Header("Animation")]
        [SerializeField] private ToggleAnimation animationType = ToggleAnimation.ScaleRotate;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutBack;
        [SerializeField, Range(0f, 1f), Tooltip("Point in animation where sprite swaps (0-1)")]
        private float spriteSwapPoint = 0.5f;

        [Header("Scale Animation")]
        [SerializeField] private float scaleAmount = 1.2f;

        [Header("Rotation Animation")]
        [SerializeField] private float rotationAngle = 180f;

        [Header("Fade Animation")]
        [SerializeField] private float fadeOutAlpha = 0f;

        [Header("Events")]
        public UnityEvent<bool> onValueChanged;

        public enum ToggleAnimation
        {
            None,
            Scale,
            Rotate,
            ScaleRotate,
            Fade,
            ScaleFade,
            Flip
        }

        private Tween animationTween;
        private bool isAnimating = false;

        public bool IsOn
        {
            get => isOn;
            set
            {
                if (isOn != value)
                {
                    isOn = value;
                    UpdateVisuals(true);
                    onValueChanged?.Invoke(isOn);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            // Auto-get image if not assigned
            if (targetImage == null)
                targetImage = GetComponent<Image>();

            if (targetImage == null)
                targetImage = GetComponentInChildren<Image>();

            // Disable Unity's built-in transitions
            transition = Transition.None;

            // Set initial sprite
            UpdateVisuals(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable() || isAnimating)
                return;

            Toggle();
        }

        public void Toggle()
        {
            IsOn = !IsOn;
        }

        public void SetValueWithoutNotify(bool value)
        {
            isOn = value;
            UpdateVisuals(false);
        }

        private void UpdateVisuals(bool animate)
        {
            if (targetImage == null) return;

            animationTween?.Kill();

            Sprite targetSprite = isOn ? spriteOn : spriteOff;

            if (!animate || animationType == ToggleAnimation.None)
            {
                targetImage.sprite = targetSprite;
                return;
            }

            isAnimating = true;

            switch (animationType)
            {
                case ToggleAnimation.Scale:
                    AnimateScale(targetSprite);
                    break;

                case ToggleAnimation.Rotate:
                    AnimateRotate(targetSprite);
                    break;

                case ToggleAnimation.ScaleRotate:
                    AnimateScaleRotate(targetSprite);
                    break;

                case ToggleAnimation.Fade:
                    AnimateFade(targetSprite);
                    break;

                case ToggleAnimation.ScaleFade:
                    AnimateScaleFade(targetSprite);
                    break;

                case ToggleAnimation.Flip:
                    AnimateFlip(targetSprite);
                    break;
            }
        }

        private void AnimateScale(Sprite targetSprite)
        {
            Vector3 originalScale = transform.localScale;
            float firstHalf = animationDuration * spriteSwapPoint;
            float secondHalf = animationDuration * (1f - spriteSwapPoint);

            animationTween = DOTween.Sequence()
                .Append(transform.DOScale(originalScale * scaleAmount, firstHalf).SetEase(animationEase))
                .AppendCallback(() => targetImage.sprite = targetSprite)
                .Append(transform.DOScale(originalScale, secondHalf).SetEase(animationEase))
                .OnComplete(() => isAnimating = false);
        }

        private void AnimateRotate(Sprite targetSprite)
        {
            Vector3 currentRotation = transform.localEulerAngles;
            float firstHalf = animationDuration * spriteSwapPoint;
            float secondHalf = animationDuration * (1f - spriteSwapPoint);

            animationTween = DOTween.Sequence()
                .Append(transform.DOLocalRotate(currentRotation + new Vector3(0, 0, rotationAngle * spriteSwapPoint), firstHalf).SetEase(animationEase))
                .AppendCallback(() => targetImage.sprite = targetSprite)
                .Append(transform.DOLocalRotate(currentRotation + new Vector3(0, 0, rotationAngle), secondHalf).SetEase(animationEase))
                .OnComplete(() => isAnimating = false);
        }

        private void AnimateScaleRotate(Sprite targetSprite)
        {
            Vector3 originalScale = transform.localScale;
            Vector3 currentRotation = transform.localEulerAngles;
            float firstHalf = animationDuration * spriteSwapPoint;
            float secondHalf = animationDuration * (1f - spriteSwapPoint);

            animationTween = DOTween.Sequence()
                .Append(transform.DOScale(originalScale * scaleAmount, firstHalf).SetEase(animationEase))
                .Join(transform.DOLocalRotate(currentRotation + new Vector3(0, 0, rotationAngle * spriteSwapPoint), firstHalf).SetEase(animationEase))
                .AppendCallback(() => targetImage.sprite = targetSprite)
                .Append(transform.DOScale(originalScale, secondHalf).SetEase(animationEase))
                .Join(transform.DOLocalRotate(currentRotation + new Vector3(0, 0, rotationAngle), secondHalf).SetEase(animationEase))
                .OnComplete(() => isAnimating = false);
        }

        private void AnimateFade(Sprite targetSprite)
        {
            Color originalColor = targetImage.color;
            float firstHalf = animationDuration * spriteSwapPoint;
            float secondHalf = animationDuration * (1f - spriteSwapPoint);

            animationTween = DOTween.Sequence()
                .Append(targetImage.DOFade(fadeOutAlpha, firstHalf))
                .AppendCallback(() => targetImage.sprite = targetSprite)
                .Append(targetImage.DOFade(originalColor.a, secondHalf))
                .OnComplete(() => isAnimating = false);
        }

        private void AnimateScaleFade(Sprite targetSprite)
        {
            Vector3 originalScale = transform.localScale;
            Color originalColor = targetImage.color;
            float firstHalf = animationDuration * spriteSwapPoint;
            float secondHalf = animationDuration * (1f - spriteSwapPoint);

            animationTween = DOTween.Sequence()
                .Append(transform.DOScale(originalScale * scaleAmount, firstHalf).SetEase(animationEase))
                .Join(targetImage.DOFade(fadeOutAlpha, firstHalf))
                .AppendCallback(() => targetImage.sprite = targetSprite)
                .Append(transform.DOScale(originalScale, secondHalf).SetEase(animationEase))
                .Join(targetImage.DOFade(originalColor.a, secondHalf))
                .OnComplete(() => isAnimating = false);
        }

        private void AnimateFlip(Sprite targetSprite)
        {
            Vector3 originalScale = transform.localScale;
            float firstHalf = animationDuration * spriteSwapPoint;
            float secondHalf = animationDuration * (1f - spriteSwapPoint);

            animationTween = DOTween.Sequence()
                .Append(transform.DOScaleX(0f, firstHalf).SetEase(Ease.InQuad))
                .AppendCallback(() => targetImage.sprite = targetSprite)
                .Append(transform.DOScaleX(originalScale.x, secondHalf).SetEase(Ease.OutQuad))
                .OnComplete(() => isAnimating = false);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            animationTween?.Kill();
            isAnimating = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            animationTween?.Kill();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            transition = Transition.None;

            if (targetImage != null && Application.isPlaying)
            {
                targetImage.sprite = isOn ? spriteOn : spriteOff;
            }
        }
#endif
    }
}
