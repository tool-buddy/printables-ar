namespace ToolBuddy.PrintablesAR.Application
{
    public partial class ApplicationStateMachine
    {
        public enum ApplicationState
        {
            Initializing,
            AwaitingModel,
            LoadingModel,
            SpawningModel,
            ManipulatingModel,
            ShowingHelp,
            ShowingError,
            Quitting
        }
    }
}