namespace ToolBuddy.PrintableAR
{
    public partial class TouchStateMachine
    {
        public enum TouchState
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
    }
}