using ColorChain.Core;
using ColorChain.Gameplay;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorChain.GamePlay
{
    public class GameplayManager : MonoBehaviour
    {
        private static GameplayManager instance;
        public static GameplayManager Instance => instance;
        public PowerUpSystem PowerUpSystem => _powerUpSystem;
        [Header("Game Components")]
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private ShiningEffect _backgroundRenderer;

        [Header("Gameplay Settings")]
        [SerializeField] private float _regenerationDelay = 1.0f;
        [SerializeField] private TileGrid _tileGridPrefab;
        [SerializeField] private PowerUpConfig _powerUpConfig;
        [SerializeField] private ChainReactionConfig _chainReactionConfig;

        private float _multiplierTimeRemaining = 0f;

        private TileGrid _tileGrid;
        private PowerUpSystem _powerUpSystem;
        private ChainReaction _chainReaction;

        #region Unity Lifecycle

        private void Awake()
        {
            DOTween.Init();
            DOTween.SetTweensCapacity(500, 200);

            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Start()
        {
            InitializeComponents();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            CleanupGameplay();
            if (instance == this)
                instance = null;
        }
        private void Update()
        {
            _multiplierTimeRemaining -= Time.deltaTime;
        }

        #endregion

        #region Initialization

        private void InitializeGameplay()
        {
            if (_tileGrid != null)
            {
                _tileGrid.ResetGrid();
                _tileGrid.gameObject.SetActive(true);
                return;
            }
            if (_tileGridPrefab != null)
            {
                _tileGrid = Instantiate(_tileGridPrefab, Vector3.zero, Quaternion.identity);
                _tileGrid.InitializeGrid();

                _chainReaction.SetTileGrid(_tileGrid);
            }
        }

        private void InitializeComponents()
        {
            if (_inputManager == null)
            {
                _inputManager = FindAnyObjectByType<InputManager>();
                if (_inputManager == null)
                {
                    // Create InputManager if it doesn't exist
                    GameObject inputManagerGO = new GameObject("InputManager");
                    _inputManager = inputManagerGO.AddComponent<InputManager>();
                }
            }

            InitializePowerUp();
            InitializeChainReaction();
        }
        private void InitializePowerUp()
        {
            _powerUpSystem = new PowerUpSystem(_powerUpConfig);
        }
        private void InitializeChainReaction()
        {
            _chainReaction = new ChainReaction(_chainReactionConfig);
        }

        private void SubscribeToEvents()
        {
            // Subscribe to tile events
            Tile.OnTileClicked += OnTileClicked;

            // Subscribe to chain reaction events (must be after ChainReaction is created)
            if (_chainReaction != null)
            {
                _chainReaction.OnChainExcecuted += OnchainExcecuted;
                _chainReaction.OnChainCompleted += OnChainCompleted;
            }

            if (_powerUpSystem != null)
            {
                _powerUpSystem.OnPowerUp += OnPowerUp;
            }

            // Subscribe to game manager events
            GameStateManager.OnGameStarted += OnGameStarted;
            GameStateManager.OnGameEnded += OnGameEnded;
            GameStateManager.OnStateChanged += OnGameStateChanged;
        }

        private void CleanupGameplay()
        {
            // Unsubscribe from events
            Tile.OnTileClicked -= OnTileClicked;

            if (_chainReaction != null)
            {
                _chainReaction.OnChainExcecuted -= OnchainExcecuted;
                _chainReaction.OnChainCompleted -= OnChainCompleted;
            }

            if (_powerUpSystem != null)
            {
                _powerUpSystem.OnPowerUp -= OnPowerUp;
            }

            GameStateManager.OnGameStarted -= OnGameStarted;
            GameStateManager.OnGameEnded -= OnGameEnded;
            GameStateManager.OnStateChanged -= OnGameStateChanged;
        }

        #endregion

        #region Event Handlers

        private void OnTileClicked(Tile clickedTile)
        {
            AudioManager.PlayTileClick();

            if (_chainReaction != null && GameStateManager.IsGameActive)
            {
                _chainReaction.StartChain(clickedTile);
            }
        }

        private void OnchainExcecuted(int chainSize)
        {
            if (_powerUpSystem == null) return;

            // Play chain sound with pitch variation based on chain size
            float pitch = 1f + (chainSize * 0.1f);
            AudioManager.PlayChainReaction(pitch);

            var calculatedSize = Mathf.RoundToInt(GetCurrentMultiplier() * chainSize);
            ScoreManager.AddChainScore(calculatedSize);

            _powerUpSystem.AddPowerBarProgress(chainSize);
        }

        private void OnChainCompleted(List<Tile> chainedTiles)
        {
            // Start coroutine to regenerate tiles after a delay
            if (_tileGrid != null)
            {
                StartCoroutine(RegenerateTilesAfterDelay(chainedTiles, _regenerationDelay));
            }
        }

        private void OnGameStarted()
        {
            // Reset score for new game
            ScoreManager.ResetCurrentScore();

            InitializeGameplay();

            AudioManager.PlayBackgroundMusic();
        }

        private void OnGameEnded()
        {
            // Handle game over logic specific to gameplay
        }

        private void OnGameStateChanged(GameState state)
        {
            if (_tileGrid != null)
            {
                _tileGrid.OnGameStateChanged(state);
            }

            switch (state)
            {
                case GameState.MainMenu:
                    _tileGrid.gameObject.SetActive(false);
                    _backgroundRenderer.Play();
                    break;
                case GameState.GameOver:
                case GameState.Paused:
                    break;
                case GameState.Playing:
                    _backgroundRenderer.StopRotate();
                    break;

            }
        }

        private void OnPowerUp(PowerUpType type)
        {
            AudioManager.PlayPowerUp();

            // Execute the power-up effect based on type
            switch (type)
            {
                case PowerUpType.ColorConverter:
                    ExecuteColorConverter();
                    break;
                case PowerUpType.TimeBonus:
                    ExecuteTimeBonus();
                    break;
                case PowerUpType.ScoreMultiplier:
                    ExecuteScoreMultiplier();
                    break;
            }
        }


        #endregion

        #region Coroutines

        private IEnumerator RegenerateTilesAfterDelay(List<Tile> tilesToRegenerate, float delay)
        {
            Debug.Log($"Starting tile regeneration in {delay} seconds for {tilesToRegenerate.Count} tiles");

            // Wait for the specified delay
            yield return new WaitForSeconds(delay);

            // Check if game is still active before regenerating
            if (GameStateManager.IsGameActive && _tileGrid != null)
            {
                _tileGrid.RegenerateTiles(tilesToRegenerate);
                Debug.Log($"Regenerated {tilesToRegenerate.Count} tiles with new colors");
            }
        }

        #endregion

        #region PowerUp Execution
        private void ExecuteColorConverter()
        {
            Dictionary<TileColor, HashSet<Tile>> tilesByColor = _tileGrid.GetTilesByColor();
            if (tilesByColor == null || tilesByColor.Count < 2)
                return;

            // Find most common color by count
            TileColor mostCommon = tilesByColor.OrderByDescending(x => x.Value.Count).First().Key;
            // Find second most common color by count
            TileColor secondCommon = tilesByColor.OrderByDescending(x => x.Value.Count).Skip(1).First().Key;

            // Execute with visual effect
            StartCoroutine(ExecuteColorConverterEffect(tilesByColor[mostCommon], secondCommon));
        }

        private IEnumerator ExecuteColorConverterEffect(HashSet<Tile> tilesToConvert, TileColor targetColor)
        {
            yield return StartCoroutine(_tileGrid.UpdateTilesColorAnimated(tilesToConvert, targetColor));
        }

        private void ExecuteTimeBonus()
        {
            GameStateManager.AddBonusTime(_powerUpSystem.TimerBonusAmount);

            // Show visual feedback
            StartCoroutine(ShowTimeBonusEffect());
        }

        private IEnumerator ShowTimeBonusEffect()
        {
            // Flash background or play particle effect
            if (_backgroundRenderer != null)
            {
                // Flash effect
                _backgroundRenderer.PlayFlash();
            }

            // TODO: Show floating text "+10s" at timer position
            Debug.Log($"+{_powerUpSystem.TimerBonusAmount}s Time Bonus!");

            yield return null;
        }

        private void ExecuteScoreMultiplier()
        {
            _multiplierTimeRemaining += _powerUpSystem.MultiplierDuration;

            // Show visual feedback
            StartCoroutine(ShowMultiplierEffect());
        }

        private IEnumerator ShowMultiplierEffect()
        {
            // TODO: Show "2X" or "3X" indicator on screen
            // TODO: Play particle burst effect
            // TODO: Change score text color temporarily

            Debug.Log($"{_powerUpSystem.MultiplierAmount}X Multiplier Active for {_powerUpSystem.MultiplierDuration}s!");

            yield return null;
        }
        #endregion


        public float GetCurrentMultiplier()
        {
            return _multiplierTimeRemaining > 0 ? _powerUpSystem.MultiplierAmount : 1f;
        }
    }
}