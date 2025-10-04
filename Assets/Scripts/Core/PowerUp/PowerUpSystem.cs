using System;
using UnityEngine;

namespace ColorChain.Core
{
    public enum PowerUpType
    {
        ColorConverter,
        ScoreMultiplier,
        TimeBonus
    }

    public class PowerUpSystem
    {
        #region Fields & Props
        // Power Bar Management
        private const float MAX_POWER_BAR = 100f;
        private float _currentPowerBar = 0f;

        private float _multiplierAmount = 2f;
        private float _multiplierDuration = 5f;
        public float _timerBonusAmount = 10f;

        private PowerUpConfig _config;

        public float TimerBonusAmount => _timerBonusAmount;
        public float MultiplierAmount => _multiplierAmount;
        public float MultiplierDuration => _multiplierDuration;
        public float CurrentPowerBar => _currentPowerBar;
        // Events
        public event Action<PowerUpType> OnPowerUp;

        #endregion

        public PowerUpSystem(PowerUpConfig config)
        {
            this._config = config;

            this._multiplierAmount = config.multiplierAmount;
            this._timerBonusAmount = config.timeBonusAmount;
            this._multiplierDuration = config.multiplierDuration;
        }

        public void AddPowerBarProgress(int chainSize)
        {
            if (chainSize >= 2 && chainSize <= 3)
                _currentPowerBar += _config.smallChainFill;
            else if (chainSize >= 4 && chainSize <= 5)
                _currentPowerBar += _config.mediumChainFill;
            else if (chainSize >= 6 && chainSize <= 7)
                _currentPowerBar += _config.largeChainFill;
            else if (chainSize >= 8)
                _currentPowerBar += _config.hugeChainFill;

            _currentPowerBar = Mathf.Min(_currentPowerBar, MAX_POWER_BAR);
            if (_currentPowerBar >= MAX_POWER_BAR)
            {
                SelectAndActivatePowerUp();
            }
        }

        private void SelectAndActivatePowerUp()
        {
            PowerUpType selectedPowerUp = SelectRandomPowerUp();
            ActivatePowerUpSequence(selectedPowerUp);
        }
        private PowerUpType SelectRandomPowerUp()
        {
            // Use weighted randomization
            // ColorConverter = 30%, TimeBonus = 40%, ScoreMultiplier = 30%
            // Return selected type
            //return PowerUpType.TimeFreeze;

            int totalWeight = _config.colorConverterWeight + _config.timeBonusWeight + _config.scoreMultiplierWeight;
            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            if (randomValue < _config.colorConverterWeight)
                return PowerUpType.ColorConverter;
            else if (randomValue < _config.colorConverterWeight + _config.timeBonusWeight)
                return PowerUpType.TimeBonus;
            else
                return PowerUpType.ScoreMultiplier;
        }

        private void ActivatePowerUpSequence(PowerUpType type)
        {
            OnPowerUp?.Invoke(type);
        }

        #region Helper

        public void ResetPowerBar()
        {
            _currentPowerBar -= MAX_POWER_BAR;
        }
        #endregion
    }
}
