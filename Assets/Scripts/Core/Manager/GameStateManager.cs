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

        public static event Action<GameState, GameState, Action> OnStateChangeRequested;
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
            OnStateChangeRequested = null;
            OnStateChanged = null;
            OnTimerChanged = null;
            OnGameStarted = null;
            OnGameEnded = null;
        }

        #region State Transitions

        public static void StartGame()
        {
            TransitionToState(GameState.Playing, state =>
            {
                _timer = TIME_LIMIT;
                _isGameActive = true;
                OnGameStarted?.Invoke();
            });
        }

        public static void PauseGame()
        {
            if (_currentState != GameState.Playing) return;

            TransitionToState(GameState.Paused, state =>
            {
                _isGameActive = false;
            });
        }

        public static void ResumeGame()
        {
            if (_currentState != GameState.Paused) return;

            TransitionToState(GameState.Playing, state =>
            {
                _isGameActive = true;
            });
        }

        public static void EndGame()
        {
            TransitionToState(GameState.GameOver, state =>
            {
                _isGameActive = false;
                OnGameEnded?.Invoke();
            });
        }

        public static void ToMainMenu()
        {
            TransitionToState(GameState.MainMenu, state =>
            {
                _isGameActive = false;
                _timer = TIME_LIMIT;
            });
        }

        private static void TransitionToState(GameState newState, Action<GameState> onStateEnter)
        {
            GameState oldState = _currentState;

            if (OnStateChangeRequested != null)
            {
                // Request UI to handle transition animations
                OnStateChangeRequested.Invoke(oldState, newState, () =>
                {
                    CompleteTransition(newState, onStateEnter);
                });
            }
            else
            {
                // No UI listeners, transition immediately
                CompleteTransition(newState, onStateEnter);
            }
        }

        private static void CompleteTransition(GameState newState, Action<GameState> onStateEnter)
        {
            _currentState = newState;
            onStateEnter?.Invoke(newState);
            OnStateChanged?.Invoke(newState);
        }

        #endregion

        #region Timer Management

        private static void UpdateTimer(float deltaTime)
        {
            if (!_isGameActive || _currentState != GameState.Playing) return;

            _timer -= deltaTime;
            OnTimerChanged?.Invoke(_timer);

            if (_timer <= 0f)
            {
                _timer = 0f;
                EndGame();
            }
        }

        public static void AddBonusTime(float timeBonusAmount)
        {
            _timer += timeBonusAmount;
            OnTimerChanged?.Invoke(_timer);
        }

        #endregion
    }
}