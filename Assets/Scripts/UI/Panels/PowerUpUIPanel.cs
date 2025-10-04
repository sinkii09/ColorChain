using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ColorChain.Core;
using DG.Tweening;
using ColorChain.GamePlay;

namespace ColorChain.UI
{
    public class PowerUpUIPanel : BaseUIPanel
    {
        [Header("Power Bar")]
        [SerializeField] private Slider powerBarSlider;
        [SerializeField] private Image powerBarFill;
        [SerializeField] private Image powerBarGlow;

        [Header("Power-Up Icons")]
        [SerializeField] private Image powerUpIcon;
        [SerializeField] private Sprite colorConverterIcon;
        [SerializeField] private Sprite timeBonusIcon;
        [SerializeField] private Sprite scoreMultiplierIcon;

        [Header("Active Effects")]
        [SerializeField] private Image multiplierActiveIcon;
        [SerializeField] private Image multiplierTimerFill;

        [Header("Visual Settings")]
        [SerializeField] private Gradient powerBarGradient;
        [SerializeField] private float iconAnimationDuration = 2f;

        private PowerUpSystem powerUpSystem;
        private float multiplierDuration = 0f;
        private float multiplierTimer = 0f;
        private bool isMultiplierActive = false;

        protected override void OnInitialize()
        {
            if (powerBarSlider != null)
            {
                powerBarSlider.minValue = 0f;
                powerBarSlider.maxValue = 100f;
                powerBarSlider.value = 0f;
            }

            HidePowerUpIcon();
            HideMultiplierIndicator();
            UpdatePowerBarVisual(0f);
        }

        protected override void SubscribeToEvents()
        {
            if (GameplayManager.Instance != null && GameplayManager.Instance.PowerUpSystem != null)
            {
                powerUpSystem = GameplayManager.Instance.PowerUpSystem;
                powerUpSystem.OnPowerUp += OnPowerUpActivated;
            }
        }

        protected override void UnsubscribeFromEvents()
        {
            if (powerUpSystem != null)
            {
                powerUpSystem.OnPowerUp -= OnPowerUpActivated;
            }
        }

        private void Update()
        {
            if (powerUpSystem != null)
            {
                UpdatePowerBar(powerUpSystem.CurrentPowerBar);
            }

            if (isMultiplierActive)
            {
                UpdateMultiplierTimer();
            }
        }

        private void UpdatePowerBar(float powerValue)
        {
            if (powerBarSlider != null)
            {
                powerBarSlider.DOValue(powerValue, 0.3f);
            }

            UpdatePowerBarVisual(powerValue);
        }

        private void UpdatePowerBarVisual(float powerValue)
        {
            float normalizedValue = powerValue / 100f;

            if (powerBarFill != null && powerBarGradient != null)
            {
                powerBarFill.color = powerBarGradient.Evaluate(normalizedValue);
            }

            if (powerBarGlow != null)
            {
                powerBarGlow.gameObject.SetActive(powerValue >= 100f);
                if (powerValue >= 100f)
                {
                    AnimatePowerBarGlow();
                }
            }
        }

        private void AnimatePowerBarGlow()
        {
            if (powerBarGlow != null)
            {
                powerBarGlow.DOFade(0.3f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        private void OnPowerUpActivated(PowerUpType type)
        {
            ShowPowerUpIcon(type);

            if (type == PowerUpType.ScoreMultiplier)
            {
                StartMultiplierEffect();
            }

            if (powerUpSystem != null)
            {
                powerUpSystem.ResetPowerBar();
            }
        }

        private void ShowPowerUpIcon(PowerUpType type)
        {
            if (powerUpIcon == null) return;

            Sprite icon = GetPowerUpIcon(type);
            if (icon != null)
            {
                powerUpIcon.sprite = icon;
                powerUpIcon.gameObject.SetActive(true);
                AnimatePowerUpIcon();
            }
        }

        private Sprite GetPowerUpIcon(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.ColorConverter:
                    return colorConverterIcon;
                case PowerUpType.TimeBonus:
                    return timeBonusIcon;
                case PowerUpType.ScoreMultiplier:
                    return scoreMultiplierIcon;
                default:
                    return null;
            }
        }

        private void AnimatePowerUpIcon()
        {
            if (powerUpIcon == null) return;

            Sequence sequence = DOTween.Sequence();

            powerUpIcon.transform.localScale = Vector3.zero;
            powerUpIcon.DOFade(1f, 0f);

            sequence.Append(powerUpIcon.transform.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack));
            sequence.Append(powerUpIcon.transform.DOScale(1f, 0.2f));
            sequence.AppendInterval(iconAnimationDuration);
            sequence.Append(powerUpIcon.DOFade(0f, 0.3f));
            sequence.OnComplete(() => HidePowerUpIcon());
        }

        private void HidePowerUpIcon()
        {
            if (powerUpIcon != null)
            {
                powerUpIcon.gameObject.SetActive(false);
            }
        }

        private void StartMultiplierEffect()
        {
            isMultiplierActive = true;
            multiplierDuration = powerUpSystem?.MultiplierDuration ?? 5f;
            multiplierTimer = multiplierDuration;

            ShowMultiplierIndicator();
        }

        private void ShowMultiplierIndicator()
        {
            if (multiplierActiveIcon != null)
            {
                multiplierActiveIcon.gameObject.SetActive(true);
                multiplierActiveIcon.transform.DOScale(1.2f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            if (multiplierTimerFill != null)
            {
                multiplierTimerFill.gameObject.SetActive(true);
                multiplierTimerFill.fillAmount = 1f;
            }
        }

        private void UpdateMultiplierTimer()
        {
            multiplierTimer -= Time.deltaTime;

            if (multiplierTimerFill != null)
            {
                multiplierTimerFill.fillAmount = multiplierTimer / multiplierDuration;
            }

            if (multiplierTimer <= 0)
            {
                EndMultiplierEffect();
            }
        }

        private void EndMultiplierEffect()
        {
            isMultiplierActive = false;
            HideMultiplierIndicator();
        }

        private void HideMultiplierIndicator()
        {
            if (multiplierActiveIcon != null)
            {
                DOTween.Kill(multiplierActiveIcon.transform);
                multiplierActiveIcon.transform.DOScale(0f, 0.3f)
                    .OnComplete(() => multiplierActiveIcon.gameObject.SetActive(false));
            }

            if (multiplierTimerFill != null)
            {
                multiplierTimerFill.gameObject.SetActive(false);
            }
        }

        protected override void OnCleanup()
        {
            DOTween.Kill(this);
            DOTween.Kill(powerBarGlow);
            DOTween.Kill(powerUpIcon);
            DOTween.Kill(multiplierActiveIcon?.transform);
        }
    }
}