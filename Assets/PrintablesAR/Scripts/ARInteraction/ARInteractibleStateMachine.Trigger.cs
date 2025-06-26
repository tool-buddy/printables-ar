namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public partial class ARInteractibleStateMachine
    {
        /// <summary>
        /// Represents the events that can cause a state transition in the interactible state machine.
        /// </summary>
        public enum Trigger
        {
            /// <summary>
            /// Triggered when a finger touches the screen.
            /// </summary>
            FingerDown,
            
            /// <summary>
            /// Triggered when a finger moves on the screen.
            /// </summary>
            FingerMove,
            
            /// <summary>
            /// Triggered when a finger is lifted from the screen.
            /// </summary>
            FingerUp,
            
            /// <summary>
            /// Triggered when the object is successfully placed.
            /// </summary>
            PlacementSuccess,
            
            /// <summary>
            /// Triggered when the object placement fails, for example when trying to place outside the an AR plane.
            /// </summary>
            PlacementFail,
            
            /// <summary>
            /// Triggered when a drag is determined to be primarily along the X-axis.
            /// </summary>
            XDragDetermined,
            
            /// <summary>
            /// Triggered when a drag is determined to be primarily along the Y-axis.
            /// </summary>
            YDragDetermined,
            
            /// <summary>
            /// Triggered when a drag gesture unlocks dragging in both axes.
            /// </summary>
            BidirectionalDragUnlocked
        }
    }
}
