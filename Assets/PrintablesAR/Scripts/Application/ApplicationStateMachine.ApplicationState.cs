namespace ToolBuddy.PrintablesAR.Application
{
    public partial class ApplicationStateMachine
    {
        public enum ApplicationState
        {
            Initializing,
            AwaitingModel,
            LoadingModel,
            LoadingError,
            SpawningModel,
            ManipulatingModel
        }
    }
}