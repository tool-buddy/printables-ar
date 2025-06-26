using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Stateless;
using ToolBuddy.PrintablesAR.Sound;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.EnhancedTouch;
using static ToolBuddy.PrintablesAR.ARInteraction.ARInteractibleStateMachine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    /// <summary>
    /// A component that makes a GameObject AR interactable object.
    /// </summary>
    public partial class ARInteractable : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets the <see cref="Raycaster"/> used for hit testing against the AR environment.
        /// </summary>
        [CanBeNull]
        public Raycaster Raycaster { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AudioPlayer"/> for providing audio feedback.
        /// </summary>
        [CanBeNull]
        public AudioPlayer AudioPlayer { get; set; }


        #region State

        private readonly ARInteractibleStateMachine _stateMachine = new ARInteractibleStateMachine();

        private Quaternion _preDragRotation;
        private Vector2 _currentDragStartPosition;
        private Vector3 _prePinchScale;
        private readonly TouchTransitionState _touchTransitionState = new TouchTransitionState();

        /// <summary>
        /// Represents the maximal distance in centimeters that a finger can move during a linear drag operation before it is considered a bidirectional drag.
        /// </summary>
        private const float _linearDragMargin = 0.7f;

        /// <summary>
        /// Represents the minimum drag distance in centimeters before a drag operation is considered valid.
        /// </summary>
        private const float _dragMovementThreshold = 0.1f;

        private Vector2 DragVector
        {
            get
            {
                if (_stateMachine.IsInState(TouchState.Dragging) == false)
                    throw new InvalidOperationException(
                        $"DragVector can not be called when outside of the {nameof(TouchState.Dragging)} state"
                    );

                return _touchTransitionState.FirstPressedFinger.screenPosition - _currentDragStartPosition;
            }
        }

        /// <summary>
        /// Gets the current touch interaction state of the object.
        /// </summary>
        public TouchState State => _stateMachine.State;

        #endregion


        #region Unity callbacks

        private void Awake()
        {
            ConfigureStateMachine();
            new ARInteractibleFeedbackProvider(
                gameObject,
                _stateMachine,
                AudioPlayer
            );
        }

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

                case TouchState.XDragging:
                    TransformManipulator.Rotate(
                        new Vector2(
                            DragVector.x,
                            0
                        ),
                        transform,
                        _preDragRotation,
                        Camera.main.transform.position
                    );
                    break;
                case TouchState.YDragging:
                    TransformManipulator.Rotate(
                        new Vector2(
                            0,
                            DragVector.y
                        ),
                        transform,
                        _preDragRotation,
                        Camera.main.transform.position
                    );
                    break;
                case TouchState.XYDragging:
                    TransformManipulator.Rotate(
                        DragVector,
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
            }
        }

        #endregion

        #region StateMachine callbacks

        private void ConfigureStateMachine()
        {
            _stateMachine.Configure(TouchState.Pinching)
                .OnEntry(() => _prePinchScale = transform.localScale);

            _stateMachine.Configure(TouchState.Dragging)
                .OnEntry(
                    () =>
                    {
                        _preDragRotation = transform.rotation;
                        _currentDragStartPosition = _touchTransitionState.FirstPressedFingerPosition.Value;
                    }
                );

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
            if (_touchTransitionState.FirstPressedFinger == null)
                return;
            if (HasMovedBeyondDragThreshold() == false)
                return;

            switch (_stateMachine.State)
            {
                case TouchState.DetectingInteraction:
                    _stateMachine.FireFingerTrigger(
                        _stateMachine.FingerMoveTrigger,
                        finger
                    );
                    break;
                case TouchState.UndefinedDragging:
                    _stateMachine.Fire(
                        Mathf.Abs(DragVector.x) >= Mathf.Abs(DragVector.y)
                            ? Trigger.XDragDetermined
                            : Trigger.YDragDetermined
                    );
                    break;
                case TouchState.XDragging:
                    if (Mathf.Abs(DragVector.y) >= ScreenCmToPixels(_linearDragMargin))
                        _stateMachine.Fire(Trigger.BidirectionalDragUnlocked);
                    break;
                case TouchState.YDragging:
                    if (Mathf.Abs(DragVector.x) >= ScreenCmToPixels(_linearDragMargin))
                        _stateMachine.Fire(Trigger.BidirectionalDragUnlocked);
                    break;
            }
        }

        private void ProcessFingerUp(
            Finger finger) =>
            _stateMachine.FireFingerTrigger(
                _stateMachine.FingerUpTrigger,
                finger
            );

        #endregion

        private bool HasMovedBeyondDragThreshold()
        {
            Finger firstPressedFinger = _touchTransitionState.FirstPressedFinger;
            Assert.IsTrue(
                firstPressedFinger != null,
                nameof(firstPressedFinger) + " != null"
            );

            Touch firstTouch = firstPressedFinger.currentTouch;
            float pixelDistance = Vector2.Distance(
                firstTouch.screenPosition,
                firstTouch.startScreenPosition
            );

            return pixelDistance > ScreenCmToPixels(_dragMovementThreshold);
        }

        private static float ScreenCmToPixels(
            float distanceInCm)
        {
            float distanceInInches = distanceInCm / 2.54f;
            return distanceInInches * GetScreenDpi();
        }

        private static float GetScreenDpi()
        {
            const float fallbackDpi = 360f;

            float dpi = Screen.dpi;

            if (dpi == 0)
            {
                Debug.LogError($"Invalid value for screen DPI {dpi}");
                dpi = fallbackDpi;
            }

            return dpi;
        }
    }
}