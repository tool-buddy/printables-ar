using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR
{
    public abstract class TouchMonoBehaviour : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            if (!EnhancedTouchSupport.enabled) EnhancedTouchSupport.Enable();
            SubscribeToTouchEventS();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeToTouchEventS();
            if (EnhancedTouchSupport.enabled) EnhancedTouchSupport.Disable();
        }

        private void SubscribeToTouchEventS()
        {
            Touch.onFingerDown += ProcessFingerDown;
            Touch.onFingerMove += ProcessFingerMove;
            Touch.onFingerUp += ProcessFingerUp;
        }

        private void UnsubscribeToTouchEventS()
        {
            Touch.onFingerDown -= ProcessFingerDown;
            Touch.onFingerMove -= ProcessFingerMove;
            Touch.onFingerUp -= ProcessFingerUp;
        }

        protected abstract void ProcessFingerDown(
            Finger finger);

        protected abstract void ProcessFingerMove(
            Finger finger);

        protected abstract void ProcessFingerUp(
            Finger finger);
    }
}