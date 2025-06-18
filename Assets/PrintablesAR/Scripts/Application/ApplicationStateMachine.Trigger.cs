namespace ToolBuddy.PrintablesAR.Application
{
    public partial class ApplicationStateMachine
    {
        public enum Trigger
        {
            RequiredHardwareFound,
            RequiredSoftwareFound,
            ModelLoadingStarted,
            ModelLoadingSuccess,
            ModelLoadingError,
            PermissionError,
            ModelSpawned,
            CloseButtonPressed,
            HelpButtonPressed,
            BackButtonPressed
        }
    }
}