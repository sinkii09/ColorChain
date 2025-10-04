using UnityEngine;

namespace ColorChain.UI
{
    public abstract class BaseUIPanel : MonoBehaviour
    {
        protected bool isInitialized = false;

        public virtual void Initialize()
        {
            if (isInitialized) return;

            OnInitialize();
            SubscribeToEvents();
            isInitialized = true;
        }

        public virtual void Cleanup()
        {
            UnsubscribeFromEvents();
            OnCleanup();
            isInitialized = false;
        }

        protected virtual void OnDestroy()
        {
            if (isInitialized)
            {
                Cleanup();
            }
        }

        protected abstract void OnInitialize();
        protected abstract void SubscribeToEvents();
        protected abstract void UnsubscribeFromEvents();
        protected virtual void OnCleanup() { }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}