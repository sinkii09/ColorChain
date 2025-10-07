using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ColorChain.UI
{
    /// <summary>
    /// Animated power-up progress bar with fill animation and effects
    /// </summary>
    public class PowerUpBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image glowImage;
        [SerializeField] private RectTransform containerRect;

        [Header("Fill Settings")]
        [SerializeField] private Gradient fillGradient;
        [SerializeField] private bool useGradient = true;
        [SerializeField] private Color fillColorStart = new Color(0.2f, 0.8f, 1f);
        [SerializeField] private Color fillColorFull = new Color(1f, 0.8f, 0.2f);

        [Header("Animation Settings")]
        [SerializeField] private float fillDuration = 0.3f;
        [SerializeField] private Ease fillEase = Ease.OutQuad;
        [SerializeField] private bool animateOnFill = true;

        [Header("Full Animation")]
        [SerializeField] private bool pulseWhenFull = true;
        [SerializeField] private float pulseScale = 1.1f;
        [SerializeField] private float pulseDuration = 0.5f;

        [Header("Glow Effect")]
        [SerializeField] private bool enableGlow = true;
        [SerializeField] private float glowIntensity = 2f;

        private float currentProgress = 0f;
        private Tween fillTween;
        private Tween pulseTween;
        private Tween glowTween;
        private Vector3 originalScale;

        private void Awake()
        {
            if (containerRect == null)
                containerRect = transform as RectTransform;

            originalScale = containerRect.localScale;

            // Initialize gradient if not set
            if (fillGradient == null || fillGradient.colorKeys.Length == 0)
            {
                fillGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[2];
                colorKeys[0] = new GradientColorKey(fillColorStart, 0f);
                colorKeys[1] = new GradientColorKey(fillColorFull, 1f);

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                fillGradient.SetKeys(colorKeys, alphaKeys);
            }

            // Initialize fill amount
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
                UpdateFillColor(0f);
            }

            // Hide glow initially
            if (glowImage != null && enableGlow)
            {
                glowImage.enabled = false;
            }
        }

        /// <summary>
        /// Set progress value between 0 and 1
        /// </summary>
        public void SetProgress(float progress, bool animate = true)
        {
            progress = Mathf.Clamp01(progress);

            if (animate && animateOnFill)
            {
                AnimateFill(currentProgress, progress);
            }
            else
            {
                currentProgress = progress;
                if (fillImage != null)
                {
                    fillImage.fillAmount = progress;
                    UpdateFillColor(progress);
                }
            }

            // Check if full
            if (progress >= 1f)
            {
                OnBarFull();
            }
            else
            {
                StopFullAnimation();
            }
        }

        /// <summary>
        /// Add progress increment
        /// </summary>
        public void AddProgress(float amount, bool animate = true)
        {
            SetProgress(currentProgress + amount, animate);
        }

        /// <summary>
        /// Reset bar to empty
        /// </summary>
        public void Reset(bool animate = false)
        {
            SetProgress(0f, animate);
        }

        private void AnimateFill(float from, float to)
        {
            fillTween?.Kill();

            fillTween = DOTween.To(
                () => from,
                x => {
                    currentProgress = x;
                    if (fillImage != null)
                    {
                        fillImage.fillAmount = x;
                        UpdateFillColor(x);
                    }
                },
                to,
                fillDuration
            ).SetEase(fillEase);
        }

        private void UpdateFillColor(float progress)
        {
            if (fillImage == null) return;

            if (useGradient && fillGradient != null)
            {
                fillImage.color = fillGradient.Evaluate(progress);
            }
            else
            {
                fillImage.color = Color.Lerp(fillColorStart, fillColorFull, progress);
            }
        }

        private void OnBarFull()
        {
            if (pulseWhenFull)
            {
                PlayPulseAnimation();
            }

            if (enableGlow && glowImage != null)
            {
                PlayGlowAnimation();
            }
        }

        private void PlayPulseAnimation()
        {
            pulseTween?.Kill();

            pulseTween = containerRect.DOScale(originalScale * pulseScale, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void PlayGlowAnimation()
        {
            if (glowImage == null) return;

            glowImage.enabled = true;
            glowTween?.Kill();

            Color glowColor = fillImage != null ? fillImage.color : Color.white;
            glowColor.a = 0f;
            glowImage.color = glowColor;

            glowColor.a = glowIntensity;
            glowTween = glowImage.DOColor(glowColor, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopFullAnimation()
        {
            pulseTween?.Kill();
            glowTween?.Kill();

            containerRect.localScale = originalScale;

            if (glowImage != null)
            {
                glowImage.enabled = false;
            }
        }

        private void OnDestroy()
        {
            fillTween?.Kill();
            pulseTween?.Kill();
            glowTween?.Kill();
        }

        private void OnDisable()
        {
            StopFullAnimation();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying && fillImage != null)
            {
                fillImage.fillAmount = currentProgress;
                UpdateFillColor(currentProgress);
            }
        }
#endif
    }
}
