using UnityEngine;
using System.Collections.Generic;

namespace ColorChain.Core
{
    public class TileGrid : MonoBehaviour
    {
        #region Constants and Fields

        private static readonly TileColor[] COLORS = { TileColor.Red, TileColor.Blue, TileColor.Green, TileColor.Yellow, TileColor.Purple };
        private static readonly Vector2[] DIRECTIONS = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        [Header("Grid Settings")]
        public const int WIDTH = 6;
        public const int HEIGHT = 8;
        public const float TILE_SPACING = 1.1f;

        [Header("Tile Configuration")]
        [SerializeField] private Tile _tilePrefab;
        [SerializeField] private GameObject _framePrefab;
        [SerializeField] private TileColorData tileColorData;
        [SerializeField] private Vector3 _gridOffset = Vector3.zero;

        private Tile[,] _tiles = new Tile[WIDTH, HEIGHT];
        private GameObject[,] _frames = new GameObject[WIDTH, HEIGHT];

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeGrid();
        }

        private void OnDestroy()
        {
            ClearGrid();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.darkRed;
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    Vector3 pos = GridToWorldPosition(x, y);
                    Gizmos.DrawWireCube(pos, Vector3.one * 0.9f);
                }
            }
        }

        #endregion

        #region Grid Initialization

        public void InitializeGrid()
        {
            CreateTiles();
            RandomizeColors();
        }

        private void CreateTiles()
        {
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    Vector3 worldPos = GridToWorldPosition(x, y);
                    GameObject frame = Instantiate(_framePrefab, worldPos, Quaternion.identity, transform);
                    Tile tile = Instantiate(_tilePrefab, worldPos, Quaternion.identity, transform);

                    _frames[x, y] = frame;
                    _tiles[x, y] = tile;

                    tile.Initialize(x, y, GetRandomColor(), tileColorData);
                }
            }
        }

        private void RandomizeColors()
        {
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    if (_tiles[x, y] != null)
                    {
                        // Ensure tile has the color data reference
                        if (tileColorData != null)
                        {
                            _tiles[x, y].SetTileColorData(tileColorData);
                        }

                        _tiles[x, y].SetTileColor(GetRandomColor());
                    }
                }
            }
        }

        private TileColor GetRandomColor()
        {
            return COLORS[Random.Range(0, COLORS.Length)];
        }

        #endregion

        #region Grid Access

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT)
            {
                return null;
            }
            return _tiles[x, y];
        }

        public List<Tile> GetNeighbors(int x, int y)
        {
            List<Tile> neighbors = new List<Tile>();

            foreach (Vector2 dir in DIRECTIONS)
            {
                int newX = x + (int)dir.x;
                int newY = y + (int)dir.y;

                Tile neighbor = GetTile(newX, newY);
                if (neighbor != null && neighbor.IsActive)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        private Vector3 GridToWorldPosition(int x, int y)
        {
            float worldX = (x - (WIDTH - 1) * 0.5f) * TILE_SPACING;
            float worldY = (y - (HEIGHT - 1) * 0.5f) * TILE_SPACING;
            return new Vector3(worldX, worldY, 0) + _gridOffset;
        }

        public Dictionary<TileColor, HashSet<Tile>> GetTilesByColor()
        {
            Dictionary<TileColor, HashSet<Tile>> tilesByColor = new Dictionary<TileColor, HashSet<Tile>>();
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    Tile tile = GetTile(x, y);
                    if (tile != null && tile.IsActive)
                    {
                        if (!tilesByColor.ContainsKey(tile.TileColor))
                        {
                            HashSet<Tile> encounteredColors = new HashSet<Tile>();
                            tilesByColor[tile.TileColor] = encounteredColors;
                        }

                        tilesByColor[tile.TileColor].Add(tile);
                    }
                }
            }

            return tilesByColor;
        }

        #endregion

        #region Grid Management

        public void ResetGrid()
        {
            RandomizeColors();
        }

        public void RegenerateTiles(List<Tile> tilesToRegenerate)
        {
            foreach (var tile in tilesToRegenerate)
            {
                if (tile != null)
                {
                    // Ensure tile has the color data reference
                    if (tileColorData != null)
                    {
                        tile.SetTileColorData(tileColorData);
                    }

                    // Reactivate the tile with a new random color
                    tile.SetTileColor(GetRandomColor());
                    tile.SetActive(true);

                    // Play regeneration effect if needed
                    tile.PlayActivationEffect();
                }
            }
        }

        public void ClearGrid()
        {
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    if (_tiles[x, y] != null)
                    {
                        DestroyImmediate(_tiles[x, y].gameObject);
                        _tiles[x, y] = null;
                    }

                    if(_frames[x, y] != null)
                    {
                        DestroyImmediate( _frames[x, y].gameObject);
                        _frames[x, y] = null;
                    }
                }
            }
        }

        public void UpdateTilesColor(HashSet<Tile> tiles, TileColor newColor)
        {
            foreach (var tile in tiles)
            {
                if (tile != null)
                {
                    tile.SetTileColor(newColor);
                    tile.PlayColorChangeEffect();
                }
            }
        }

        #endregion

        #region Game State Management

        public void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                case GameState.GameOver:
                case GameState.Paused:
                    SetTilesInteractive(false);
                    break;
                case GameState.Playing:
                    SetTilesInteractive(true);
                    break;
            }
        }

        private void SetTilesInteractive(bool interactive)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {

                    if (_frames[x, y] != null)
                    {
                        _frames[x, y].SetActive(interactive);
                    }

                    if (_tiles[x, y] != null)
                    {
                        _tiles[x, y].SetActive(interactive);
                    }
                }
            }
        }

        #endregion
    }
}