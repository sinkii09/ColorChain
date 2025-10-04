using UnityEngine;

namespace ColorChain.Core
{
    /// <summary>
    /// Automatically adjusts camera size to fit the tile grid in any resolution
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFitGrid : MonoBehaviour
    {
        [Header("Grid Dimensions")]
        [Tooltip("Number of columns in the grid")]
        [SerializeField] private int gridWidth = TileGrid.WIDTH;

        [Tooltip("Number of rows in the grid")]
        [SerializeField] private int gridHeight = TileGrid.HEIGHT;

        [Tooltip("Spacing between tiles (must match TileGrid spacing)")]
        [SerializeField] private float tileSpacing = TileGrid.TILE_SPACING;

        [Header("Camera Settings")]
        [Tooltip("Extra padding around the grid (in world units)")]
        [SerializeField] private float padding = 1.0f;

        [Tooltip("Update camera size when screen resolution changes")]
        [SerializeField] private bool updateOnResize = true;

        private Camera mainCamera;
        private Vector2 lastScreenSize;

        private void Awake()
        {
            mainCamera = GetComponent<Camera>();

            if (!mainCamera.orthographic)
            {
                Debug.LogWarning("CameraFitGrid: Camera should be orthographic for 2D grid!");
            }
        }

        private void Start()
        {
            AdjustCameraSize();
            lastScreenSize = new Vector2(Screen.width, Screen.height);
        }

        private void Update()
        {
            if (updateOnResize)
            {
                Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
                if (currentScreenSize != lastScreenSize)
                {
                    AdjustCameraSize();
                    lastScreenSize = currentScreenSize;
                }
            }
        }

        /// <summary>
        /// Calculates and sets the optimal camera size to fit the grid
        /// </summary>
        [ContextMenu("Adjust Camera Size")]
        public void AdjustCameraSize()
        {
            // Calculate grid dimensions in world units
            float gridWorldWidth = (gridWidth - 1) * tileSpacing;
            float gridWorldHeight = (gridHeight - 1) * tileSpacing;

            // Add padding
            float totalWidth = gridWorldWidth + (padding * 2);
            float totalHeight = gridWorldHeight + (padding * 2);

            // Get current aspect ratio
            float aspectRatio = (float)Screen.width / Screen.height;

            // Calculate required orthographic size for both dimensions
            // orthographicSize is half the vertical height
            float verticalSize = totalHeight / 2f;

            // For horizontal, we need to account for aspect ratio
            // horizontalView = orthographicSize * 2 * aspectRatio
            // So: orthographicSize = horizontalView / (2 * aspectRatio)
            float horizontalSize = totalWidth / (2f * aspectRatio);

            // Use the larger size to ensure everything fits
            float requiredSize = Mathf.Max(verticalSize, horizontalSize);

            mainCamera.orthographicSize = requiredSize;

            Debug.Log($"CameraFitGrid: Adjusted camera size to {requiredSize:F2} " +
                      $"(Grid: {gridWidth}x{gridHeight}, Spacing: {tileSpacing}, " +
                      $"Aspect: {aspectRatio:F2}, Resolution: {Screen.width}x{Screen.height})");
        }

        /// <summary>
        /// Updates grid dimensions if they change at runtime
        /// </summary>
        public void SetGridDimensions(int width, int height, float spacing)
        {
            gridWidth = width;
            gridHeight = height;
            tileSpacing = spacing;
            AdjustCameraSize();
        }

        private void OnValidate()
        {
            // Auto-adjust in editor when values change
            if (Application.isPlaying && mainCamera != null)
            {
                AdjustCameraSize();
            }
        }

        // Visualize the grid bounds in editor
        private void OnDrawGizmosSelected()
        {
            float gridWorldWidth = (gridWidth - 1) * tileSpacing;
            float gridWorldHeight = (gridHeight - 1) * tileSpacing;

            // Draw grid bounds (red)
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldWidth, gridWorldHeight, 0));

            // Draw camera view with padding (green)
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldWidth + padding * 2, gridWorldHeight + padding * 2, 0));
        }
    }
}