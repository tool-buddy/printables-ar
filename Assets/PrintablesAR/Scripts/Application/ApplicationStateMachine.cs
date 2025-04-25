namespace ToolBuddy.PrintablesAR.Application
{
    /// <summary>
    /// State machine that manages the lifecycle of the application.
    /// </summary>
    public partial class
        ApplicationStateMachine : StateMachineBase<ApplicationStateMachine.ApplicationState, ApplicationStateMachine.Trigger>
    {
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
                );

            Configure(ApplicationState.LoadingModel)
                .Permit(
                    Trigger.ModelLoadingSuccess,
                    ApplicationState.SpawningModel
                )
                .Permit(
                    Trigger.ModelLoadingError,
                    ApplicationState.LoadingError
                );

            Configure(ApplicationState.LoadingError)
                .Permit(
                    Trigger.Reset,
                    ApplicationState.AwaitingModel
                );

            Configure(ApplicationState.SpawningModel)
                .Permit(
                    Trigger.ModelSpawned,
                    ApplicationState.ManipulatingModel
                ); 
            
            Configure(ApplicationState.ManipulatingModel)
                .Permit(
                    Trigger.ModelLoadingStarted,
                    ApplicationState.LoadingModel
                );
        }
    }
}