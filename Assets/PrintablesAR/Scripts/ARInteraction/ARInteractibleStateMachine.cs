using Stateless;
using UnityEngine.InputSystem.EnhancedTouch;

namespace ToolBuddy.PrintableAR.ARInteraction
{
    public partial class ARInteractibleStateMachine : StateMachine<ARInteractibleStateMachine.TouchState, ARInteractibleStateMachine.Trigger>
    {
        public TriggerWithParameters<Finger> FingerDownTrigger { get; }

        public TriggerWithParameters<Finger> FingerMoveTrigger { get; }

        public TriggerWithParameters<Finger> FingerUpTrigger { get; }


        public ARInteractibleStateMachine() : base(TouchState.Idle)
        {
            FingerDownTrigger = SetTriggerParameters<Finger>(Trigger.FingerDown);
            FingerMoveTrigger = SetTriggerParameters<Finger>(Trigger.FingerMove);
            FingerUpTrigger = SetTriggerParameters<Finger>(Trigger.FingerUp);

            Configure(TouchState.Idle)
                .PermitIf(
                    Trigger.FingerDown,
                    TouchState.UnknownInteraction,
                    () => Touch.activeFingers.Count == 1
                )
                .PermitIf(
                    Trigger.FingerDown,
                    TouchState.Pinching,
                    () => Touch.activeFingers.Count == 2
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
                    () => Touch.activeFingers.Count == 2
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
                    () => Touch.activeFingers.Count == 0
                );

            Configure(TouchState.Pinching)
                .PermitIf(
                    Trigger.FingerUp,
                    TouchState.Idle,
                    () => Touch.activeFingers.Count == 0
                )
                .PermitIf(
                    Trigger.FingerUp,
                    TouchState.Dragging,
                    () => Touch.activeFingers.Count == 1
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