# UI Layout Guide for GameplayHUD

## Recommended Prefab Structure

```
Canvas (Screen Space - Overlay)
│
├── GameplayHUD
│   │
│   ├── TopBar (Anchor: Top, Stretch Width)
│   │   ├── ScoreContainer (HorizontalLayoutGroup, Left Alignment)
│   │   │   └── ScoreUIPanel
│   │   │       ├── ScoreText (Large)
│   │   │       └── HighScoreText (Small)
│   │   │
│   │   └── TimerContainer (Center Alignment)
│   │       └── TimerUIPanel
│   │           ├── TimerIcon
│   │           ├── TimerFillBar (Radial or Horizontal)
│   │           └── TimerText
│   │
│   └── BottomBar (Anchor: Bottom, Stretch Width)
│       └── PowerUpContainer (Center Alignment)
│           └── PowerUpUIPanel
│               ├── PowerBarSlider
│               ├── PowerBarGlow
│               ├── PowerUpIcon
│               └── MultiplierIndicator
```

## Anchor Presets Configuration

### TopBar
- **Anchor**: Top Stretch
- **Pivot**: (0.5, 1)
- **Position**: (0, -50) from top
- **Height**: 100-150 pixels
- **Padding**: 20px horizontal

### ScoreContainer (Inside TopBar)
- **Alignment**: Left
- **Position**: Left side with 30px padding
- **Size**: 200x100

### TimerContainer (Inside TopBar)
- **Alignment**: Center
- **Position**: Center of TopBar
- **Size**: 150x100

### BottomBar
- **Anchor**: Bottom Stretch
- **Pivot**: (0.5, 0)
- **Position**: (0, 50) from bottom
- **Height**: 100-120 pixels

### PowerUpContainer (Inside BottomBar)
- **Alignment**: Center
- **Position**: Center of BottomBar
- **Size**: 300x100

## Visual Separation Tips

1. **Use Consistent Spacing**
   - Maintain 20-30px padding from screen edges
   - Keep 10-15px spacing between UI elements

2. **Visual Hierarchy**
   - Score: Top-left (most frequently checked)
   - Timer: Top-center (important, needs attention)
   - PowerUp: Bottom-center (secondary information)

3. **Background Panels**
   - Add semi-transparent background panels to group related UI
   - Use subtle borders or gradients to separate sections

4. **Responsive Scaling**
   - Use Canvas Scaler with "Scale With Screen Size"
   - Reference Resolution: 1920x1080
   - Screen Match Mode: 0.5

## Color Scheme Suggestions

- **Score Panel**: Blue/Cyan tones
- **Timer Panel**: Green → Yellow → Red gradient
- **PowerUp Panel**: Purple/Gold tones

## Implementation in Unity

1. Create empty GameObjects for each container
2. Add LayoutGroup components where needed
3. Set proper anchors for each element
4. Use ContentSizeFitter for dynamic sizing
5. Test on different aspect ratios (16:9, 16:10, 21:9)

## Safe Areas for Mobile

If targeting mobile, ensure UI elements are within safe areas:
- Top Safe Area: 50-100px (for notches)
- Bottom Safe Area: 30-50px (for gesture bars)