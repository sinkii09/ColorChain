using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using ColorChain.Core;

namespace ColorChain.UI
{
    /// <summary>
    /// Animated button component that extends Unity Selectable with DOTween animations
    /// Properly handles all UI states including keyboard/controller navigation
    /// </summary>
    public class AnimatedButton : Selectable, IPointerClickHandler
    {
        [Header("Animation Type")]
        [SerializeField] private AnimationType animationType = AnimationType.Scale;

        [Header("Scale Animation")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float pressScale = 0.9f;
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        [Header("Punch Animation")]
        [SerializeField] private float punchScale = 0.2f;
        [SerializeField] private float punchDuration = 0.3f;
        [SerializeField] private int punchVibrato = 10;
        [SerializeField] private float punchElasticity = 1f;

        [Header("Rotation Animation")]
        [SerializeField] private float hoverRotation = 5f;
        [SerializeField] private float pressRotation = -5f;
        [SerializeField] private float rotationDuration = 0.2f;
        [SerializeField] private Ease rotationEase = Ease.OutQuad;

        [Header("Color Animation")]
        [SerializeField] private bool useColorAnimation = true;
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color pressColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private float colorDuration = 0.15f;

        [Header("Floating Effect")]
        [SerializeField] private bool enableFloating = false;
        [SerializeField] private float floatingDistance = 10f;
        [SerializeField] private float floatingDuration = 1.5f;
        [SerializeField] private Ease floatingEase = Ease.InOutSine;
        [SerializeField] private bool floatingUseLocalPosition = true;

        [Header("Sound (Optional)")]
        [SerializeField] private AudioClip customHoverSound;
        [SerializeField] private AudioClip customClickSound;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent onClick;

        public enum AnimationType
        {
            Scale,
            Punch,
            Rotation,
            Color,
            ScaleAndColor,
            All
        }

        private Vector3 originalScale;
        private Quaternion originalRotation;
        private Color originalColor;
        private Vector3 originalPosition;

        private Tween scaleTween;
        private Tween rotationTween;
        private Tween colorTween;
        private Tween floatingTween;

        protected override void Awake()
        {
            base.Awake();

            originalScale = transform.localScale;
            originalRotation = transform.localRotation;

            if (floatingUseLocalPosition)
                originalPosition = transform.localPosition;
            else
                originalPosition = transform.position;

            // Use inherited targetGraphic or find one
            if (targetGraphic == null)
            {
                targetGraphic = GetComponent<Graphic>();
            }
            if (targetGraphic == null)
            {
                targetGraphic = GetComponentInChildren<Graphic>();
            }

            if (targetGraphic != null)
                originalColor = targetGraphic.color;

            // Disable Unity's built-in transitions
            transition = Transition.None;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (enableFloating)
            {
                StartFloating();
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (!gameObject.activeInHierarchy)
                return;

            switch (state)
            {
                case SelectionState.Normal:
                    TransitionToNormal(instant);
                    break;

                case SelectionState.Highlighted:
                    TransitionToHighlighted(instant);
                    PlayHoverSound();
                    break;

                case SelectionState.Pressed:
                    TransitionToPressed(instant);
                    break;

                case SelectionState.Selected:
                    TransitionToHighlighted(instant);
                    break;

                case SelectionState.Disabled:
                    TransitionToDisabled(instant);
                    break;
            }
        }

        private void TransitionToNormal(bool instant)
        {
            KillAllTweens();

            if (instant)
            {
                transform.localScale = originalScale;
                transform.localRotation = originalRotation;
                if (targetGraphic != null && useColorAnimation)
                    targetGraphic.color = originalColor;
            }
            else
            {
                scaleTween = transform.DOScale(originalScale, scaleDuration).SetEase(scaleEase);
                rotationTween = transform.DOLocalRotate(originalRotation.eulerAngles, rotationDuration).SetEase(rotationEase);

                if (targetGraphic != null && useColorAnimation)
                {
                    colorTween = targetGraphic.DOColor(originalColor, colorDuration);
                }
            }
        }

        private void TransitionToHighlighted(bool instant)
        {
            KillAllTweens();

            switch (animationType)
            {
                case AnimationType.Scale:
                case AnimationType.ScaleAndColor:
                case AnimationType.All:
                    AnimateScale(originalScale * hoverScale, instant);
                    break;

                case AnimationType.Punch:
                    if (!instant)
                        AnimatePunch();
                    break;

                case AnimationType.Rotation:
                    AnimateRotation(hoverRotation, instant);
                    break;
            }

            if ((animationType == AnimationType.Color ||
                animationType == AnimationType.ScaleAndColor ||
                animationType == AnimationType.All) && useColorAnimation)
            {
                AnimateColor(hoverColor, instant);
            }

            if (animationType == AnimationType.All)
            {
                AnimateRotation(hoverRotation, instant);
            }
        }

        private void TransitionToPressed(bool instant)
        {
            KillAllTweens();

            switch (animationType)
            {
                case AnimationType.Scale:
                case AnimationType.ScaleAndColor:
                case AnimationType.All:
                    AnimateScale(originalScale * pressScale, instant);
                    break;

                case AnimationType.Rotation:
                    AnimateRotation(pressRotation, instant);
                    break;
            }

            if ((animationType == AnimationType.Color ||
                animationType == AnimationType.ScaleAndColor ||
                animationType == AnimationType.All) && useColorAnimation)
            {
                AnimateColor(pressColor, instant);
            }

            if (animationType == AnimationType.All)
            {
                AnimateRotation(pressRotation, instant);
            }
        }

        private void TransitionToDisabled(bool instant)
        {
            KillAllTweens();

            if (instant)
            {
                transform.localScale = originalScale;
                transform.localRotation = originalRotation;
                if (targetGraphic != null && useColorAnimation)
                    targetGraphic.color = disabledColor;
            }
            else
            {
                scaleTween = transform.DOScale(originalScale, scaleDuration).SetEase(scaleEase);
                rotationTween = transform.DOLocalRotate(originalRotation.eulerAngles, rotationDuration).SetEase(rotationEase);

                if (targetGraphic != null && useColorAnimation)
                {
                    colorTween = targetGraphic.DOColor(disabledColor, colorDuration);
                }
            }
        }

        private void AnimateScale(Vector3 targetScale, bool instant)
        {
            if (instant)
            {
                transform.localScale = targetScale;
            }
            else
            {
                scaleTween = transform.DOScale(targetScale, scaleDuration).SetEase(scaleEase);
            }
        }

        private void AnimatePunch()
        {
            scaleTween = transform.DOPunchScale(Vector3.one * punchScale, punchDuration, punchVibrato, punchElasticity);
        }

        private void AnimateRotation(float angle, bool instant)
        {
            Vector3 targetRotation = originalRotation.eulerAngles + new Vector3(0, 0, angle);

            if (instant)
            {
                transform.localRotation = Quaternion.Euler(targetRotation);
            }
            else
            {
                rotationTween = transform.DOLocalRotate(targetRotation, rotationDuration).SetEase(rotationEase);
            }
        }

        private void AnimateColor(Color targetColor, bool instant)
        {
            if (targetGraphic == null) return;

            if (instant)
            {
                targetGraphic.color = targetColor;
            }
            else
            {
                colorTween = targetGraphic.DOColor(targetColor, colorDuration);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable())
                return;

            PlayClickSound();
            onClick?.Invoke();
        }

        private void PlayHoverSound()
        {
            if (customHoverSound != null)
            {
                AudioManager.PlaySFX(customHoverSound);
            }
            else
            {
                AudioManager.PlayButtonHover();
            }
        }

        private void PlayClickSound()
        {
            if (customClickSound != null)
            {
                AudioManager.PlaySFX(customClickSound);
            }
            else
            {
                AudioManager.PlayButtonClick();
            }
        }

        private void StartFloating()
        {
            if (!gameObject.activeInHierarchy) return;

            floatingTween?.Kill();

            Vector3 targetPos = originalPosition + new Vector3(0, floatingDistance, 0);

            if (floatingUseLocalPosition)
            {
                floatingTween = transform.DOLocalMoveY(targetPos.y, floatingDuration)
                    .SetEase(floatingEase)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                floatingTween = transform.DOMoveY(targetPos.y, floatingDuration)
                    .SetEase(floatingEase)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        private void StopFloating()
        {
            floatingTween?.Kill();

            if (floatingUseLocalPosition)
                transform.localPosition = originalPosition;
            else
                transform.position = originalPosition;
        }

        public void SetFloating(bool enabled)
        {
            enableFloating = enabled;

            if (enabled && gameObject.activeInHierarchy)
                StartFloating();
            else
                StopFloating();
        }

        private void KillAllTweens()
        {
            scaleTween?.Kill();
            rotationTween?.Kill();
            colorTween?.Kill();
            floatingTween?.Kill();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            KillAllTweens();
            transform.localScale = originalScale;
            transform.localRotation = originalRotation;

            if (floatingUseLocalPosition)
                transform.localPosition = originalPosition;
            else
                transform.position = originalPosition;

            if (targetGraphic != null)
            {
                targetGraphic.color = originalColor;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            KillAllTweens();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            // Force transition to None in editor
            transition = Transition.None;
        }
#endif
    }
}
