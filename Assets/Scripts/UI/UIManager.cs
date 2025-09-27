using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ColorChain.Core;

namespace ColorChain.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Score UI")]
        [SerializeField] private TextMeshProUGUI currentScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI lastChainScoreText;

        [Header("Timer UI")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Slider timerSlider;

        [Header("Game State UI")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject pausePanel;

        [Header("Settings")]
        [SerializeField] private float timerFlashThreshold = 10f;
        [SerializeField] private Color normalTimerColor = Color.white;
        [SerializeField] private Color warningTimerColor = Color.red;

        private bool isTimerFlashing = false;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        private void InitializeUI()
        {
            // Initialize UI elements
            UpdateScoreDisplay(0);
            UpdateHighScoreDisplay(ScoreManager.HighScore);
            UpdateTimerDisplay(GameStateManager.TIME_LIMIT);

            // Setup initial game state UI
            ShowMainMenuState();

            // Setup button listeners
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseGameClicked);
        }

        private void SubscribeToEvents()
        {
            // Score events
            ScoreManager.OnScoreChanged += UpdateScoreDisplay;
            ScoreManager.OnHighScoreBeaten += UpdateHighScoreDisplay;
            ScoreManager.OnChainScored += OnChainScored;

            // Game state events
            GameStateManager.OnStateChanged += OnGameStateChanged;
            GameStateManager.OnTimerChanged += UpdateTimerDisplay;
        }

        private void UnsubscribeFromEvents()
        {
            // Score events
            ScoreManager.OnScoreChanged -= UpdateScoreDisplay;
            ScoreManager.OnHighScoreBeaten -= UpdateHighScoreDisplay;
            ScoreManager.OnChainScored -= OnChainScored;

            // Game state events
            GameStateManager.OnStateChanged -= OnGameStateChanged;
            GameStateManager.OnTimerChanged -= UpdateTimerDisplay;
        }

        #endregion

        #region Score UI

        private void UpdateScoreDisplay(int score)
        {
            if (currentScoreText != null)
            {
                currentScoreText.text = ScoreManager.FormatScore(score);
            }
        }

        private void UpdateHighScoreDisplay(int highScore)
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"Best: {ScoreManager.FormatScore(highScore)}";
            }
        }

        private void OnChainScored(int chainSize, int points)
        {
            if (lastChainScoreText != null)
            {
                lastChainScoreText.text = $"+{ScoreManager.FormatScore(points)}";

                // Show chain score briefly
                ShowChainScoreEffect(chainSize, points);
            }
        }

        private void ShowChainScoreEffect(int chainSize, int points)
        {
            // TODO: Add chain score animation/effect
            StartCoroutine(FadeOutChainScore());
        }

        private System.Collections.IEnumerator FadeOutChainScore()
        {
            yield return new WaitForSeconds(2f);

            if (lastChainScoreText != null)
            {
                lastChainScoreText.text = "";
            }
        }

        #endregion

        #region Timer UI

        private void UpdateTimerDisplay(float timeRemaining)
        {
            // Update timer text
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / GameStateManager.TIME_LIMIT);
                int seconds = Mathf.FloorToInt(timeRemaining % GameStateManager.TIME_LIMIT);
                timerText.text = $"{minutes:00}:{seconds:00}";

                // Handle timer warning state
                HandleTimerWarning(timeRemaining);
            }

            // Update timer slider
            if (timerSlider != null)
            {
                timerSlider.value = timeRemaining / GameStateManager.TIME_LIMIT;
            }
        }

        private void HandleTimerWarning(float timeRemaining)
        {
            if (timeRemaining <= timerFlashThreshold && !isTimerFlashing)
            {
                StartTimerFlashing();
            }
            else if (timeRemaining > timerFlashThreshold && isTimerFlashing)
            {
                StopTimerFlashing();
            }
        }

        private void StartTimerFlashing()
        {
            isTimerFlashing = true;
            if (timerText != null)
            {
                StartCoroutine(FlashTimer());
            }
        }

        private void StopTimerFlashing()
        {
            isTimerFlashing = false;
            if (timerText != null)
            {
                timerText.color = normalTimerColor;
            }
        }

        private System.Collections.IEnumerator FlashTimer()
        {
            while (isTimerFlashing)
            {
                if (timerText != null)
                {
                    timerText.color = warningTimerColor;
                    yield return new WaitForSeconds(0.5f);

                    timerText.color = normalTimerColor;
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    break;
                }
            }
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
            SetPanelActive(pausePanel, false);
            SetButtonActive(startGameButton, true);
            SetButtonActive(pauseButton, false);
        }

        private void ShowPlayingState()
        {
            SetPanelActive(gameOverPanel, false);
            SetPanelActive(pausePanel, false);
            SetButtonActive(startGameButton, false);
            SetButtonActive(pauseButton, true);
        }

        private void ShowPausedState()
        {
            SetPanelActive(pausePanel, true);
            SetPanelActive(gameOverPanel, false);
        }

        private void ShowGameOverState()
        {
            SetPanelActive(gameOverPanel, true);
            SetPanelActive(pausePanel, false);
            SetButtonActive(startGameButton, true);
            SetButtonActive(pauseButton, false);

            StopTimerFlashing();
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        private void SetButtonActive(Button button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        #endregion

        #region Button Handlers

        private void OnStartGameClicked()
        {
            GameStateManager.StartGame();
        }

        private void OnPauseGameClicked()
        {
            if (GameStateManager.CurrentState == GameState.Playing)
            {
                GameStateManager.PauseGame();
            }
            else if (GameStateManager.CurrentState == GameState.Paused)
            {
                GameStateManager.ResumeGame();
            }
        }

        #endregion
    }
}