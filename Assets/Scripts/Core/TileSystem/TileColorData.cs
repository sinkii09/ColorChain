using UnityEngine;

namespace ColorChain.Core
{
    [CreateAssetMenu(fileName = "TileColorData", menuName = "Color Chain/Tile Color Data")]
    public class TileColorData : ScriptableObject
    {
        [System.Serializable]
        public class ColorSpriteMapping
        {
            [Header("Color Configuration")]
            public TileColor tileColor;

            [Header("Visual Assets")]
            public Sprite tileSprite;
            public Color overlayColor = Color.white; // Optional tint for the sprite

            [Header("Optional Effects")]
            public Sprite highlightSprite; // For hover/selection effects
            public Sprite activationSprite; // For chain activation effects
        }

        [Header("Tile Color Mappings")]
        [SerializeField] private ColorSpriteMapping[] colorMappings;

        public ColorSpriteMapping GetMappingForColor(TileColor color)
        {
            foreach (var mapping in colorMappings)
            {
                if (mapping.tileColor == color)
                {
                    return mapping;
                }
            }

            Debug.LogWarning($"No sprite mapping found for color: {color}");
            return null;
        }

        public Sprite GetSpriteForColor(TileColor color)
        {
            var mapping = GetMappingForColor(color);
            return mapping?.tileSprite;
        }

        public Color GetOverlayColorForColor(TileColor color)
        {
            var mapping = GetMappingForColor(color);
            return mapping?.overlayColor ?? Color.white;
        }

        public Sprite GetHighlightSpriteForColor(TileColor color)
        {
            var mapping = GetMappingForColor(color);
            return mapping?.highlightSprite;
        }

        public Sprite GetActivationSpriteForColor(TileColor color)
        {
            var mapping = GetMappingForColor(color);
            return mapping?.activationSprite;
        }

        // Validation in the editor
        private void OnValidate()
        {
            // Check for duplicate color mappings
            for (int i = 0; i < colorMappings.Length; i++)
            {
                for (int j = i + 1; j < colorMappings.Length; j++)
                {
                    if (colorMappings[i].tileColor == colorMappings[j].tileColor)
                    {
                        Debug.LogWarning($"Duplicate mapping found for color: {colorMappings[i].tileColor}");
                    }
                }
            }
        }
    }
}