namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public partial class ARInteractibleStateMachine
    {
        public enum TouchState
        {
            Idle,

            /// <summary>
            /// Potential tap/drag/pinch
            /// </summary>
            DetectingInteraction,
            Placing,
            Dragging,
            UndefinedDragging,
            XDragging,
            YDragging,
            XYDragging,
            Pinching
        }
    }
}