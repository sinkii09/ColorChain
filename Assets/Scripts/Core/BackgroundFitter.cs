using UnityEngine;

namespace ColorChain.Core
{
    /// <summary>
    /// Creates an infinite scrolling background effect using sprite tiling
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundFitter : MonoBehaviour
    {
        [Header("Coverage Settings")]
        [Tooltip("Auto-scale to cover camera view")]
        [SerializeField] private bool autoScale = true;
        [SerializeField] private float scalePadding = 1.2f; // Extra scale to prevent edge gaps

        private SpriteRenderer spriteRenderer;
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Enable tiling
            spriteRenderer.drawMode = SpriteDrawMode.Tiled;

            if (autoScale)
            {
                FitToScreen();
            }
        }
        /// <summary>
        /// Fit tiled sprite to cover the entire camera view
        /// </summary>
        private void FitToScreen()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // Get camera bounds in world space
            float cameraHeight = cam.orthographicSize * 2f;
            float cameraWidth = cameraHeight * cam.aspect;

            float factor = spriteRenderer.sprite.pixelsPerUnit / 100;

            // For Tiled mode, set the size property with padding
            spriteRenderer.size = new Vector2(
                cameraWidth * scalePadding * factor,
                cameraHeight * scalePadding * factor
            );
        }
    }
}
