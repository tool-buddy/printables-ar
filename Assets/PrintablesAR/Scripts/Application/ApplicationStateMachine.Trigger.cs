namespace ToolBuddy.PrintablesAR.Application
{
    public partial class ApplicationStateMachine
    {
        /// <summary>
        /// Represents the events that can cause a state transition in the application's state machine.
        /// </summary>
        public enum Trigger
        {
            /// <summary>
            /// Triggered when the necessary AR hardware is detected.
            /// </summary>
            RequiredHardwareFound,
            
            /// <summary>
            /// Triggered when the necessary AR software is installed and ready.
            /// </summary>
            RequiredSoftwareFound,
            
            /// <summary>
            /// Triggered when the process of loading a 3D model begins.
            /// </summary>
            ModelLoadingStarted,
            
            /// <summary>
            /// Triggered when a 3D model has been successfully loaded.
            /// </summary>
            ModelLoadingSuccess,
            
            /// <summary>
            /// Triggered when an error occurs during model loading.
            /// </summary>
            ModelLoadingError,
            
            /// <summary>
            /// Triggered when a required permission has been denied.
            /// </summary>
            PermissionError,
            
            /// <summary>
            /// Triggered when the loaded model is successfully spawned in the AR scene.
            /// </summary>
            ModelSpawned,
            
            /// <summary>
            /// Triggered when the user presses a close button (e.g., on a popup).
            /// </summary>
            CloseButtonPressed,
            
            /// <summary>
            /// Triggered when the user requests to see the help screen.
            /// </summary>
            HelpButtonPressed,
            
            /// <summary>
            /// Triggered when the user presses the system's back button.
            /// </summary>
            BackButtonPressed
        }
    }
}