using UnityEngine;
using UnityEngine.InputSystem;
using ColorChain.Core;

namespace ColorChain.Core
{
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private LayerMask tileLayerMask = -1;
        [SerializeField] private Camera gameCamera;

        private InputSystem_Actions inputActions;
        private InputAction clickAction;
        private InputAction pointAction;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeInputSystem();
            SetupCamera();
        }

        private void OnEnable()
        {
            EnableInput();
        }

        private void OnDisable()
        {
            DisableInput();
        }

        private void OnDestroy()
        {
            CleanupInput();
        }

        #endregion

        #region Input System Setup

        private void InitializeInputSystem()
        {
            inputActions = new InputSystem_Actions();

            // Get the UI actions for clicking and pointing
            clickAction = inputActions.UI.Click;
            pointAction = inputActions.UI.Point;

            // Subscribe to click events
            clickAction.performed += OnClickPerformed;
        }

        private void SetupCamera()
        {
            if (gameCamera == null)
            {
                gameCamera = Camera.main;
                if (gameCamera == null)
                {
                    gameCamera = FindAnyObjectByType<Camera>();
                }
            }
        }

        private void EnableInput()
        {
            inputActions?.Enable();
        }

        private void DisableInput()
        {
            inputActions?.Disable();
        }

        private void CleanupInput()
        {
            if (clickAction != null)
            {
                clickAction.performed -= OnClickPerformed;
            }

            inputActions?.Dispose();
        }

        #endregion

        #region Input Event Handlers

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            if (!GameStateManager.IsGameActive) return;

            // Only handle when button is actually pressed (value > 0) to avoid double-trigger on release
            float buttonValue = context.ReadValue<float>();
            if (buttonValue == 0f) return;

            Vector2 screenPosition = pointAction.ReadValue<Vector2>();
            HandleClickAtPosition(screenPosition);
        }

        private void HandleClickAtPosition(Vector2 screenPosition)
        {
            if (gameCamera == null) return;

            // Convert screen position to world position
            Vector3 worldPosition = gameCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, gameCamera.nearClipPlane));

            // Perform raycast to find clicked tile
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, tileLayerMask);

            if (hit.collider != null)
            {
                // Try to get the Tile component
                Tile clickedTile = hit.collider.GetComponent<Tile>();
                if (clickedTile != null)
                {
                    clickedTile.HandleTileClick();
                }
            }
        }

        #endregion

        #region Public Interface

        public void SetTileLayerMask(LayerMask layerMask)
        {
            tileLayerMask = layerMask;
        }

        public void SetCamera(Camera camera)
        {
            gameCamera = camera;
        }

        #endregion
    }
}