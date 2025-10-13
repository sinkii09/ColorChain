using UnityEngine;
using TMPro;
using ColorChain.Core;
using ColorChain.Utils;
using DG.Tweening;

namespace ColorChain.UI
{
    public class ScoreUIPanel : BaseUIPanel
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [Header("High Score Effect Settings")]
        [SerializeField] private float highScoreScaleAmount = 0.3f;
        [SerializeField] private float highScoreScaleDuration = 0.5f;
        [SerializeField] private Color highScoreFlashColor = Color.yellow;
        [SerializeField] private float highScoreFlashDuration = 0.3f;
        [SerializeField] private int highScoreFlashCount = 3;

        private int displayedScore = 0;

        protected override void OnInitialize()
        {
            displayedScore = 0;
            UpdateScoreDisplay(0);
            UpdateHighScoreDisplay(ScoreManager.HighScore);
        }

        protected override void SubscribeToEvents()
        {
            ScoreManager.OnScoreChanged += OnScoreChanged;
            ScoreManager.OnHighScoreBeaten += OnHighScoreBeaten;
        }

        protected override void UnsubscribeFromEvents()
        {
            ScoreManager.OnScoreChanged -= OnScoreChanged;
            ScoreManager.OnHighScoreBeaten -= OnHighScoreBeaten;
        }

        private void OnScoreChanged(int newScore)
        {
            AnimateScoreChange(newScore);
        }

        private void UpdateScoreDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = NumberFormatter.FormatNumber(score);
            }
        }

        private void AnimateScoreChange(int targetScore)
        {
            DOTween.Kill(this);

            DOTween.To(() => displayedScore, x =>
            {
                displayedScore = x;
                UpdateScoreDisplay(displayedScore);
            }, targetScore, 0.5f)
            .SetEase(Ease.OutQuad);

            if (scoreText != null)
            {
                scoreText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 2);
            }
        }

        private void OnHighScoreBeaten(int newHighScore)
        {
            UpdateHighScoreDisplay(newHighScore);
            PlayHighScoreEffect();
        }

        private void UpdateHighScoreDisplay(int highScore)
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"{NumberFormatter.FormatNumber(highScore)}";
            }
        }

        private void PlayHighScoreEffect()
        {
            if (highScoreText == null) return;

            // Kill existing tweens
            DOTween.Kill(highScoreText.transform);
            DOTween.Kill(highScoreText);

            // Store original values
            Color originalColor = highScoreText.color;
            Vector3 originalScale = highScoreText.transform.localScale;

            // Create celebration sequence
            Sequence celebrationSequence = DOTween.Sequence();

            // Big scale punch
            celebrationSequence.Append(
                highScoreText.transform.DOPunchScale(Vector3.one * highScoreScaleAmount, highScoreScaleDuration, 5, 0.5f)
            );

            // Flash color multiple times
            for (int i = 0; i < highScoreFlashCount; i++)
            {
                celebrationSequence.Join(
                    highScoreText.DOColor(highScoreFlashColor, highScoreFlashDuration / 2)
                        .SetLoops(2, LoopType.Yoyo)
                        .SetDelay(i * highScoreFlashDuration)
                );
            }

            // Restore original color at the end
            celebrationSequence.OnComplete(() =>
            {
                highScoreText.color = originalColor;
                highScoreText.transform.localScale = originalScale;
            });

            // Play sound effect
            AudioManager.PlayPowerUp(); // Celebratory sound
        }

        protected override void OnCleanup()
        {
            DOTween.Kill(this);
            DOTween.Kill(scoreText?.transform);
            DOTween.Kill(highScoreText?.transform);
            DOTween.Kill(highScoreText);
        }
    }
}