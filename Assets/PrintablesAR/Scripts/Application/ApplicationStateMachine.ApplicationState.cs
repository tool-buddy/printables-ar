namespace ToolBuddy.PrintablesAR.Application
{
    public partial class ApplicationStateMachine
    {
        /// <summary>
        /// Represents the different states of the application lifecycle.
        /// </summary>
        public enum ApplicationState
        {
            /// <summary>
            /// The state where the application checks for AR hardware compatibility.
            /// </summary>
            CheckingHardware,
            
            /// <summary>
            /// The state where the application checks for necessary AR software.
            /// </summary>
            CheckingSoftware,
            
            /// <summary>
            /// The state where the application is waiting for a 3D model to be loaded.
            /// </summary>
            AwaitingModel,
            
            /// <summary>
            /// The state where the application is actively loading a 3D model.
            /// </summary>
            LoadingModel,
            
            /// <summary>
            /// The state where the application is spawning the loaded model in the AR environment.
            /// </summary>
            SpawningModel,
            
            /// <summary>
            /// The state where the user can interact with and manipulate the spawned model.
            /// </summary>
            ManipulatingModel,
            
            /// <summary>
            /// The state where the application displays the help popup.
            /// </summary>
            ShowingHelp,
            
            /// <summary>
            /// The state where the application displays an error message.
            /// </summary>
            ShowingError,
            
            /// <summary>
            /// The state where the application is preparing to close.
            /// </summary>
            Quitting
        }
    }
}