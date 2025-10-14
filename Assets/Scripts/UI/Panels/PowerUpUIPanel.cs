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
        [SerializeField] private PowerUpBar powerBar;

        [Header("Particle Effects")]
        [SerializeField] private ParticleSystem powerBarFullParticles;

        [Header("Power-Up Icons")]
        [SerializeField] private Image powerUpIcon;
        [SerializeField] private Sprite colorConverterIcon;
        [SerializeField] private Sprite timeBonusIcon;
        [SerializeField] private Sprite scoreMultiplierIcon;

        [Header("Active Effects")]
        [SerializeField] private Image multiplierActiveIcon;
        [SerializeField] private Image multiplierTimerFill;

        [Header("Visual Settings")]
        [SerializeField] private float iconAnimationDuration = 2f;

        private PowerUpSystem powerUpSystem;
        private float multiplierDuration = 0f;
        private float multiplierTimer = 0f;
        private bool isMultiplierActive = false;
        private bool isPlayingParticles = false;

        protected override void OnInitialize()
        {
            if (powerBar != null)
            {
                powerBar.OnReset(false);
            }

            HidePowerUpIcon();
            HideMultiplierIndicator();
        }

        protected override void SubscribeToEvents()
        {
            if (GameplayManager.Instance != null && GameplayManager.Instance.PowerUpSystem != null)
            {
                powerUpSystem = GameplayManager.Instance.PowerUpSystem;
                powerUpSystem.OnPowerUp += OnPowerUpActivated;
            }

            if (powerBar != null)
            {
                powerBar.OnBarFullReached += OnPowerBarFull;
            }
        }

        protected override void UnsubscribeFromEvents()
        {
            if (powerUpSystem != null)
            {
                powerUpSystem.OnPowerUp -= OnPowerUpActivated;
            }

            if (powerBar != null)
            {
                powerBar.OnBarFullReached -= OnPowerBarFull;
            }
        }

        private void Update()
        {
            if (powerUpSystem != null && !isPlayingParticles)
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
            if (powerBar != null)
            {
                float normalizedValue = powerValue / 100f;
                powerBar.SetProgress(normalizedValue, true);
            }
        }

        private void OnPowerUpActivated(PowerUpType type)
        {
            ShowPowerUpIcon(type);

            if (type == PowerUpType.ScoreMultiplier)
            {
                StartMultiplierEffect();
            }

            // Reset internal value immediately to prevent duplicate triggers
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

        private void OnPowerBarFull()
        {
            SpawnParticleBurst();
        }

        private void SpawnParticleBurst()
        {
            if (powerBarFullParticles == null || powerBar == null) return;

            // Set flag to pause bar updates during particle effect
            isPlayingParticles = true;

            // Instantiate particle system
            ParticleSystem particleInstance = Instantiate(powerBarFullParticles);

            // Parent to the same canvas as power bar (important for UI space)
            Canvas parentCanvas = powerBar.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                particleInstance.transform.SetParent(parentCanvas.transform, false);
            }
            else
            {
                particleInstance.transform.SetParent(powerBar.transform.parent, false);
            }

            // Position at power bar using RectTransform (for UI elements)
            RectTransform particleRect = particleInstance.GetComponent<RectTransform>();
            RectTransform barRect = powerBar.GetComponent<RectTransform>();

            if (particleRect != null && barRect != null)
            {
                // Copy position from bar
                particleRect.anchoredPosition = barRect.anchoredPosition;
                particleRect.anchorMin = barRect.anchorMin;
                particleRect.anchorMax = barRect.anchorMax;
                particleRect.sizeDelta = barRect.sizeDelta;
            }
            else
            {
                // Fallback: use world position
                particleInstance.transform.position = powerBar.transform.position;
            }

            // Play particles
            particleInstance.Play();

            // Auto-destroy after main duration finishes
            float duration = particleInstance.main.duration + particleInstance.main.startLifetime.constantMax;
            Destroy(particleInstance.gameObject, duration);

            // Resume bar updates after particle effect completes
            DOTween.Sequence()
                .AppendInterval(duration)
                .OnComplete(() => isPlayingParticles = false);
        }

        protected override void OnCleanup()
        {
            DOTween.Kill(this);
            DOTween.Kill(powerUpIcon);
            DOTween.Kill(multiplierActiveIcon?.transform);
        }
    }
}