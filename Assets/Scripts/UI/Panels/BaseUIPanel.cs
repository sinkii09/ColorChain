using UnityEngine;

namespace ColorChain.UI
{
    public abstract class BaseUIPanel : MonoBehaviour
    {
        protected bool isInitialized = false;
        protected bool isActive = false;
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
            isActive = false;
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

        public void Show()
        {
            if (!isInitialized)
            {
                Initialize();
            }

            if (isActive) return;
            isActive = true;
            OnShow();
        }

        public void Hide()
        {
            if (!isActive) return;
            isActive = false;
            OnHide();
        }

        protected virtual void OnShow()
        {
            gameObject.SetActive(true);

        }
        protected virtual void OnHide()
        {
            gameObject.SetActive(false);
        }

        public virtual float GetTransitionDuration()
        {
            return 0f; // Default: no animation
        }
    }
}