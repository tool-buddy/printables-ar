namespace ToolBuddy.PrintablesAR.Application
{
    public partial class ApplicationStateMachine
    {
        public enum ApplicationState
        {
            Initialization,
            NoModelLoaded,
            ModelLoading,
            LoadingError,
            ModelPlacement
        }
    }
}