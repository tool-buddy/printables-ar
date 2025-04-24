using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.EnhancedTouch;
using static ToolBuddy.PrintablesAR.ARInteraction.ARInteractibleStateMachine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public class ARInteractable : MonoBehaviour
    {
        #region State

        private readonly ARInteractibleStateMachine _stateMachine = new ARInteractibleStateMachine();

        [CanBeNull]
        private Finger _transitionTriggeringFinger;

        private Quaternion _preDragRotation;
        private Vector3 _prePinchScale;

        #endregion


        private static Touch? FirstTouch =>
            Touch.activeTouches.Count > 0
                ? Touch.activeTouches.First()
                : null;

        private static Touch? SecondTouch =>
            Touch.activeTouches.Count > 1
                ? Touch.activeTouches[1]
                : null;

		public Raycaster Raycaster { get; set; }

        #region Unity callbacks

        private void Awake() =>
            ConfigureStateMachine();

        protected virtual void OnEnable() =>
            SubscribeToTouchEvents();

        protected virtual void OnDisable() =>
            UnsubscribeToTouchEvents();

        private void Update()
        {
            switch (_stateMachine.State)
            {
                case TouchState.Placing:

                    Assert.IsTrue(
                        _transitionTriggeringFinger != null,
                        nameof(_transitionTriggeringFinger) + " != null"
                    );

                    Assert.IsNotNull(
                        Raycaster,
                        nameof(Raycaster) + " != null"
                    );

                    bool placementSucceeded = TransformManipulator.TryPlace(
                        _transitionTriggeringFinger,
                        transform,
                        Camera.main.transform.position,
                        Raycaster
                    );

                    _stateMachine.FirePlacementTrigger(
                        placementSucceeded
                    );

                    break;
                case TouchState.Dragging:
                    Assert.IsTrue(
                        FirstTouch != null,
                        nameof(FirstTouch) + " != null"
                    );
                    TransformManipulator.Rotate(
                        FirstTouch.Value,
                        transform,
                        _preDragRotation,
                        Camera.main.transform.position
                    );
                    break;
                case TouchState.Pinching:
                    Assert.IsTrue(
                        FirstTouch != null,
                        nameof(FirstTouch) + " != null"
                    );
                    Assert.IsTrue(
                        SecondTouch != null,
                        nameof(SecondTouch) + " != null"
                    );
                    TransformManipulator.Scale(
                        FirstTouch.Value,
                        SecondTouch.Value,
                        transform,
                        _prePinchScale
                    );
                    break;
                case TouchState.Idle:
                case TouchState.UnknownInteraction:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region StateMachine callbacks

        private void ConfigureStateMachine()
        {
            _stateMachine.Configure(TouchState.Pinching)
                .OnEntry(() => _prePinchScale = transform.localScale);

            _stateMachine.Configure(TouchState.Dragging)
                .OnEntry(() => _preDragRotation = transform.rotation);

            _stateMachine.OnTransitioned(
                t =>
                {
                    _transitionTriggeringFinger = t.Parameters.Length > 0
                        ? t.Parameters[0] as Finger
                        : null;
                }
            );
        }

        private bool HasMovedBeyondThreshold()
        {
            Assert.IsTrue(
                FirstTouch != null,
                nameof(FirstTouch) + " != null"
            );

            return Vector2.Distance(
                       FirstTouch.Value.screenPosition,
                       FirstTouch.Value.startScreenPosition
                   )
                   > 10;
        }

        #endregion

        #region Touch callbacks

        private void SubscribeToTouchEvents()
        {
            Touch.onFingerDown += ProcessFingerDown;
            Touch.onFingerMove += ProcessFingerMove;
            Touch.onFingerUp += ProcessFingerUp;
        }

        private void UnsubscribeToTouchEvents()
        {
            Touch.onFingerDown -= ProcessFingerDown;
            Touch.onFingerMove -= ProcessFingerMove;
            Touch.onFingerUp -= ProcessFingerUp;
        }

        private void ProcessFingerDown(
            Finger finger)
        {
            //ignore UI touches only for taps, not for snaps
            if (finger.index == 0 && Raycaster.IsFingerOnUI(finger))
                return;

            _stateMachine.FireFingerTrigger(
                _stateMachine.FingerDownTrigger,
                finger
            );
        }

        private void ProcessFingerMove(
            Finger finger)
        {
            if (HasMovedBeyondThreshold() == false)
                return;

            _stateMachine.FireFingerTrigger(
                _stateMachine.FingerMoveTrigger,
                finger
            );
        }

        private void ProcessFingerUp(
            Finger finger) =>
            _stateMachine.FireFingerTrigger(
                _stateMachine.FingerUpTrigger,
                finger
            );

        #endregion
    }

}
