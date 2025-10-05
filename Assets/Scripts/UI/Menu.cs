using ColorChain.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ColorChain.UI
{
    public class Menu : BaseUIPanel
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button startGameButton;
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

        protected override void OnInitialize()
        {
        }

        protected override void SubscribeToEvents()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);
        }

        protected override void UnsubscribeFromEvents()
        {
            if (startGameButton != null)
                startGameButton.onClick.RemoveListener(OnStartGameClicked);
        }
        #endregion
    }
}