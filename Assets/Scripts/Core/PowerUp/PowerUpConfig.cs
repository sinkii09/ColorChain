using UnityEngine;

namespace ColorChain.Core
{
    [System.Serializable]
    public class PowerUpConfig
    {
        [Header("Bar Fill Amounts")]
        public float smallChainFill = 10f;   // 2-3 tiles
        public float mediumChainFill = 25f;  // 4-5 tiles
        public float largeChainFill = 40f;   // 6-7 tiles
        public float hugeChainFill = 60f;    // 8+ tiles

        [Header("Power-Up Settings")]
        public float timeBonusAmount = 10f;
        public float multiplierDuration = 5f;
        public float multiplierAmount = 2f;

        [Header("Randomization Weights")]
        public int colorConverterWeight = 30;
        public int timeBonusWeight = 40;
        public int scoreMultiplierWeight = 30;
    }
}
