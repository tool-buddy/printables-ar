namespace ToolBuddy.PrintableAR
{
    public partial class TouchStateMachine
    {
        public enum Trigger
        {
            FingerDown,
            FingerMove,
            FingerUp,
            PlacementSuccess,
            PlacementFail
        }
    }
}