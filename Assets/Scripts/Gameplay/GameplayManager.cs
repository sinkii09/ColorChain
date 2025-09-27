using UnityEngine;
using ColorChain.Core;

namespace ColorChain.GamePlay
{
    public class GameplayManager : MonoBehaviour
    {
        [Header("Game Components")]
        [SerializeField] private TileGrid _tileGrid;

        [Header("Gameplay Settings")]
        [SerializeField] private int _minChainSize = 2;
        [SerializeField] private float _chainDelay = 0.1f;

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
        }

        private void InitializeChainReaction()
        {
            _chainReaction = new ChainReaction(_tileGrid, _minChainSize, _chainDelay);
        }

        private void SubscribeToEvents()
        {
            // Subscribe to tile events
            Tile.OnTileClicked += OnTileClicked;

            // Subscribe to game manager events
            GameStateManager.OnGameStarted += OnGameStarted;
            GameStateManager.OnGameEnded += OnGameEnded;
            GameStateManager.OnStateChanged += OnGameStateChanged;
        }

        private void CleanupGameplay()
        {
            // Unsubscribe from events
            Tile.OnTileClicked -= OnTileClicked;
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