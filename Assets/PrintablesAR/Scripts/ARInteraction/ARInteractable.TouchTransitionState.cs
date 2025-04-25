using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public partial class ARInteractable
    {
        /// <summary>
        ///     The state of touch inputs when transitioning between states of <see cref="ARInteractibleStateMachine" />.
        /// </summary>
        private class TouchTransitionState
        {
            /// <summary>
            ///     The first finger pressed at the moment of the transition
            /// </summary>
            [CanBeNull]
            public Finger FirstPressedFinger { get; private set; }

            /// <summary>
            ///     The second finger pressed at the moment of the transition
            /// </summary>

            [CanBeNull]
            public Finger SecondPressedFinger { get; private set; }

            /// <summary>
            ///     The position of <see cref="FirstPressedFinger" /> at the moment of the transition
            /// </summary>

            public Vector2? FirstPressedFingerPosition { get; private set; }

            /// <summary>
            ///     The position of <see cref="SecondPressedFinger" /> at the moment of the transition
            /// </summary>

            public Vector2? SecondPressedFingerPosition { get; private set; }

            /// <summary>
            ///     The position of the finger that triggered the transition. This can be a finger that is not pressed, for example
            ///     when there is a transition due to a finger up event.
            /// </summary>
            public Vector2? TransitionTriggeringFingerPosition { get; private set; }

            public void Set(
                [CanBeNull] Finger firstPressedFinger,
                [CanBeNull] Finger secondPressedFinger,
                Vector2? transitionTriggeringFingerPosition)
            {
                FirstPressedFinger = firstPressedFinger;
                FirstPressedFingerPosition = firstPressedFinger?.screenPosition;
                SecondPressedFinger = secondPressedFinger;
                SecondPressedFingerPosition = secondPressedFinger?.screenPosition;
                TransitionTriggeringFingerPosition = transitionTriggeringFingerPosition;
            }
        }
    }
}