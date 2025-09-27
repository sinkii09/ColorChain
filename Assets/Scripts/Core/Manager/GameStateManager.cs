using System;
using UnityEngine;

namespace ColorChain.Core
{
    public static class GameStateManager
    {
        public const float TIME_LIMIT = 60f;

        private static GameState _currentState = GameState.MainMenu;
        private static float _timer = TIME_LIMIT;
        private static bool _isGameActive = false;

        public static GameState CurrentState => _currentState;
        public static float GameTimer => _timer;
        public static bool IsGameActive => _isGameActive;

        public static event Action<GameState> OnStateChanged;
        public static event Action<float> OnTimerChanged;
        public static event Action OnGameStarted;
        public static event Action OnGameEnded;

        public static void Initialize()
        {
            _currentState = GameState.MainMenu;
            _timer = TIME_LIMIT;
            _isGameActive = false;
        }

        public static void Update(float deltaTime)
        {
            UpdateTimer(deltaTime);
        }

        public static void Terminate()
        {
            _isGameActive = false;
            OnStateChanged = null;
            OnTimerChanged = null;
            OnGameStarted = null;
            OnGameEnded = null;
        }

        public static void StartGame()
        {
            _currentState = GameState.Playing;
            _timer = TIME_LIMIT;
            _isGameActive = true;

            OnStateChanged?.Invoke(_currentState);
            OnGameStarted?.Invoke();

            Debug.Log("Game Started");
        }

        public static void PauseGame()
        {
            if (_currentState == GameState.Playing)
            {
                _currentState = GameState.Paused;
                _isGameActive = false;
                OnStateChanged?.Invoke(_currentState);
            }
        }

        public static void ResumeGame()
        {
            if (_currentState == GameState.Paused)
            {
                _currentState = GameState.Playing;
                _isGameActive = true;
                OnStateChanged?.Invoke(_currentState);
            }
        }

        public static void EndGame()
        {
            _currentState = GameState.GameOver;
            _isGameActive = false;

            OnStateChanged?.Invoke(_currentState);
            OnGameEnded?.Invoke();
        }

        public static void UpdateTimer(float deltaTime)
        {
            if (_isGameActive && _currentState == GameState.Playing)
            {
                _timer -= deltaTime;
                OnTimerChanged?.Invoke(_timer);

                if (_timer <= 0f)
                {
                    _timer = 0f;
                    EndGame();
                }
            }
        }
    }
}