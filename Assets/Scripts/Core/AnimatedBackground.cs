using UnityEngine;
using DG.Tweening;

namespace ColorChain.Core
{
    /// <summary>
    /// Animates camera background color cycling through game colors
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class AnimatedBackground : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float colorTransitionDuration = 8f;
        [SerializeField] private float delayBetweenColors = 2f;
        [SerializeField] private Ease easeType = Ease.InOutSine;

        [Header("Color Palette - Choose One Style")]
        [SerializeField] private BackgroundStyle style = BackgroundStyle.Subtle;

        [Header("Custom Colors (if style = Custom)")]
        [SerializeField] private Color[] customColors = new Color[0];

        private Color[] backgroundColors;

        public enum BackgroundStyle
        {
            Subtle,          // Very light, barely noticeable
            Pastel,          // Soft pastel tones
            Dark,            // Dark subtle tones
            Monochrome,      // Single color variations
            Custom           // Use custom colors array
        }

        [Header("Options")]
        [SerializeField] private bool playOnAwake = true;
        [SerializeField] private bool loop = true;

        private Camera mainCamera;
        private Sequence colorSequence;

        private void Awake()
        {
            mainCamera = GetComponent<Camera>();
            InitializeColors();
        }

        private void Start()
        {
            if (playOnAwake)
            {
                PlayAnimation();
            }
        }

        private void InitializeColors()
        {
            switch (style)
            {
                case BackgroundStyle.Subtle:
                    // Very desaturated, almost white
                    backgroundColors = new Color[]
                    {
                        new Color(0.95f, 0.93f, 0.93f),  // Very light pink
                        new Color(0.93f, 0.94f, 0.96f),  // Very light blue
                        new Color(0.94f, 0.96f, 0.94f),  // Very light green
                        new Color(0.96f, 0.96f, 0.93f)   // Very light yellow
                    };
                    break;

                case BackgroundStyle.Pastel:
                    // Soft pastel, more visible but still gentle
                    backgroundColors = new Color[]
                    {
                        new Color(1f, 0.85f, 0.85f),     // Pastel pink
                        new Color(0.85f, 0.9f, 1f),      // Pastel blue
                        new Color(0.9f, 1f, 0.9f),       // Pastel green
                        new Color(1f, 1f, 0.85f),        // Pastel yellow
                        new Color(0.95f, 0.88f, 1f)      // Pastel purple
                    };
                    break;

                case BackgroundStyle.Dark:
                    // Dark, subtle tones
                    backgroundColors = new Color[]
                    {
                        new Color(0.15f, 0.13f, 0.14f),  // Dark reddish
                        new Color(0.12f, 0.13f, 0.16f),  // Dark blue
                        new Color(0.12f, 0.15f, 0.13f),  // Dark green
                        new Color(0.14f, 0.14f, 0.12f)   // Dark yellowish
                    };
                    break;

                case BackgroundStyle.Monochrome:
                    // Single color with slight variations
                    backgroundColors = new Color[]
                    {
                        new Color(0.92f, 0.92f, 0.94f),  // Light gray-blue
                        new Color(0.88f, 0.88f, 0.90f),  // Medium gray-blue
                        new Color(0.85f, 0.85f, 0.87f),  // Darker gray-blue
                        new Color(0.90f, 0.90f, 0.92f)   // Light gray-blue
                    };
                    break;

                case BackgroundStyle.Custom:
                    backgroundColors = customColors.Length > 0 ? customColors : new Color[] { Color.white };
                    break;
            }
        }

        public void PlayAnimation()
        {
            // Kill existing sequence if any
            colorSequence?.Kill();

            // Set initial color
            if (backgroundColors.Length > 0)
            {
                mainCamera.backgroundColor = backgroundColors[0];
            }

            // Create color transition sequence
            colorSequence = DOTween.Sequence();

            for (int i = 0; i < backgroundColors.Length; i++)
            {
                int nextIndex = (i + 1) % backgroundColors.Length;
                Color targetColor = backgroundColors[nextIndex];

                colorSequence.Append(
                    mainCamera.DOColor(targetColor, colorTransitionDuration)
                        .SetEase(easeType)
                );

                if (delayBetweenColors > 0)
                {
                    colorSequence.AppendInterval(delayBetweenColors);
                }
            }

            // Loop if enabled
            if (loop)
            {
                colorSequence.SetLoops(-1);
            }

            colorSequence.Play();
        }

        public void StopAnimation()
        {
            colorSequence?.Kill();
        }

        public void PauseAnimation()
        {
            colorSequence?.Pause();
        }

        public void ResumeAnimation()
        {
            colorSequence?.Play();
        }

        public void SetColors(Color[] colors)
        {
            backgroundColors = colors;
            PlayAnimation();
        }

        private void OnDestroy()
        {
            colorSequence?.Kill();
        }
    }
}