using UnityEngine;

namespace ColorChain.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioConfig audioConfig;
        [SerializeField] private GameObject audioPool;
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }

        private void Update()
        {
            GameStateManager.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            GameStateManager.Terminate();
            ScoreManager.Terminate();
            AudioManager.Terminate();
        }

        private void InitializeGame()
        {
            GameStateManager.Initialize();
            ScoreManager.Initialize();
            InitializeAudio();

            AudioManager.PlayMenuMusic();
        }

        private void InitializeAudio()
        {
            if (audioConfig == null)
            {
                Debug.LogWarning("AudioConfig not assigned in GameInitializer");
                return;
            }
            AudioManager.Initialize(audioConfig, audioPool.transform);
        }
    }
}