# Power-Up System Implementation Plan

## System Overview
**Type**: Fill-the-Bar with Auto-Activation
**Core Mechanic**: Players fill a power bar through chain reactions. When full, a random power-up automatically activates.

---

## 1. Power Bar Mechanics

### Bar Filling Rules
```
Chain Size    | Bar Fill Amount | Points
------------- | --------------- | -------
2-3 tiles     | +10%           | 200-300
4-5 tiles     | +25%           | 400-500
6-7 tiles     | +40%           | 600-700
8+ tiles      | +60%           | 800+
```

### Bar Properties
- **Max Capacity**: 100%
- **Visual Feedback**: Glowing/pulsing when near full
- **Reset**: After power-up activation
- **Carry Over**: Excess % carries to next bar (optional)

---

## 2. Power-Up Types

### 2.1 Color Converter 🔄
**Effect**: Converts all tiles of one color to another
**Logic**:
1. Select most populous color on board
2. Convert to second most populous color
3. Creates opportunity for massive chains
**Duration**: Instant
**Visual**: Wave effect across converted tiles

### 2.2 Time Bonus ⏰
**Effect**: Adds +10 seconds to game timer
**Logic**:
1. Add time immediately
2. Show "+10s" floating text
3. Flash timer UI
**Duration**: Instant
**Visual**: Clock animation, green flash on timer

### 2.3 Score Multiplier 💎
**Effect**: Next 5 seconds, all chains worth 2x points
**Logic**:
1. Start 5-second countdown
2. Apply 2x multiplier to all scores
3. Show multiplier indicator
**Duration**: 5 seconds
**Visual**: Golden glow on tiles, "2X" indicator

---

## 3. Auto-Activation System

### Activation Flow
```
1. Power bar reaches 100%
2. Random power-up selected
3. 0.5s delay (build anticipation)
4. Show power-up name/icon
5. Auto-activate effect
6. Reset power bar to 0%
7. Apply carry-over if any
```

### Smart Activation Logic
```csharp
// Color Converter - Best time to activate
if (boardHasGoodColorDistribution) {
    ActivateColorConverter();
}

// Time Bonus - Activate when timer < 20 seconds
if (gameTimer < 20) {
    ActivateTimeBonus();
}

// Score Multiplier - Activate when board has chain potential
if (potentialChainsAvailable > 3) {
    ActivateScoreMultiplier();
}
```

---

## 4. Implementation Structure

### Classes to Create

#### PowerUpManager.cs
```csharp
public class PowerUpManager : MonoBehaviour
{
    // Power bar management
    float currentPowerBar = 0f;
    float maxPowerBar = 100f;

    // Power-up queue (if multiple earned quickly)
    Queue<PowerUpType> pendingPowerUps;

    // Active effects tracking
    bool isMultiplierActive = false;
    float multiplierTimeRemaining = 0f;

    public void AddPowerBarProgress(int chainSize);
    private void CheckPowerBarFull();
    private IEnumerator ActivatePowerUp(PowerUpType type);
}
```

#### PowerUpEffects.cs
```csharp
public static class PowerUpEffects
{
    public static void ExecuteColorConverter(TileGrid grid);
    public static void ExecuteTimeBonus();
    public static void ExecuteScoreMultiplier(float duration);
}
```

#### PowerUpUI.cs
```csharp
public class PowerUpUI : MonoBehaviour
{
    // UI References
    Slider powerBarSlider;
    Text powerUpNameText;
    Image powerUpIcon;

    public void UpdatePowerBar(float percentage);
    public void ShowPowerUpActivation(PowerUpType type);
    public void ShowMultiplierActive(float timeRemaining);
}
```

---

## 5. Visual/Audio Feedback

### Power Bar States
| State | Visual | Audio |
|-------|--------|-------|
| 0-25% | Normal blue bar | Silent |
| 25-50% | Slight glow | Quiet hum |
| 50-75% | Pulsing glow | Building tension |
| 75-99% | Fast pulse, particles | Exciting buildup |
| 100% | Flash + burst effect | Activation sound |

### Power-Up Activation Effects
| Power-Up | Visual Effect | Sound Effect |
|----------|--------------|--------------|
| Color Converter | Tile color wave animation | Magical transformation |
| Time Bonus | Clock appears, +10s floats up | Clock chime |
| Score Multiplier | Golden aura on grid | Coin/treasure sound |

