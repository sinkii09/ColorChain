using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ColorChain.Core;
using DG.Tweening;

namespace ColorChain.UI
{
    public class TimerUIPanel : BaseUIPanel
    {
        [Header("Timer Display")]
        [SerializeField] private Image timerFillBar;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerIcon;

        [Header("Visual Settings")]
        [SerializeField] private Gradient timerColorGradient;
        [SerializeField] private float warningThreshold = 0.2f;

        private bool isWarning = false;
        private Tween warningTween;

        protected override void OnInitialize()
        {
            UpdateTimerDisplay(GameStateManager.TIME_LIMIT);
        }

        protected override void SubscribeToEvents()
        {
            GameStateManager.OnTimerChanged += UpdateTimerDisplay;
        }

        protected override void UnsubscribeFromEvents()
        {
            GameStateManager.OnTimerChanged -= UpdateTimerDisplay;
        }

        private void UpdateTimerDisplay(float timeRemaining)
        {
            float normalizedTime = timeRemaining / GameStateManager.TIME_LIMIT;

            if (timerFillBar != null)
            {
                timerFillBar.fillAmount = normalizedTime;
                timerFillBar.color = timerColorGradient.Evaluate(normalizedTime);
            }

            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(timeRemaining);
                timerText.text = seconds.ToString();
            }

            CheckWarningState(normalizedTime);
        }

        private void CheckWarningState(float normalizedTime)
        {
            bool shouldWarn = normalizedTime <= warningThreshold;

            if (shouldWarn && !isWarning)
            {
                StartWarningAnimation();
            }
            else if (!shouldWarn && isWarning)
            {
                StopWarningAnimation();
            }
        }

        private void StartWarningAnimation()
        {
            isWarning = true;

            if (timerIcon != null)
            {
                warningTween = timerIcon.transform.DOScale(1.2f, 0.3f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        private void StopWarningAnimation()
        {
            isWarning = false;

            if (warningTween != null)
            {
                warningTween.Kill();
                warningTween = null;
            }

            if (timerIcon != null)
            {
                timerIcon.transform.localScale = Vector3.one;
            }
        }

        public void AddBonusTime(float bonusTime)
        {
            if (timerIcon != null)
            {
                timerIcon.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 2);
            }

            if (timerFillBar != null)
            {
                timerFillBar.transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1f), 0.5f, 2);
            }
        }

        protected override void OnCleanup()
        {
            StopWarningAnimation();
            DOTween.Kill(timerIcon?.transform);
            DOTween.Kill(timerFillBar?.transform);
        }
    }
}