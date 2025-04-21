using System.Linq;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public partial class
        ARInteractibleStateMachine : StateMachineBase<ARInteractibleStateMachine.TouchState, ARInteractibleStateMachine.Trigger>
    {
        public TriggerWithParameters<Finger> FingerDownTrigger { get; private set; }

        public TriggerWithParameters<Finger> FingerMoveTrigger { get; private set; }

        public TriggerWithParameters<Finger> FingerUpTrigger { get; private set; }

        private int ActiveInProgressFingerCount =>
            Touch.activeFingers.Count(f => f.currentTouch.inProgress);


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
                    TouchState.UnknownInteraction,
                    () => ActiveInProgressFingerCount == 1
                )
                .PermitIf(
                    Trigger.FingerDown,
                    TouchState.Pinching,
                    () => ActiveInProgressFingerCount == 2
                );

            Configure(TouchState.UnknownInteraction)
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