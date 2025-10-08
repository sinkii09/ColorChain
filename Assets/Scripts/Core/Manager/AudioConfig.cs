using UnityEngine;

namespace ColorChain.Core
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Color Chain/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("SFX Clips")]
        public AudioClip tileClickSound;
        public AudioClip chainReactionSound;
        public AudioClip powerUpSound;
        public AudioClip buttonClickSound;

        [Header("Music")]
        public AudioClip menuMusic;
        public AudioClip backgroundMusic;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float defaultSFXVolume = 0.7f;
        [Range(0f, 1f)] public float defaultMusicVolume = 0.5f;

        [Header("Pool Settings")]
        [Range(3, 10)] public int audioSourcePoolSize = 5;
    }
}
