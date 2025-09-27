using ColorChain.Core;
using System.Collections.Generic;
using UnityEngine;

namespace ColorChain.GamePlay
{
    public class ChainReaction
    {
        private int _minChainSize = 2;
        private float _chainDelay = 0.1f;

        private TileGrid _tileGrid;
        //private ScoreManager _scoreManager;

        public ChainReaction(TileGrid tileGrid, int minChainSize = 2, float chainDelay = 0.1f)
        {
            _tileGrid = tileGrid;
            _minChainSize = minChainSize;
            _chainDelay = chainDelay;
        }
        public void StartChain(Tile startTile)
        {
            // 1. Get the color to match
            // 2. Find all connected tiles
            // 3. Validate minimum chain size
            // 4. Execute chain if valid

            List<Tile> connectedTiles = FindConnectedTiles(startTile);
            if (connectedTiles.Count >= _minChainSize)
            {
                ExecuteChain(connectedTiles);
            }
        }

        private List<Tile> FindConnectedTiles(Tile startTile)
        {
            List<Tile> connectedTiles = new List<Tile>();
            HashSet<Tile> visited = new HashSet<Tile>();
            Queue<Tile> queue = new Queue<Tile>();

            TileColor targetColor = startTile.TileColor;
            // Start BFS from the clicked tile
            queue.Enqueue(startTile);
            visited.Add(startTile);

            while (queue.Count > 0)
            {
                Tile currentTile = queue.Dequeue();
                connectedTiles.Add(currentTile);

                // Get grid position for neighbor lookup
                Vector2 tilePos = currentTile.GetTilePos();
                var neighbors = _tileGrid.GetNeighbors((int)tilePos.x, (int)tilePos.y);

                foreach (Tile neighbor in neighbors)
                {
                    // Check: same color, active, not visited
                    if (neighbor.TileColor == targetColor &&
                        neighbor.IsActive &&
                        !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            Debug.Log($"Found chain of {connectedTiles.Count} tiles with color {targetColor}");
            return connectedTiles;
        }
        private void ExecuteChain(List<Tile> chainTiles)
        {
            // 1. Calculate and award score
            ScoreManager.AddChainScore(chainTiles.Count);

            // 2. Deactivate tiles
            foreach (var tile in chainTiles)
            {
                tile.SetActive(false);
            }

            // 3. Play effects
            PlayChainEffects(chainTiles);

            // 4. Check for game over conditions
            // TODO: Add game over logic if needed
        }

        private void PlayChainEffects(List<Tile> tiles)
        {

        }
    }
}
