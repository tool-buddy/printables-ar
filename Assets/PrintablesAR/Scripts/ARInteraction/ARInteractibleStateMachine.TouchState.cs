namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public partial class ARInteractibleStateMachine
    {
        /// <summary>
        /// Represents the various states of touch interaction.
        /// </summary>
        public enum TouchState
        {
            /// <summary>
            /// The state when there is no touch interaction.
            /// </summary>
            Idle,

            /// <summary>
            /// The state when an interaction started but was not defined yet (potential tap/drag/pinch).
            /// </summary>
            DetectingInteraction,
            
            /// <summary>
            /// The state when the object is being placed in the environment.
            /// </summary>
            Placing,
            
            /// <summary>
            /// A superstate for all dragging-related activities. Dragging leads to the object's rotation.
            /// </summary>
            Dragging,
            
            /// <summary>
            /// The initial state of a drag, before the direction is determined.
            /// </summary>
            UndefinedDragging,
            
            /// <summary>
            /// The state for dragging along the X-axis.
            /// </summary>
            XDragging,
            
            /// <summary>
            /// The state for dragging along the Y-axis.
            /// </summary>
            YDragging,
            
            /// <summary>
            /// The state for dragging freely in both X and Y directions.
            /// </summary>
            XYDragging,
            
            /// <summary>
            /// The state when two fingers are used to scale the object.
            /// </summary>
            Pinching
        }
    }
}
