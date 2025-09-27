using UnityEngine;

namespace ColorChain.Core
{
    public class GameInitializer : MonoBehaviour
    {
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
        }

        private void InitializeGame()
        {
            GameStateManager.Initialize();
            ScoreManager.Initialize();
        }
    }
}