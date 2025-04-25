using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Stateless;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.EnhancedTouch;
using static ToolBuddy.PrintablesAR.ARInteraction.ARInteractibleStateMachine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public partial class ARInteractable : MonoBehaviour
    {
        [CanBeNull]
        public Raycaster Raycaster { get; set; }

        #region State

        private readonly ARInteractibleStateMachine _stateMachine = new ARInteractibleStateMachine();

        private Quaternion _preDragRotation;
        private Vector3 _prePinchScale;
        private readonly TouchTransitionState _touchTransitionState = new TouchTransitionState();

        #endregion


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

                    bool placementSucceeded = TransformManipulator.TryPlace(
                        _touchTransitionState.TransitionTriggeringFingerPosition.Value,
                        transform,
                        Camera.main.transform.position,
                        Raycaster
                    );

                    _stateMachine.FirePlacementTrigger(
                        placementSucceeded
                    );

                    break;
                case TouchState.Dragging:

                    TransformManipulator.Rotate(
                        _touchTransitionState.FirstPressedFinger.screenPosition
                        - _touchTransitionState.FirstPressedFingerPosition.Value,
                        transform,
                        _preDragRotation,
                        Camera.main.transform.position
                    );
                    break;
                case TouchState.Pinching:

                    TransformManipulator.Scale(
                        transform,
                        _prePinchScale,
                        _touchTransitionState.FirstPressedFinger.screenPosition,
                        _touchTransitionState.SecondPressedFinger.screenPosition,
                        _touchTransitionState.FirstPressedFingerPosition.Value,
                        _touchTransitionState.SecondPressedFingerPosition.Value
                    );
                    break;
                case TouchState.Idle:
                case TouchState.DetectingInteraction:
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
                t => UpdateTouchTransitionState(t)
            );
        }

        private void UpdateTouchTransitionState(
            StateMachine<TouchState, Trigger>.Transition transition)
        {
            List<Finger> firstTwoPressedFingers =
                Touch.activeFingers.Where(f => f.isActive && f.currentTouch.inProgress).Take(2).ToList();
            Finger firstPressedFinger = firstTwoPressedFingers.Count > 0
                ? firstTwoPressedFingers[0]
                : null;
            Finger secondPressedFinger = firstTwoPressedFingers.Count > 1
                ? firstTwoPressedFingers[1]
                : null;
            _touchTransitionState.Set(
                firstPressedFinger,
                secondPressedFinger,
                transition.Parameters.Length > 0
                    ? ((Finger)transition.Parameters[0]).screenPosition
                    : null
            );
        }

        private bool HasMovedBeyondThreshold()
        {
            Finger firstPressedFinger = _touchTransitionState.FirstPressedFinger;
            Assert.IsTrue(
                firstPressedFinger != null,
                nameof(firstPressedFinger) + " != null"
            );

            Touch firstTouch = firstPressedFinger.currentTouch;

            return Vector2.Distance(
                       firstTouch.screenPosition,
                       firstTouch.startScreenPosition
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
            if (finger.index == 0
                && Raycaster.IsFingerOnUI(
                    finger.screenPosition
                ))
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