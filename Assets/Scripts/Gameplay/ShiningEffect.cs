using UnityEngine;
using DG.Tweening;

namespace ColorChain.Gameplay
{
    public class ShiningEffect : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float duration = 1f;
        [SerializeField] private float scaleMultiplier = 1.05f;
        [SerializeField] private Ease easeType = Ease.InOutSine;

        [Header("Scale Settings")]
        [SerializeField] private bool useScale = true;

        [Header("Glow Settings")]
        [SerializeField] private bool useGlow = true;
        [SerializeField, Range(0.1f, 1f)] private float minAlpha = 0.5f;
        [SerializeField, Range(0.1f, 1f)] private float maxAlpha = 0.85f;

        [Header("Rotation Settings")]
        [SerializeField] private bool useRotation = true;
        [SerializeField] private float rotationDuration = 2f;
        [SerializeField] private float rotationAngle = 360f;

        [Header("Auto Play")]
        [SerializeField] private bool playOnStart = true;

        private Vector3 originalScale;
        private Quaternion originalRotation;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Sequence shineSequence;
        private Tween rotationTween;
        private Tween scaleTween;
        private Tween fadeTween;

        private void Awake()
        {
            originalScale = transform.localScale;
            originalRotation = transform.rotation;
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

        public void Play()
        {
            Stop();

            bool hasSequence = useScale || useGlow;

            if (hasSequence)
            {
                shineSequence = DOTween.Sequence();

                // Scale animation (if enabled)
                if (useScale)
                {
                    shineSequence.Append(transform.DOScale(originalScale * scaleMultiplier, duration)
                        .SetEase(easeType));
                    shineSequence.Append(transform.DOScale(originalScale, duration)
                        .SetEase(easeType));
                }

                // Fade animation (if enabled) - runs in parallel with scale
                if (useGlow && spriteRenderer != null)
                {
                    shineSequence.Append(spriteRenderer.DOFade(minAlpha, duration)
                        .SetEase(easeType));
                    shineSequence.Append(spriteRenderer.DOFade(maxAlpha, duration)
                        .SetEase(easeType));
                }

                shineSequence.SetLoops(-1, LoopType.Restart);
            }

            // Rotation runs independently
            if (useRotation)
            {
                PlayRotate();
            }
        }

        public void PlayScale()
        {
            scaleTween?.Kill();

            scaleTween = DOTween.Sequence()
                .Append(transform.DOScale(originalScale * scaleMultiplier, duration)
                    .SetEase(easeType))
                .Append(transform.DOScale(originalScale, duration)
                    .SetEase(easeType))
                .SetLoops(-1, LoopType.Restart);
        }

        public void StopScale()
        {
            scaleTween?.Kill();
            transform.localScale = originalScale;
        }

        public void PlayFade()
        {
            if (spriteRenderer == null) return;

            fadeTween?.Kill();

            fadeTween = DOTween.Sequence()
                .Append(spriteRenderer.DOFade(minAlpha, duration)
                    .SetEase(easeType))
                .Append(spriteRenderer.DOFade(maxAlpha, duration)
                    .SetEase(easeType))
                .SetLoops(-1, LoopType.Restart);
        }

        public void StopFade()
        {
            fadeTween?.Kill();

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        public void PlayRotate()
        {
            rotationTween?.Kill();

            rotationTween = transform.DORotate(new Vector3(0, 0, rotationAngle), rotationDuration, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
        }

        public void StopRotate()
        {
            rotationTween?.Kill();
            transform.rotation = originalRotation;
        }

        public void Stop()
        {
            shineSequence?.Kill();
            rotationTween?.Kill();
            scaleTween?.Kill();
            fadeTween?.Kill();

            transform.localScale = originalScale;
            transform.rotation = originalRotation;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        private void OnDestroy()
        {
            Stop();
        }
    }
}
