using ColorChain.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ColorChain.GamePlay
{
    public class GameplayManager : MonoBehaviour
    {
        [Header("Game Components")]
        [SerializeField] private TileGrid _tileGrid;
        [SerializeField] private InputManager _inputManager;

        [Header("Gameplay Settings")]
        [SerializeField] private int _minChainSize = 2;
        [SerializeField] private float _chainDelay = 0.1f;
        [SerializeField] private float _regenerationDelay = 1.0f;
        [SerializeField] private PowerUpConfig _powerUpConfig;

        private float _multiplierTimeRemaining = 0f;

        private PowerUpSystem _powerUpSystem;
        private ChainReaction _chainReaction;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeGameplay();
        }

        private void OnDestroy()
        {
            CleanupGameplay();
        }
        private void Update()
        {
            _multiplierTimeRemaining -= Time.deltaTime;
        }

        #endregion

        #region Initialization

        private void InitializeGameplay()
        {
            InitializeComponents();
            InitializePowerUp();
            InitializeChainReaction();
            SubscribeToEvents();
        }

        private void InitializeComponents()
        {
            if (_tileGrid == null)
            {
                _tileGrid = FindAnyObjectByType<TileGrid>();
            }

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
        }
        private void InitializePowerUp()
        {
            _powerUpSystem = new PowerUpSystem(_powerUpConfig);
        }
        private void InitializeChainReaction()
        {
            _chainReaction = new ChainReaction(_tileGrid, _minChainSize, _chainDelay);
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

            if (_tileGrid != null)
            {
                _tileGrid.ResetGrid();
            }
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
        }

        private void OnPowerUp(PowerUpType type)
        {
            // 1. Show power-up name/icon (0.5s delay for anticipation)
            // 2. Execute the power-up effect
            // 3. Reset power bar to 0
            // 4. Handle any overflow (optional)
            _powerUpSystem.ResetPowerBar();
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