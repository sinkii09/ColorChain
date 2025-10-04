using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ColorChain.Core;

namespace ColorChain.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("HUD System")]
        [SerializeField] private GameplayHUD gameplayHUD;
        [SerializeField] private Menu mainMenu;

        [Header("Game State UI")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject pausePanel;

        private static UIManager instance;
        public static UIManager Instance => instance;

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            if (instance == this)
                instance = null;
        }

        #endregion

        #region Initialization

        private void InitializeUI()
        {
            // Setup initial game state UI
            ShowMainMenuState();

            // Setup button listeners

        }

        private void SubscribeToEvents()
        {
            // Game state events
            GameStateManager.OnStateChanged += OnGameStateChanged;
        }

        private void UnsubscribeFromEvents()
        {
            // Game state events
            GameStateManager.OnStateChanged -= OnGameStateChanged;
        }


        #endregion

        #region Game State UI

        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    ShowMainMenuState();
                    break;
                case GameState.Playing:
                    ShowPlayingState();
                    break;
                case GameState.Paused:
                    ShowPausedState();
                    break;
                case GameState.GameOver:
                    ShowGameOverState();
                    break;
            }
        }

        private void ShowMainMenuState()
        {
            SetPanelActive(gameOverPanel, false);

            if (gameplayHUD != null)
                gameplayHUD.HideGameplayUI();

            if (mainMenu != null)
                mainMenu.ShowMainMenuUI();
        }

        private void ShowPlayingState()
        {
            SetPanelActive(gameOverPanel, false);

            if (mainMenu != null)
                mainMenu.HideMainMenuUI();

            if (gameplayHUD != null)
                gameplayHUD.ShowGameplayUI();
        }

        private void ShowPausedState()
        {
            SetPanelActive(pausePanel, true);
            SetPanelActive(gameOverPanel, false);
        }

        private void ShowGameOverState()
        {
            SetPanelActive(gameOverPanel, true);
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        #endregion
    }
}