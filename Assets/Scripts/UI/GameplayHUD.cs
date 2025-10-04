using UnityEngine;
using ColorChain.Core;
using UnityEngine.UI;

namespace ColorChain.UI
{
    [RequireComponent(typeof(SafeArea))]
    public class GameplayHUD : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private ScoreUIPanel scorePanel;
        [SerializeField] private TimerUIPanel timerPanel;
        [SerializeField] private PowerUpUIPanel powerUpPanel;

        [Header("Layout Containers")]
        [SerializeField] private RectTransform topBar;
        [SerializeField] private RectTransform bottomBar;

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;

        private static GameplayHUD instance;
        public static GameplayHUD Instance => instance;

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
            InitializePanels();
        }

        private void InitializePanels()
        {
            if (scorePanel != null)
                scorePanel.Initialize();

            if (timerPanel != null)
                timerPanel.Initialize();

            if (powerUpPanel != null)
                powerUpPanel.Initialize();

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseGameClicked);
        }

        private void OnDestroy()
        {
            if (scorePanel != null)
                scorePanel.Cleanup();

            if (timerPanel != null)
                timerPanel.Cleanup();

            if (powerUpPanel != null)
                powerUpPanel.Cleanup();

            if (pauseButton != null)
                pauseButton.onClick.RemoveListener(OnPauseGameClicked);

            if (instance == this)
                instance = null;
        }

        public void ShowGameplayUI()
        {
            if (scorePanel != null) scorePanel.Show();
            if (timerPanel != null) timerPanel.Show();
            if (powerUpPanel != null) powerUpPanel.Show();

            if (pauseButton != null)
                pauseButton.gameObject.SetActive(true);
        }

        public void HideGameplayUI()
        {
            if (scorePanel != null) scorePanel.Hide();
            if (timerPanel != null) timerPanel.Hide();
            if (powerUpPanel != null) powerUpPanel.Hide();

            if (pauseButton != null)
                pauseButton.gameObject.SetActive(false);
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
    }
}