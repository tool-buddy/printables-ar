using Stateless;
using UnityEngine.InputSystem.EnhancedTouch;

namespace ToolBuddy.PrintableAR
{
    public partial class TouchStateMachine : StateMachine<TouchStateMachine.TouchState, TouchStateMachine.Trigger>
    {
        //todo subdivide in multiple classes

        private TriggerWithParameters<Finger>
            _fingerDownTrigger;

        public TriggerWithParameters<Finger> FingerDownTrigger =>
            _fingerDownTrigger;

        private TriggerWithParameters<Finger>
            _fingerMoveTrigger;

        public TriggerWithParameters<Finger> FingerMoveTrigger =>
            _fingerMoveTrigger;

        private TriggerWithParameters<Finger>
            _fingerUpTrigger;

        public TriggerWithParameters<Finger> FingerUpTrigger =>
            _fingerUpTrigger;


        public TouchStateMachine() : base(TouchState.Idle)
        {
            _fingerDownTrigger = SetTriggerParameters<Finger>(Trigger.FingerDown);
            _fingerMoveTrigger = SetTriggerParameters<Finger>(Trigger.FingerMove);
            _fingerUpTrigger = SetTriggerParameters<Finger>(Trigger.FingerUp);

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