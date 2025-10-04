using ColorChain.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorChain.UI
{
    [RequireComponent(typeof(SafeArea))]
    public class Menu : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button startGameButton;

        private void Start()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);
        }

        private void OnDestroy()
        {
            if (startGameButton != null)
                startGameButton.onClick.RemoveListener(OnStartGameClicked);
        }

        public void ShowMainMenuUI()
        {
            if (_titleText != null)
            {
                _titleText.gameObject.SetActive(true);
            }

            SetButtonActive(startGameButton, true);
        }

        public void HideMainMenuUI()
        {
            if (_titleText != null)
            {
                _titleText.gameObject.SetActive(false);
            }

            SetButtonActive(startGameButton, false);
        }
        private void SetButtonActive(Button button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        #region Button Handlers

        private void OnStartGameClicked()
        {
            GameStateManager.StartGame();
        }
        #endregion
    }
}