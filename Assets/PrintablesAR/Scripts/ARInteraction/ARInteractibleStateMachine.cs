using System.Linq;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    /// <summary>
    /// State machine that manages the lifecycle of an <see cref="ARInteractable"/>.
    /// </summary>
    public partial class
        ARInteractibleStateMachine : StateMachineBase<ARInteractibleStateMachine.TouchState, ARInteractibleStateMachine.Trigger>
    {
        /// <summary>
        /// A trigger that is fired when a finger touches the screen.
        /// </summary>
        public TriggerWithParameters<Finger> FingerDownTrigger { get; private set; }

        /// <summary>
        /// A trigger that is fired when a finger moves on the screen.
        /// </summary>
        public TriggerWithParameters<Finger> FingerMoveTrigger { get; private set; }

        /// <summary>
        /// A trigger that is fired when a finger is lifted from the screen.
        /// </summary>
        public TriggerWithParameters<Finger> FingerUpTrigger { get; private set; }

        private int ActiveInProgressFingerCount =>
            Touch.activeFingers.Count(f => f.currentTouch.inProgress);

        /// <summary>
        /// Initializes a new instance of the <see cref="ARInteractibleStateMachine"/> class.
        /// </summary>
        public ARInteractibleStateMachine() : base(TouchState.Idle) { }

        protected override void SetTriggerParameters()
        {
            FingerDownTrigger = SetTriggerParameters<Finger>(Trigger.FingerDown);
            FingerMoveTrigger = SetTriggerParameters<Finger>(Trigger.FingerMove);
            FingerUpTrigger = SetTriggerParameters<Finger>(Trigger.FingerUp);
        }

        protected override void Configure()
        {
            base.Configure();

            Configure(TouchState.Idle)
                .PermitIf(
                    Trigger.FingerDown,
                    TouchState.DetectingInteraction,
                    () => ActiveInProgressFingerCount == 1
                )
                .PermitIf(
                    Trigger.FingerDown,
                    TouchState.Pinching,
                    () => ActiveInProgressFingerCount == 2
                );

            Configure(TouchState.DetectingInteraction)
                .Permit(
                    Trigger.FingerMove,
                    TouchState.Dragging
                )
                .Permit(
                    Trigger.FingerUp,
                    TouchState.Placing
                )
                .PermitIf(
                    Trigger.FingerDown,
                    TouchState.Pinching,
                    () => ActiveInProgressFingerCount == 2
                );

            Configure(TouchState.Placing)
                .Permit(
                    Trigger.PlacementSuccess,
                    TouchState.Idle
                )
                .Permit(
                    Trigger.PlacementFail,
                    TouchState.Idle
                );

            Configure(TouchState.Dragging)
                .InitialTransition(TouchState.UndefinedDragging)
                .PermitIf(
                    Trigger.FingerUp,
                    TouchState.Idle,
                    () => ActiveInProgressFingerCount == 0
                )
                .PermitIf(
                    Trigger.FingerDown,
                    TouchState.Pinching,
                    () => ActiveInProgressFingerCount == 2
                );

            Configure(TouchState.UndefinedDragging)
                .SubstateOf(TouchState.Dragging)
                .Permit(
                    Trigger.XDragDetermined,
                    TouchState.XDragging
                )
                .Permit(
                    Trigger.YDragDetermined,
                    TouchState.YDragging
                );

            Configure(TouchState.XDragging)
                .SubstateOf(TouchState.Dragging)
                .Permit(
                    Trigger.BidirectionalDragUnlocked,
                    TouchState.XYDragging
                );

            Configure(TouchState.YDragging)
                .SubstateOf(TouchState.Dragging)
                .Permit(
                    Trigger.BidirectionalDragUnlocked,
                    TouchState.XYDragging
                );

            Configure(TouchState.XYDragging)
                .SubstateOf(TouchState.Dragging);

            Configure(TouchState.Pinching)
                .PermitIf(
                    Trigger.FingerUp,
                    TouchState.Idle,
                    () => ActiveInProgressFingerCount == 0
                )
                .PermitIf(
                    Trigger.FingerUp,
                    TouchState.Dragging,
                    () => ActiveInProgressFingerCount == 1
                );
        }

        //todo transform these two methods to a TryFire... returning bool.

        /// <summary>
        /// Fires a state machine trigger associated with a finger action.
        /// </summary>
        /// <param name="triggerWithParameters">The trigger to fire, with its parameters.</param>
        /// <param name="finger">The finger that initiated the action.</param>
        public void FireFingerTrigger(
            TriggerWithParameters<Finger> triggerWithParameters,
            Finger finger)
        {
            if (CanFire(triggerWithParameters.Trigger))
                Fire(
                    triggerWithParameters,
                    finger
                );
        }

        /// <summary>
        /// Fires a state machine trigger to indicate the result of a placement attempt.
        /// </summary>
        /// <param name="placementSucceeded">A value indicating whether the placement was successful.</param>
        public void FirePlacementTrigger(
            bool placementSucceeded)
        {
            Trigger trigger = placementSucceeded
                ? Trigger.PlacementSuccess
                : Trigger.PlacementFail;
            if (CanFire(trigger))
                Fire(
                    trigger
                );
        }
    }
}