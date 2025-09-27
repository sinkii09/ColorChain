using UnityEngine;
using System;

namespace ColorChain.Core
{
    public class Tile : MonoBehaviour
    {
        [Header("Tile Properties")]
        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private TileColor _tileColor;
        [SerializeField] private bool _isActive;

        [Header("Components")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _tileCollider;

        public bool IsActive => _isActive;
        public TileColor TileColor => _tileColor;

        // Events
        public static event Action<Tile> OnTileClicked;
        private void Awake()
        {
            SetupComponents();
        }

        private void SetupComponents()
        {
            // Setup SpriteRenderer
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (_spriteRenderer == null)
                {
                    _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                }
            }

            // Setup Collider2D
            if (_tileCollider == null)
            {
                _tileCollider = GetComponent<Collider2D>();
                if (_tileCollider == null)
                {
                    BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
                    _tileCollider = boxCollider;

                    // Set collider size to match sprite bounds
                    if (_spriteRenderer != null && _spriteRenderer.sprite != null)
                    {
                        boxCollider.size = _spriteRenderer.sprite.bounds.size;
                    }
                    else
                    {
                        // Default size for tiles
                        boxCollider.size = Vector2.one;
                    }
                }
            }
        }

        public void Initialize(int gridX, int gridY, TileColor color)
        {
            SetTilePos(gridX, gridY);
            SetTileColor(color);
            _isActive = true;
        }

        public void SetWorldPos(Vector3 worldPos)
        {
            transform.position = worldPos;
        }

        public void SetTilePos(int gridX, int gridY)
        {
            this.x = gridX;
            this.y = gridY;
        }

        public Vector2 GetTilePos()
        {
            return new Vector2(x, y);
        }

        public void SetTileColor(TileColor color)
        {
            _tileColor = color;
            UpdateVisualColor();
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            gameObject.SetActive(active);
        }

        private void UpdateVisualColor()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = GetColorFromTileColor(_tileColor);
            }
        }

        private Color GetColorFromTileColor(TileColor color)
        {
            return color switch
            {
                TileColor.Red => Color.red,
                TileColor.Blue => Color.blue,
                TileColor.Green => Color.green,
                TileColor.Yellow => Color.yellow,
                TileColor.Purple => new Color(0.5f, 0f, 0.5f),
                _ => Color.white
            };
        }

        public void HandleTileClick()
        {
            if (!_isActive || !GameStateManager.IsGameActive) return;

            Debug.Log($"Tile clicked at ({x}, {y}) with color {_tileColor}");
            OnTileClicked?.Invoke(this);
        }

        public void PlayActivationEffect()
        {
            // TODO: Add visual/audio feedback for tile activation
        }

        public void PlayDeactivationEffect()
        {
            // TODO: Add visual/audio feedback for tile removal
        }
    }
}