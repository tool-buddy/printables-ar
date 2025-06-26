using ToolBuddy.PrintablesAR.Application;
using UnityEngine.UIElements;
using static ToolBuddy.PrintablesAR.Application.ApplicationStateMachine;

namespace ToolBuddy.PrintablesAR.UI
{
    /// <summary>
    /// A class that controls the visibility and interactivity of UI elements based on the application's state.
    /// </summary>
    public class UIController
    {
        private readonly ApplicationStateMachine _stateMachine;
        private readonly MainUI _mainUI;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIController"/> class.
        /// </summary>
        /// <param name="stateMachine">The application state machine.</param>
        /// <param name="mainUI">The main UI.</param>
        public UIController(
            ApplicationStateMachine stateMachine,
            MainUI mainUI)
        {
            _stateMachine = stateMachine;
            _mainUI = mainUI;
        }

        /// <summary>
        /// Sets up the initial state of the UI and registers event listeners.
        /// </summary>
        public void Initialize()
        {
            SetInitialVisibility();
            SetInteractivity();
            ListenToStateMachine();
            //todo verify that indeed removing event listeners is not needed
        }

        #region Initialization

        private void SetInitialVisibility()
        {
            _mainUI.Hud.style.display = DisplayStyle.None;
            _mainUI.HintLayer.style.display = DisplayStyle.None;
            _mainUI.LoadingOverlay.style.display = DisplayStyle.None;
            _mainUI.ErrorOverlay.style.display = DisplayStyle.None;
            _mainUI.HelpOverlay.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Set some elements to not pickable, so that UI does not block interactions with the scene.
        /// Picking Mode is settable in the UXML, but for some reason it is ignored by Unity. So I ended up setting it up here.
        /// </summary>
        private void SetInteractivity()
        {
            _mainUI.DocumentRoot.pickingMode = PickingMode.Ignore;
            _mainUI.RootElement.pickingMode = PickingMode.Ignore;
            _mainUI.Hud.pickingMode = PickingMode.Ignore;
            _mainUI.HintLayer.pickingMode = PickingMode.Ignore;
            _mainUI.ButtonContainer.pickingMode = PickingMode.Ignore;
        }


        private void ListenToStateMachine()
        {
            _stateMachine.Configure(ApplicationState.AwaitingModel)
                .OnEntryFrom(
                    Trigger.RequiredSoftwareFound,
                    ShowHud
                );

            _stateMachine.Configure(ApplicationState.LoadingModel)
                .OnEntry(ShowLoading)
                .OnExit(HideLoading);

            _stateMachine.Configure(ApplicationState.ShowingError)
                .OnEntryFrom(
                    _stateMachine.ModelLoadingErrorTrigger,
                    SetErrorMessage
                )
                .OnEntryFrom(
                    _stateMachine.PermissionErrorTrigger,
                    SetErrorMessage
                )
                .OnEntry(ShowError)
                .OnExit(HideError);

            _stateMachine.Configure(ApplicationState.ShowingHelp)
                .OnEntry(ShowHelp)
                .OnExit(HideHelp);
        }

        #endregion

        #region Event Listeners

        private const int _transitionDuration = 100;

        private void SetErrorMessage(
            string errorMessage)
        {
            string truncatedErrorMessage;
            {
                const int maxMessageLength = 900;
                truncatedErrorMessage = errorMessage.Length > maxMessageLength
                    ? errorMessage.Substring(
                          0,
                          maxMessageLength
                      )
                      + "..."
                    : errorMessage;
            }

            _mainUI.ErrorMessageLabel.text = truncatedErrorMessage;
        }

        private void ShowHud() =>
            _mainUI.Hud.style.display = DisplayStyle.Flex;

        private void ShowLoading()
        {
            _mainUI.ShowLayer(
                _mainUI.LoadingOverlay,
                _transitionDuration
            );

            StartLoadingSpinnerRotation();
        }

        private void StartLoadingSpinnerRotation()
        {
            VisualElement spinner = _mainUI.LoadingSpinner;
            IVisualElementScheduledItem spinnerRotationScheduledItem = spinner.schedule.Execute(
                timeState =>
                {
                    float oldAngle = spinner.style.rotate.value.angle.ToDegrees();
                    float angleIncrement = 360 * (timeState.deltaTime / 1000f);
                    spinner.style.rotate = new Rotate(
                        new Angle(
                            (oldAngle + angleIncrement) % 360,
                            AngleUnit.Degree
                        )
                    );
                }
            );
            spinnerRotationScheduledItem.Until(() => _stateMachine.State != ApplicationState.LoadingModel);
        }

        private void HideLoading() =>
            _mainUI.HideLayer(
                _mainUI.LoadingOverlay,
                _transitionDuration
            );

        private void ShowError() =>
            _mainUI.ShowLayer(
                _mainUI.ErrorOverlay,
                _transitionDuration
            );

        private void HideError() =>
            _mainUI.HideLayer(
                _mainUI.ErrorOverlay,
                _transitionDuration
            );

        private void ShowHelp() =>
            _mainUI.ShowLayer(
                _mainUI.HelpOverlay,
                _transitionDuration
            );

        private void HideHelp() =>
            _mainUI.HideLayer(
                _mainUI.HelpOverlay,
                _transitionDuration
            );

        #endregion
    }
}