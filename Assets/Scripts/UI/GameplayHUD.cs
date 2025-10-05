using UnityEngine;
using ColorChain.Core;
using UnityEngine.UI;
using DG.Tweening;

namespace ColorChain.UI
{
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

        [Header("Animation Settings")]
        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;

        private static GameplayHUD instance;
        public static GameplayHUD Instance => instance;

        private Vector2 topBarHiddenPos;
        private Vector2 topBarVisiblePos;
        private Vector2 bottomBarHiddenPos;
        private Vector2 bottomBarVisiblePos;

        private Sequence topBarSequence;
        private Sequence bottomBarSequence;

        private bool _isActive = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            // Initialize bar positions in Awake before SafeArea modifies layout
            InitializeBarPositions();
        }

        private void Start()
        {
            InitializePanels();
        }

        private void InitializeBarPositions()
        {
            if (topBar != null)
            {
                topBarVisiblePos = topBar.anchoredPosition;
                topBarHiddenPos = new Vector2(topBarVisiblePos.x, topBarVisiblePos.y + topBar.rect.height);
                topBar.anchoredPosition = topBarHiddenPos; // Start hidden
            }

            if (bottomBar != null)
            {
                bottomBarVisiblePos = bottomBar.anchoredPosition;
                bottomBarHiddenPos = new Vector2(bottomBarVisiblePos.x, bottomBarVisiblePos.y - bottomBar.rect.height);
                bottomBar.anchoredPosition = bottomBarHiddenPos; // Start hidden
            }
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

            topBarSequence?.Kill();
            bottomBarSequence?.Kill();

            if (instance == this)
                instance = null;
        }

        public void ShowGameplayUI()
        {
            if (_isActive) return;
            _isActive = true;

            if (scorePanel != null) scorePanel.Show();
            if (timerPanel != null) timerPanel.Show();
            if (powerUpPanel != null) powerUpPanel.Show();

            if (pauseButton != null)
                pauseButton.gameObject.SetActive(true);

            ShowBars();
        }

        public void HideGameplayUI()
        {
            if (!_isActive) return;
            _isActive = false;

            HideBars();

            if (scorePanel != null) scorePanel.Hide();
            if (timerPanel != null) timerPanel.Hide();
            if (powerUpPanel != null) powerUpPanel.Hide();

            if (pauseButton != null)
                pauseButton.gameObject.SetActive(false);
        }

        private void ShowBars()
        {
            if (topBar != null)
            {
                topBarSequence?.Kill();
                topBarSequence = DOTween.Sequence();
                topBarSequence.Append(topBar.DOAnchorPos(topBarVisiblePos, transitionDuration)
                    .SetEase(showEase));
            }

            if (bottomBar != null)
            {
                bottomBarSequence?.Kill();
                bottomBarSequence = DOTween.Sequence();
                bottomBarSequence.Append(bottomBar.DOAnchorPos(bottomBarVisiblePos, transitionDuration)
                    .SetEase(showEase));
            }
        }

        private void HideBars()
        {
            if (topBar != null)
            {
                topBarSequence?.Kill();
                topBarSequence = DOTween.Sequence();
                topBarSequence.Append(topBar.DOAnchorPos(topBarHiddenPos, transitionDuration)
                    .SetEase(hideEase));
            }

            if (bottomBar != null)
            {
                bottomBarSequence?.Kill();
                bottomBarSequence = DOTween.Sequence();
                bottomBarSequence.Append(bottomBar.DOAnchorPos(bottomBarHiddenPos, transitionDuration)
                    .SetEase(hideEase));
            }
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