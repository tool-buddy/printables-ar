namespace ToolBuddy.PrintablesAR.Application
{
    public partial class ApplicationStateMachine
    {
        public enum ApplicationState
        {
            CheckingHardware,
            CheckingSoftware,
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