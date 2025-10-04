using UnityEngine;
using TMPro;
using ColorChain.Core;
using DG.Tweening;

namespace ColorChain.UI
{
    public class ScoreUIPanel : BaseUIPanel
    {
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;

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
            ScoreManager.OnHighScoreBeaten += UpdateHighScoreDisplay;
        }

        protected override void UnsubscribeFromEvents()
        {
            ScoreManager.OnScoreChanged -= OnScoreChanged;
            ScoreManager.OnHighScoreBeaten -= UpdateHighScoreDisplay;
        }

        private void OnScoreChanged(int newScore)
        {
            AnimateScoreChange(newScore);
        }

        private void UpdateScoreDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString();
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

        private void UpdateHighScoreDisplay(int highScore)
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"BEST: {highScore}";
            }
        }

        protected override void OnCleanup()
        {
            DOTween.Kill(this);
            DOTween.Kill(scoreText?.transform);
        }
    }
}