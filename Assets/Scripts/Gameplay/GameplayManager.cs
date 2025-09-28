using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ColorChain.Core;

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

        #endregion

        #region Initialization

        private void InitializeGameplay()
        {
            InitializeComponents();
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
                _chainReaction.OnChainCompleted += OnChainCompleted;
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
                _chainReaction.OnChainCompleted -= OnChainCompleted;
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

        #region Public Interface

        public TileGrid GetTileGrid()
        {
            return _tileGrid;
        }

        public ChainReaction GetChainReaction()
        {
            return _chainReaction;
        }

        #endregion
    }
}