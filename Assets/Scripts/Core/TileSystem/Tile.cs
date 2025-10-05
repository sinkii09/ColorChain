using UnityEngine;
using System;
using DG.Tweening;

namespace ColorChain.Core
{
    public class Tile : MonoBehaviour
    {
        #region Fields & Properties

        [Header("Tile Properties")]
        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private TileColor _tileColor;
        [SerializeField] private bool _isActive;

        [Header("Components")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _tileCollider;

        [Header("Visual Configuration")]
        [SerializeField] private TileColorData _tileColorData;
        [SerializeField] private float _animationDuration = 0.2f;

        private Sequence activationSequence;
        private Sequence deactivationSequence;
        private Sequence colorChangeSequence;
        private Sequence noChainSequence;

        public bool IsActive => _isActive;
        public TileColor TileColor => _tileColor;

        #endregion

        #region Events

        public static event Action<Tile> OnTileClicked;

        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            SetupComponents();
        }

        private void OnDestroy()
        {
            // Clean up all sequences to prevent memory leaks
            activationSequence?.Kill();
            deactivationSequence?.Kill();
            colorChangeSequence?.Kill();
            noChainSequence?.Kill();
        }

        #endregion

        #region Initialization

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

        public void Initialize(int gridX, int gridY, TileColor color, TileColorData colorData = null)
        {
            SetTilePos(gridX, gridY);
            if (colorData != null)
            {
                SetTileColorData(colorData);
            }
            SetTileColor(color);
            _isActive = true;
        }

        public void SetTileColorData(TileColorData colorData)
        {
            _tileColorData = colorData;
            // Update visuals if we already have a color set
            if (_tileColor != TileColor.Red || _spriteRenderer != null)
            {
                UpdateVisualColor();
            }
        }

        #endregion

        #region Public API

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
            if (active)
            {
                gameObject.SetActive(true);
                PlayActivationEffect();
            }
            else
            {
                PlayDeactivationEffect(() => gameObject.SetActive(false));
            }
        }

        #endregion

        #region Visual Updates

        private void UpdateVisualColor()
        {
            if (_spriteRenderer != null && _tileColorData != null)
            {
                // Set the sprite for this tile color
                Sprite tileSprite = _tileColorData.GetSpriteForColor(_tileColor);
                if (tileSprite != null)
                {
                    _spriteRenderer.sprite = tileSprite;
                }

                // Apply overlay color (tint) if specified
                Color overlayColor = _tileColorData.GetOverlayColorForColor(_tileColor);
                _spriteRenderer.color = overlayColor;

                PlayColorChangeEffect();
                // Update collider size to match new sprite if needed
                UpdateColliderSize();
            }
            else
            {
                // Fallback to solid colors if no sprite data is available
                _spriteRenderer.color = GetFallbackColorFromTileColor(_tileColor);
            }
        }

        private void UpdateColliderSize()
        {
            if (_tileCollider is BoxCollider2D boxCollider && _spriteRenderer != null && _spriteRenderer.sprite != null)
            {
                boxCollider.size = _spriteRenderer.sprite.bounds.size;
            }
        }

        private Color GetFallbackColorFromTileColor(TileColor color)
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

        #endregion

        #region Input Handling

        public void HandleTileClick()
        {
            if (!_isActive || !GameStateManager.IsGameActive) return;

            Debug.Log($"Tile clicked at ({x}, {y}) with color {_tileColor}");
            OnTileClicked?.Invoke(this);
        }

        #endregion

        #region Visual Effects

        private void PlayActivationEffect()
        {
            activationSequence?.Kill();

            transform.localScale = Vector3.one;

            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = 1f;
                _spriteRenderer.color = c;
            }

            activationSequence = DOTween.Sequence();

            // Pop scale effect
            activationSequence.Append(transform.DOPunchScale(Vector3.one * 0.2f, _animationDuration, 5, 0.5f));

            // Flash effect
            if (_spriteRenderer != null)
            {
                Color originalColor = _spriteRenderer.color;
                activationSequence.Join(_spriteRenderer.DOColor(Color.white, _animationDuration / 2));
                activationSequence.Append(_spriteRenderer.DOColor(originalColor, _animationDuration));
            }
        }

        private void PlayDeactivationEffect(System.Action onComplete = null)
        {
            deactivationSequence?.Kill();

            if (_spriteRenderer == null)
            {
                onComplete?.Invoke();
                return;
            }

            // Shrink and fade out
            deactivationSequence = DOTween.Sequence();
            deactivationSequence.Append(transform.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack));
            deactivationSequence.Join(_spriteRenderer.DOFade(0f, _animationDuration));
            deactivationSequence.OnComplete(() => onComplete?.Invoke());
        }

        public void PlayColorChangeEffect()
        {
            if (_spriteRenderer == null) return;

            colorChangeSequence?.Kill();

            // Quick rotation and scale pulse
            colorChangeSequence = DOTween.Sequence();
            colorChangeSequence.Append(transform.DOPunchRotation(new Vector3(0, 0, 15f), _animationDuration, 5, 0.5f));
            colorChangeSequence.Join(transform.DOPunchScale(Vector3.one * 0.15f, _animationDuration, 3, 0.3f));
        }

        public void PlayNoChainEffect()
        {
            if (_spriteRenderer == null) return;

            noChainSequence?.Kill();

            // Shake and darken to indicate invalid move
            noChainSequence = DOTween.Sequence();

            // Horizontal shake (like saying "no")
            Vector3 originalPos = transform.localPosition;
            noChainSequence.Append(transform.DOShakePosition(_animationDuration * 1.5f, new Vector3(0.15f, 0, 0), 20, 90, false, true));
            noChainSequence.OnComplete(() => transform.localPosition = originalPos);

            // Darken briefly
            Color originalColor = _spriteRenderer.color;
            Color darkenedColor = originalColor * 0.6f;
            darkenedColor.a = originalColor.a;

            noChainSequence.Join(_spriteRenderer.DOColor(darkenedColor, _animationDuration * 0.5f));
            noChainSequence.Append(_spriteRenderer.DOColor(originalColor, _animationDuration));
        }

        #endregion
    }
}