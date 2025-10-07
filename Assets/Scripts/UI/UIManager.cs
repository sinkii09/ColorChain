using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ColorChain.Core;

namespace ColorChain.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameplayHUD gameplayHUD;
        [SerializeField] private Menu mainMenu;
        [SerializeField] private PausePanel pausePanel;
        [SerializeField] private GameOverPanel gameOverPanel;

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
            pausePanel.Initialize();
            gameOverPanel.Initialize();
            SetPanelActive(gameplayHUD.gameObject, false);

            ShowMainMenuState();
        }

        private void SubscribeToEvents()
        {
            // Game state events
            GameStateManager.OnStateChangeRequested += OnStateChangeRequested;
            GameStateManager.OnStateChanged += OnGameStateChanged;
        }
        private void UnsubscribeFromEvents()
        {
            // Game state events
            GameStateManager.OnStateChangeRequested -= OnStateChangeRequested;
            GameStateManager.OnStateChanged -= OnGameStateChanged;
        }


        #endregion

        #region Game State UI

        private void OnStateChangeRequested(GameState oldState, GameState newState, System.Action onComplete)
        {
            StartCoroutine(HandleStateTransition(oldState, newState, onComplete));
        }

        private System.Collections.IEnumerator HandleStateTransition(GameState oldState, GameState newState, System.Action onComplete)
        {
            // Exit current state UI with animations
            switch (oldState)
            {
                case GameState.Paused:
                    if (pausePanel != null)
                    {
                        pausePanel.Hide();
                        // Wait for pause panel hide animation
                        yield return new WaitForSeconds(pausePanel.GetTransitionDuration());
                    }
                    break;
                case GameState.MainMenu:
                    if (mainMenu != null)
                    {
                        mainMenu.Hide();
                        // Wait for main menu hide animation
                        yield return new WaitForSeconds(mainMenu.GetTransitionDuration());
                    }
                    break;
                case GameState.GameOver:
                    if (gameOverPanel != null)
                    {
                        gameOverPanel.Hide();
                        // Wait for game over panel hide animation
                        yield return new WaitForSeconds(gameOverPanel.GetTransitionDuration());
                    }
                    break;
            }

            // Complete the state change
            onComplete?.Invoke();
        }

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
            if (gameplayHUD != null)
                gameplayHUD.HideGameplayUI();

            if (mainMenu != null)
                mainMenu.Show();
        }

        private void ShowPlayingState()
        {
            if (gameplayHUD != null && gameplayHUD.gameObject.activeSelf == false)
                SetPanelActive(gameplayHUD.gameObject, true);

            if (gameplayHUD != null)
                gameplayHUD.ShowGameplayUI();
        }

        private void ShowPausedState()
        {
            if (pausePanel == null) return;
            if (pausePanel.gameObject.activeSelf == false) 
                pausePanel.gameObject.SetActive(true);

            pausePanel.Show();
        }

        private void ShowGameOverState()
        {
            if (gameOverPanel == null) return;
            if (gameOverPanel.gameObject.activeSelf == false)
                gameOverPanel.gameObject.SetActive(true);

            gameOverPanel.Show();
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