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
            if (_chainReaction != null && GameStateManager.IsGameActive)
            {
                _chainReaction.StartChain(clickedTile);
            }
        }

        private void OnchainExcecuted(int chainSize)
        {
            if (_powerUpSystem == null) return;

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

        #region PowerUp Excecution
        private void ExecuteColorConverter()
        {
            // 1. Count each color on the board
            // 2. Find most populous color
            // 3. Find second most populous color
            // 4. Convert all tiles of first color to second
            // 5. Trigger visual effects

            Dictionary<TileColor, HashSet<Tile>> tilesByColor = _tileGrid.GetTilesByColor();
            if (tilesByColor == null || tilesByColor.Count < 2)
                return;

            // Find most common color
            TileColor mostCommon = tilesByColor.OrderByDescending(x => x.Value).First().Key;
            // Find second most common color
            TileColor secondCommon = tilesByColor.OrderByDescending(x => x.Value).Skip(1).First().Key;

            _tileGrid.UpdateTilesColor(tilesByColor[mostCommon], secondCommon);
        }

        private void ExecuteTimeBonus()
        {
            // 1. Call GameStateManager.AddBonusTime(10f)
            // 2. Show "+10s" floating text
            // 3. Flash timer UI

            GameStateManager.AddBonusTime(_powerUpSystem.TimerBonusAmount);
        }

        private void ExecuteScoreMultiplier()
        {
            // 1. Set isMultiplierActive = true
            // 2. Set multiplierTimeRemaining = 5f
            // 3. Start countdown coroutine
            // 4. Show "2X" indicator on UI

            _multiplierTimeRemaining += _powerUpSystem.MultiplierDuration;
        }
        #endregion


        public float GetCurrentMultiplier()
        {
            return _multiplierTimeRemaining > 0 ? _powerUpSystem.MultiplierAmount : 1f;
        }
    }
}