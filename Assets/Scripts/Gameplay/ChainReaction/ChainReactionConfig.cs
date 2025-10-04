using System;
using UnityEngine;

namespace ColorChain.GamePlay
{
    [Serializable]
    public class ChainReactionConfig
    {
        [field: SerializeField]
        public int MinChainSize = 2;

        [field: SerializeField]
        public float ChainDelay = 0.1f;
    }
}
