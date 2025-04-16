using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Stateless;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintableAR
{
    //todo rework documentation
    /// <summary>
    /// Allows manipulating an object in AR using a state machine:
    /// - Tap to initiate placement.
    /// - Places the object on a detected plane with a neutral orientation (perpendicular to the plane, facing camera).
    /// - Single finger horizontal drag rotates around local Y (yaw).
    /// - Single finger vertical drag rotates around local X (pitch).
    /// - Two finger pinch scales the object.
    /// Rotation and scale are stored relative to the neutral transform set during placement.
    /// Requires the new Input System, AR Foundation, and Stateless library.
    /// </summary>
    public class ARObjectManipulator : TouchMonoBehaviour
    {
        //todo subdivide in multiple classes
        private enum State
        {
            Idle,

            /// <summary>
            /// Potential tap/drag/pinch
            /// </summary>
            UnknownInteraction,
            Placing,
            Dragging,
            Pinching
        }

        private enum Trigger
        {
            FingerDown,
            FingerMove,
            FingerUp,
            PlacementSuccess,
            PlacementFail
        }

        private StateMachine<State, Trigger>.TriggerWithParameters<Finger> _fingerDownTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<Finger> _fingerMoveTrigger;
        private StateMachine<State, Trigger>.TriggerWithParameters<Finger> _fingerUpTrigger;

        private StateMachine<State, Trigger> _stateMachine;

        private ARRaycastManager _arRaycastManager;

        //todo rename to hits cache?
        private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        private Quaternion _preDragRotation;
        private Vector3 _prePinchScale;

        [Header("Manipulation Settings")]
        [Tooltip("How sensitive rotation is to horizontal/vertical drag.")]
        public float RotationSensitivity = 0.4f;

        [Tooltip("How sensitive scaling is to pinch gestures.")]
        public float ScaleSensitivity = 0.005f;

        [Tooltip("How many pixels a finger must move before a drag gesture is recognized (instead of a tap).")]
        public float TapMoveThreshold = 15.0f;

        [CanBeNull]
        private object _transitionTriggeringFinger;


        private static Touch? FirstTouch =>
            Touch.activeTouches.Count > 0
                ? Touch.activeTouches.First()
                : null;

        private static Touch? SecondTouch =>
            Touch.activeTouches.Count > 1
                ? Touch.activeTouches[1]
                : null;


        #region Lifetime Callbacks

        private void Awake()
        {
            _arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
            SetupStateMachine();
        }

        #endregion

        #region State Machine Setup

        private void SetupStateMachine()
        {
            _stateMachine = new StateMachine<State, Trigger>(State.Idle);

            _fingerDownTrigger = _stateMachine.SetTriggerParameters<Finger>(Trigger.FingerDown);
            _fingerMoveTrigger = _stateMachine.SetTriggerParameters<Finger>(Trigger.FingerMove);
            _fingerUpTrigger = _stateMachine.SetTriggerParameters<Finger>(Trigger.FingerUp);

            _stateMachine.Configure(State.Idle)
                .PermitIf(
                    Trigger.FingerDown,
                    State.UnknownInteraction,
                    () => Touch.activeFingers.Count == 1
                )
                .PermitIf(
                    Trigger.FingerDown,
                    State.Pinching,
                    () => Touch.activeFingers.Count == 2
                );

            _stateMachine.Configure(State.UnknownInteraction)
                .PermitIf(
                    Trigger.FingerMove,
                    State.Dragging,
                    HasMovedBeyondThreshold
                )
                .Permit(
                    Trigger.FingerUp,
                    State.Placing
                )
                .PermitIf(
                    Trigger.FingerDown,
                    State.Pinching,
                    () => Touch.activeFingers.Count == 2
                );

            _stateMachine.Configure(State.Placing)
                .Permit(
                    Trigger.PlacementSuccess,
                    State.Idle
                )
                .Permit(
                    Trigger.PlacementFail,
                    State.Idle
                );

            _stateMachine.Configure(State.Dragging)
                .PermitIf(
                    Trigger.FingerUp,
                    State.Idle,
                    () => Touch.activeFingers.Count == 0
                );

            _stateMachine.Configure(State.Pinching)
                .OnEntry(() => _prePinchScale = transform.localScale)
                .PermitIf(
                    Trigger.FingerUp,
                    State.Idle,
                    () => Touch.activeFingers.Count == 0
                )
                .PermitIf(
                    Trigger.FingerUp,
                    State.Dragging,
                    () => Touch.activeFingers.Count == 1
                );

            _stateMachine.OnTransitioned(
                t =>
                {
                    _transitionTriggeringFinger = t.Parameters.Length > 0
                        ? t.Parameters[0]
                        : null;

                    Debug.LogWarning($"Transitioned from {t.Source} to {t.Destination}");
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

        #endregion

        #region Touch event handling

        // --- Input Processing Methods ---

        protected override void ProcessFingerDown(
            Finger finger)
        {
            TryFireFingerTrigger(
                _fingerDownTrigger,
                finger
            );
        }

        protected override void ProcessFingerMove(
            Finger finger)
        {
            TryFireFingerTrigger(
                _fingerMoveTrigger,
                finger
            );
        }

        protected override void ProcessFingerUp(
            Finger finger)
        {
            TryFireFingerTrigger(
                _fingerUpTrigger,
                finger
            );
        }

        private void TryFireFingerTrigger(
            StateMachine<State, Trigger>.TriggerWithParameters<Finger> triggerWithParameters,
            Finger finger)
        {
            if (_stateMachine.CanFire(triggerWithParameters.Trigger))
                _stateMachine.Fire(
                    triggerWithParameters,
                    finger
                );
        }

        #endregion

        #region State processing

        private void Update()
        {
            switch (_stateMachine.State)
            {
                case State.Placing:
                    Assert.IsTrue(
                        _transitionTriggeringFinger != null,
                        nameof(_transitionTriggeringFinger) + " != null"
                    );

                    bool placementSucceeded = TryPlaceObjectAt((_transitionTriggeringFinger as Finger).screenPosition);

                    _preDragRotation = transform.rotation;

                    _stateMachine.Fire(
                        placementSucceeded
                            ? Trigger.PlacementSuccess
                            : Trigger.PlacementFail
                    );

                    break;
                case State.Dragging:
                    UpdateDragging();
                    break;
                case State.Pinching:
                    UpdatePinching();
                    break;
            }
        }

        private bool TryPlaceObjectAt(
            Vector2 screenPoint)
        {
            if (!_arRaycastManager.Raycast(
                    screenPoint,
                    _hits,
                    TrackableType.PlaneWithinPolygon
                ))
                return false;

            PlaceObjectAtHit(
                transform,
                _hits[0].pose
            );

            return true;
        }

        public static void PlaceObjectAtHit(
            Transform transform,
            Pose hitPose)
        {
            transform.position = hitPose.position;
            transform.rotation = GetPlacementOrientation(hitPose);
        }

        private static Quaternion GetPlacementOrientation(
            Pose hitPose)
        {
            Vector3 cameraLookingDirection = Vector3.ProjectOnPlane(
                Camera.main.transform.position - hitPose.position,
                hitPose.up
            );

            Quaternion result;
            if (cameraLookingDirection.sqrMagnitude > 0.001f)
                result = Quaternion.LookRotation(
                    cameraLookingDirection.normalized,
                    hitPose.up
                );
            else
                //Camera directly above placement point
                result = hitPose.rotation;
            return result;
        }

        private void UpdateDragging()
        {
            Assert.IsTrue(
                FirstTouch != null,
                nameof(FirstTouch) + " != null"
            );
            Touch firstTouch = FirstTouch.Value;

            Vector2 dragDelta = firstTouch.screenPosition - firstTouch.startScreenPosition;

            float yawAmount = -dragDelta.x * RotationSensitivity;
            float pitchAmount = dragDelta.y * RotationSensitivity;

            Quaternion yawRotation = Quaternion.AngleAxis(
                yawAmount,
                transform.up
            );
            Quaternion pitchRotation = Quaternion.AngleAxis(
                pitchAmount,
                transform.right
            );

            transform.rotation = _preDragRotation * yawRotation * pitchRotation;
        }

        private void UpdatePinching()
        {
            Assert.IsTrue(
                FirstTouch != null,
                nameof(FirstTouch) + " != null"
            );
            Touch firstTouch = FirstTouch.Value;

            Assert.IsTrue(
                SecondTouch != null,
                nameof(SecondTouch) + " != null"
            );
            Touch secondTouch = SecondTouch.Value;


            float currentPinchDistance = Vector2.Distance(
                firstTouch.screenPosition,
                secondTouch.screenPosition
            );

            float initialPinchDistance = Vector2.Distance(
                firstTouch.startScreenPosition,
                secondTouch.startScreenPosition
            );

            transform.localScale = _prePinchScale * (currentPinchDistance / initialPinchDistance) * ScaleSensitivity;
        }

        #endregion
    }
}