namespace ToolBuddy.PrintablesAR.Application
{
    public partial class ApplicationStateMachine
    {
        public enum Trigger
        {
            ApplicationInitialized,
            ModelLoadingStarted,
            ModelLoadingSuccess,
            ModelLoadingError,
            ModelSpawned,
            CloseButtonPressed,
            HelpButtonPressed,
            BackButtonPressed
        }
    }
}