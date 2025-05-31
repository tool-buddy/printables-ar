namespace ToolBuddy.PrintablesAR.Application
{
    /// <summary>
    /// State machine that manages the lifecycle of the application.
    /// </summary>
    public partial class
        ApplicationStateMachine : StateMachineBase<ApplicationStateMachine.ApplicationState, ApplicationStateMachine.Trigger>
    {
        /// <summary>
        /// Represents the application state prior to entering the <see cref="ApplicationState.ShowingHelp"/> state.
        /// </summary>  
        private ApplicationState _preHelpState;

        public TriggerWithParameters<string> ModelLoadingErrorTrigger { get; private set; }

        public ApplicationStateMachine() : base(ApplicationState.Initializing) { }

        protected override void SetTriggerParameters() =>
            ModelLoadingErrorTrigger = SetTriggerParameters<string>(Trigger.ModelLoadingError);

        protected override void Configure()
        {
            base.Configure();

            //todo handle model loading cancellation
            Configure(ApplicationState.Initializing)
                .Permit(
                    Trigger.ApplicationInitialized,
                    ApplicationState.AwaitingModel
                );

            Configure(ApplicationState.AwaitingModel)
                .Permit(
                    Trigger.ModelLoadingStarted,
                    ApplicationState.LoadingModel
                )
                .Permit(
                    Trigger.HelpButtonPressed,
                    ApplicationState.ShowingHelp
                )
                .Permit(
                    Trigger.BackButtonPressed,
                    ApplicationState.Quitting
                );

            Configure(ApplicationState.LoadingModel)
                .Permit(
                    Trigger.ModelLoadingSuccess,
                    ApplicationState.SpawningModel
                )
                .Permit(
                    Trigger.ModelLoadingError,
                    ApplicationState.ShowingError
                )
                .Permit(
                    Trigger.BackButtonPressed,
                    ApplicationState.Quitting
                );

            Configure(ApplicationState.SpawningModel)
                .Permit(
                    Trigger.ModelSpawned,
                    ApplicationState.ManipulatingModel
                )
                .Permit(
                    Trigger.ModelLoadingStarted,
                    ApplicationState.LoadingModel
                )
                .Permit(
                    Trigger.HelpButtonPressed,
                    ApplicationState.ShowingHelp
                )
                .Permit(
                    Trigger.BackButtonPressed,
                    ApplicationState.Quitting
                );

            Configure(ApplicationState.ManipulatingModel)
                .Permit(
                    Trigger.ModelLoadingStarted,
                    ApplicationState.LoadingModel
                )
                .Permit(
                    Trigger.HelpButtonPressed,
                    ApplicationState.ShowingHelp
                )
                .Permit(
                    Trigger.BackButtonPressed,
                    ApplicationState.Quitting
                );

            Configure(ApplicationState.ShowingError)
                .Permit(
                    Trigger.CloseButtonPressed,
                    ApplicationState.AwaitingModel
                )
                .Permit(
                    Trigger.BackButtonPressed,
                    ApplicationState.AwaitingModel
                );

            Configure(ApplicationState.ShowingHelp)
                .OnEntry(transition => _preHelpState = transition.Source)
                .PermitDynamic(
                    Trigger.CloseButtonPressed,
                    () => _preHelpState
                )
                .PermitDynamic(
                    Trigger.BackButtonPressed,
                    () => _preHelpState
                );
        }
    }
}