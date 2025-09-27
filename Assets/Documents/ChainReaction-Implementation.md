# ChainReaction System Implementation Plan

## Core Algorithm
**Flood Fill / Recursive Propagation**:
1. Player clicks a tile
2. Find all connected tiles of the same color
3. Remove/activate all tiles in the chain
4. Calculate score based on chain size
5. Trigger visual/audio effects

## Implementation Steps

### 1. Create ChainReaction.cs (Gameplay folder)
**Key Methods**:
- `StartChain(Tile startTile)` - Entry point when tile clicked
- `FindConnectedTiles(Tile tile, TileColor color, HashSet<Tile> visited)` - Recursive search
- `ExecuteChain(List<Tile> chainTiles)` - Remove tiles and score
- `PlayChainEffects(List<Tile> tiles)` - Visual feedback

### 2. Chain Detection Logic
**Recursive Algorithm**:
```csharp
FindConnectedTiles(currentTile, targetColor, visitedSet):
  - Add currentTile to visited
  - Add currentTile to chain
  - For each neighbor of currentTile:
    - If neighbor has same color AND not visited:
      - Recursively call FindConnectedTiles(neighbor)
```

### 3. Integration Points
- **Tile.OnTileClicked()** → Call ChainReaction.StartChain()
- **TileGrid.GetNeighbors()** → Already implemented for neighbor detection
- **ScoreManager** → Award points based on chain length
- **GameManager** → Check win/lose conditions

### 4. Scoring System
- Base points: 100 per tile
- Multiplier: 1.5x for each additional tile in chain
- Minimum chain size: 2 tiles (prevent single-tile clicks)

## Detailed Implementation

### ChainReaction.cs Structure
```csharp
public class ChainReaction : MonoBehaviour
{
    [Header("Chain Settings")]
    [SerializeField] private int minChainSize = 2;
    [SerializeField] private float chainDelay = 0.1f;

    private TileGrid tileGrid;
    private ScoreManager scoreManager;

    public void StartChain(Tile startTile)
    {
        // 1. Get the color to match
        // 2. Find all connected tiles
        // 3. Validate minimum chain size
        // 4. Execute chain if valid
    }

    private List<Tile> FindConnectedTiles(Tile startTile, TileColor targetColor)
    {
        // Flood fill algorithm implementation
    }

    private void ExecuteChain(List<Tile> chainTiles)
    {
        // 1. Deactivate tiles
        // 2. Play effects
        // 3. Calculate and award score
        // 4. Check for game over conditions
    }
}
```

### Integration with Existing System

#### Tile.cs Modification
```csharp
private void OnTileClicked()
{
    // Find ChainReaction component in scene
    ChainReaction chainReaction = FindObjectOfType<ChainReaction>();
    if (chainReaction != null)
    {
        chainReaction.StartChain(this);
    }
}
```

#### TileGrid.cs Usage
```csharp
// Already implemented - use for neighbor detection
public List<Tile> GetNeighbors(int x, int y)
```

### Score Calculation Formula
```
Chain Score = Base Points × Chain Size × Multiplier^(Chain Size - 1)

Where:
- Base Points = 100
- Chain Size = Number of tiles in chain
- Multiplier = 1.5

Examples:
- 2 tiles: 100 × 2 × 1.5^1 = 300 points
- 3 tiles: 100 × 3 × 1.5^2 = 675 points
- 4 tiles: 100 × 4 × 1.5^3 = 1350 points
```

### Visual Effects Sequence
1. **Highlight Chain** - Show all tiles that will be affected
2. **Chain Activation** - Animate tiles disappearing in sequence
3. **Score Display** - Show points earned from chain
4. **Grid Update** - Refresh grid state

### Audio Effects
- **Chain Start** - Initial tile click sound
- **Chain Propagation** - Sound for each tile in sequence
- **Chain Complete** - Satisfying completion sound
- **Score Award** - Points earning sound

## Testing Checklist
- [ ] Single tile clicks are ignored (< minimum chain size)
- [ ] Valid chains are detected correctly
- [ ] Scoring calculation is accurate
- [ ] Visual effects play in correct sequence
- [ ] Audio feedback is responsive
- [ ] Grid state updates properly after chain
- [ ] Game over conditions are checked

## Performance Considerations
- **Recursion Depth**: Limit max chain size to prevent stack overflow
- **Object Pooling**: Reuse effect objects for smooth performance
- **Coroutines**: Use for smooth animation sequences
- **Memory**: Clear temporary collections after each chain

## Future Enhancements
- **Chain Combos**: Bonus points for consecutive chains
- **Special Patterns**: L-shape, T-shape bonus chains
- **Power-ups**: Enhanced chain effects
- **Cascading**: New tiles fall after chain removal

---

*Implementation Priority: High - Core gameplay mechanic*
*Estimated Time: 4-6 hours*
*Dependencies: Tile system, TileGrid, GameManager*