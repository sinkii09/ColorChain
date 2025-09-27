using System;
using UnityEngine;

namespace ColorChain.Core
{
    public static class ScoreManager
    {
        #region Constants and Fields

        private const int BASE_POINTS = 100;
        private const float CHAIN_MULTIPLIER = 1.5f;
        private const string HIGH_SCORE_KEY = "HighScore";

        private static int _currentScore = 0;
        private static int _highScore = 0;
        private static int _lastChainScore = 0;
        private static int _chainCount = 0;

        #endregion

        #region Properties

        public static int CurrentScore => _currentScore;
        public static int HighScore => _highScore;
        public static int LastChainScore => _lastChainScore;
        public static int ChainCount => _chainCount;

        #endregion

        #region Events

        public static event Action<int> OnScoreChanged;
        public static event Action<int> OnHighScoreBeaten;
        public static event Action<int, int> OnChainScored; // (chainSize, points)

        #endregion

        #region Initialization

        public static void Initialize()
        {
            LoadHighScore();
            ResetCurrentScore();
        }

        public static void Terminate()
        {
            SaveHighScore();
            OnScoreChanged = null;
            OnHighScoreBeaten = null;
            OnChainScored = null;
        }

        #endregion

        #region Scoring

        public static void AddChainScore(int chainSize)
        {
            if (chainSize <= 0) return;

            int chainPoints = CalculateChainScore(chainSize);
            _lastChainScore = chainPoints;
            _currentScore += chainPoints;
            _chainCount++;

            // Trigger events
            OnChainScored?.Invoke(chainSize, chainPoints);
            OnScoreChanged?.Invoke(_currentScore);

            // Check for new high score
            if (_currentScore > _highScore)
            {
                _highScore = _currentScore;
                OnHighScoreBeaten?.Invoke(_highScore);
                SaveHighScore();
            }

            Debug.Log($"Chain of {chainSize} tiles scored {chainPoints} points. Total: {_currentScore}");
        }

        public static int CalculateChainScore(int chainSize)
        {
            if (chainSize <= 0) return 0;

            // Formula: BasePoints × ChainSize × Multiplier^(ChainSize-1)
            float multiplier = Mathf.Pow(CHAIN_MULTIPLIER, chainSize - 1);
            int score = Mathf.RoundToInt(BASE_POINTS * chainSize * multiplier);

            return score;
        }

        public static void ResetCurrentScore()
        {
            _currentScore = 0;
            _lastChainScore = 0;
            _chainCount = 0;
            OnScoreChanged?.Invoke(_currentScore);
        }

        #endregion

        #region High Score Persistence

        private static void LoadHighScore()
        {
            _highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        }

        private static void SaveHighScore()
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, _highScore);
            PlayerPrefs.Save();
        }

        public static void ResetHighScore()
        {
            _highScore = 0;
            SaveHighScore();
            OnScoreChanged?.Invoke(_currentScore);
        }

        #endregion

        #region Utility

        public static string FormatScore(int score)
        {
            if (score >= 1000000)
                return $"{score / 1000000f:F1}M";
            else if (score >= 1000)
                return $"{score / 1000f:F1}K";
            else
                return score.ToString();
        }

        public static float GetScoreMultiplier(int chainSize)
        {
            return Mathf.Pow(CHAIN_MULTIPLIER, chainSize - 1);
        }

        #endregion
    }
}