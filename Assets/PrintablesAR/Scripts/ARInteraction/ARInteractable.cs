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
    public class ARInteractable : TouchMonoBehaviour
    {
        //TODO Refactor state and settings, maybe even remove settings once you found good values

        #region State

        private ARInteractibleStateMachine _stateMachine = new ARInteractibleStateMachine();

        [CanBeNull]
        private Finger _transitionTriggeringFinger;

        private Quaternion _preDragRotation;
        private Vector3 _prePinchScale;

        #endregion

        #region Settings

        [Header("Manipulation Settings")]
        [Tooltip("How sensitive rotation is to horizontal/vertical drag.")]
        public float RotationSensitivity = 0.4f;

        [Tooltip("How sensitive scaling is to pinch gestures.")]
        public float ScaleSensitivity = 0.005f;

        [Tooltip("How many pixels a finger must move before a drag gesture is recognized (instead of a tap).")]
        public float TapMoveThreshold = 15.0f;

        #endregion

        private static Touch? FirstTouch =>
            Touch.activeTouches.Count > 0
                ? Touch.activeTouches.First()
                : null;

        private static Touch? SecondTouch =>
            Touch.activeTouches.Count > 1
                ? Touch.activeTouches[1]
                : null;


        private void Awake() =>
            ConfigureStateMachine();

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
                   > TapMoveThreshold;
        }

        private void Update()
        {
            switch (_stateMachine.State)
            {
                case TouchState.Placing:

                    Assert.IsTrue(
                        _transitionTriggeringFinger != null,
                        nameof(_transitionTriggeringFinger) + " != null"
                    );

                    bool placementSucceeded = TransformManipulator.TryPlace(
                        _transitionTriggeringFinger,
                        transform
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
                        RotationSensitivity
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
                        _prePinchScale,
                        ScaleSensitivity
                    );
                    break;
                case TouchState.Idle:
                case TouchState.UnknownInteraction:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Touch callbacks

        protected override void ProcessFingerDown(
            Finger finger) =>
            _stateMachine.FireFingerTrigger(
                _stateMachine.FingerDownTrigger,
                finger
            );

        protected override void ProcessFingerMove(
            Finger finger)
        {
            if (HasMovedBeyondThreshold())
                _stateMachine.FireFingerTrigger(
                    _stateMachine.FingerMoveTrigger,
                    finger
                );
        }

        protected override void ProcessFingerUp(
            Finger finger) =>
            _stateMachine.FireFingerTrigger(
                _stateMachine.FingerUpTrigger,
                finger
            );

        #endregion
    }
}