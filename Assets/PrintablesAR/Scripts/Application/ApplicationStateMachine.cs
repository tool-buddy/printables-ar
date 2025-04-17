using Stateless;

namespace ToolBuddy.PrintablesAR.Application
{
    /// <summary>
    /// State machine that manages UI screen transitions and visibility
    /// </summary>
    public partial class
        ApplicationStateMachine : StateMachine<ApplicationStateMachine.ApplicationState, ApplicationStateMachine.Trigger>
    {
        public TriggerWithParameters<string> ModelLoadingErrorTrigger { get; }

        public ApplicationStateMachine() : base(ApplicationState.Initialization)
        {
            ModelLoadingErrorTrigger = SetTriggerParameters<string>(Trigger.ModelLoadingError);
            ConfigureStateMachine();
        }


        /// <summary>
        /// Initialize the state machine with states and transitions
        /// </summary>
        private void ConfigureStateMachine()
        {
            //todo handle model loading cancellation
            Configure(ApplicationState.Initialization)
                .Permit(
                    Trigger.ApplicationInitialized,
                    ApplicationState.NoModelLoaded
                );

            Configure(ApplicationState.NoModelLoaded)
                .Permit(
                    Trigger.ModelLoadingStarted,
                    ApplicationState.ModelLoading
                );

            //todo handle animation
            Configure(ApplicationState.ModelLoading)
                .Permit(
                    Trigger.ModelLoadingSuccess,
                    ApplicationState.ModelPlacement
                )
                .Permit(
                    Trigger.ModelLoadingError,
                    ApplicationState.LoadingError
                );

            Configure(ApplicationState.LoadingError)
                .Permit(
                    Trigger.Reset,
                    ApplicationState.NoModelLoaded
                );

            Configure(ApplicationState.ModelPlacement)
                .Permit(
                    Trigger.ModelLoadingStarted,
                    ApplicationState.ModelLoading
                );
        }
    }
}