---

## 6. Balancing Parameters

### Configurable Values
```csharp
[System.Serializable]
public class PowerUpConfig
{
    [Header("Power Bar")]
    public float bar2TilesFill = 10f;
    public float bar4TilesFill = 25f;
    public float bar6TilesFill = 40f;
    public float bar8TilesFill = 60f;

    [Header("Power-Up Settings")]
    public float timeBonusSeconds = 10f;
    public float multiplierDuration = 5f;
    public float multiplierAmount = 2f;

    [Header("Weights - Randomization")]
    public int colorConverterWeight = 30;  // 30% chance
    public int timeBonusWeight = 40;       // 40% chance
    public int scoreMultiplierWeight = 30; // 30% chance
}
```

---

## 7. Integration Points

### With Existing Systems

#### ChainReaction.cs
```csharp
// After chain execution
int chainSize = chainTiles.Count;
PowerUpManager.Instance.AddPowerBarProgress(chainSize);

// Check for active multiplier
if (PowerUpManager.Instance.IsMultiplierActive)
{
    score *= 2;
}
```

#### GameStateManager.cs
```csharp
// Time bonus integration
public static void AddBonusTime(float seconds)
{
    _timer += seconds;
    _timer = Mathf.Min(_timer, MAX_TIME); // Cap at max
    OnTimerChanged?.Invoke(_timer);
}
```

#### ScoreManager.cs
```csharp
// Multiplier integration
public static int CalculateScore(int baseScore)
{
    if (PowerUpManager.Instance.IsMultiplierActive)
        return baseScore * 2;
    return baseScore;
}
```

---

## 8. Testing Checklist

### Functional Tests
- [ ] Power bar fills correctly based on chain size
- [ ] Bar resets after reaching 100%
- [ ] Power-ups activate automatically
- [ ] Color Converter changes correct tiles
- [ ] Time Bonus adds exactly 10 seconds
- [ ] Score Multiplier lasts exactly 5 seconds
- [ ] Multiple power-ups queue correctly

### Visual Tests
- [ ] Power bar animation smooth
- [ ] Activation effects play correctly
- [ ] UI updates immediately
- [ ] No visual glitches during activation

### Balance Tests
- [ ] Average 3-4 power-ups per 60-second game
- [ ] Power-ups feel rewarding but not overpowered
- [ ] Random distribution feels fair

---

## 9. Future Enhancements

### Version 2.0 Ideas
1. **Combo Power-Ups**: Activate 2 power-ups simultaneously for combo effects
2. **Power-Up Preview**: Show next power-up that will be earned
3. **Player Choice**: At 100%, choose from 2 random power-ups
4. **Rare Power-Ups**: 5% chance for super power-up
5. **Power-Up Mastery**: Track which power-ups player uses most effectively

### Monetization Options
1. **Start with Power Bar at 50%**: Paid boost
2. **Guaranteed Power-Up Type**: Watch ad to choose
3. **Double Power-Up Effect**: Premium upgrade

---

## 10. Implementation Priority

### Phase 1 (Core System)
1. Create PowerUpManager
2. Implement power bar filling
3. Add auto-activation logic

### Phase 2 (Power-Ups)
1. Implement Color Converter
2. Implement Time Bonus
3. Implement Score Multiplier

### Phase 3 (Polish)
1. Add visual effects
2. Add sound effects
3. UI animations

### Phase 4 (Testing)
1. Balance testing
2. Bug fixes
3. Performance optimization

---

## Quick Start Guide

### For Developers
1. Create `PowerUpManager.cs` and attach to GameplayManager
2. Add PowerUpUI elements to Canvas
3. Connect PowerUpManager to ChainReaction for bar filling
4. Implement each power-up effect
5. Test and balance values

### Configuration in Unity Inspector
```
PowerUpManager Component:
├── Bar Fill Settings
│   ├── Small Chain Fill: 10
│   ├── Medium Chain Fill: 25
│   ├── Large Chain Fill: 40
│   └── Huge Chain Fill: 60
├── Power-Up Settings
│   ├── Time Bonus Amount: 10
│   ├── Multiplier Duration: 5
│   └── Multiplier Amount: 2
└── UI References
    ├── Power Bar Slider
    ├── Power Up Text
    └── Effect Container
```

---

*Last Updated: [Current Date]*
*Status: Ready for Implementation*
*Estimated Time: 4-6 hours*