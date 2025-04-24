namespace ToolBuddy.PrintablesAR.Application
{
    /// <summary>
    /// State machine that manages the lifecycle of the application.
    /// </summary>
    public partial class
        ApplicationStateMachine : StateMachineBase<ApplicationStateMachine.ApplicationState, ApplicationStateMachine.Trigger>
    {
        public TriggerWithParameters<string> ModelLoadingErrorTrigger { get; private set; }

        public ApplicationStateMachine() : base(ApplicationState.Initialization) { }

        protected override void SetTriggerParameters() =>
            ModelLoadingErrorTrigger = SetTriggerParameters<string>(Trigger.ModelLoadingError);

        protected override void Configure()
        {
            base.Configure();

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

            Configure(ApplicationState.ModelLoading)
                .Permit(
                    Trigger.ModelLoadingSuccess,
                    ApplicationState.ModelSpawn
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

            Configure(ApplicationState.ModelSpawn)
                .Permit(
                    Trigger.ModelSpawned,
                    ApplicationState.ModelManipulation
                ); 
            
            Configure(ApplicationState.ModelManipulation)
                .Permit(
                    Trigger.ModelLoadingStarted,
                    ApplicationState.ModelLoading
                );
        }
    }
}