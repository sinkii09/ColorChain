using UnityEngine;
using ColorChain.Core;

namespace ColorChain.UI
{
    public abstract class BaseUIPanel : MonoBehaviour
    {
        [Header("Audio (Optional)")]
        [SerializeField] protected AudioClip customShowSound;
        [SerializeField] protected AudioClip customHideSound;

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

            // Play sound - custom if assigned, otherwise default
            if (customShowSound != null)
            {
                AudioManager.PlaySFX(customShowSound);
            }
            else
            {
                AudioManager.PlayPanelShow();
            }

            OnShow();
        }

        public void Hide()
        {
            if (!isActive) return;
            isActive = false;

            // Play sound - custom if assigned, otherwise default
            if (customHideSound != null)
            {
                AudioManager.PlaySFX(customHideSound);
            }
            else
            {
                AudioManager.PlayPanelHide();
            }

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