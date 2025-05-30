using System;
using ToolBuddy.PrintablesAR.Application;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static ToolBuddy.PrintablesAR.Application.ApplicationStateMachine;

namespace ToolBuddy.PrintablesAR.UI
{
    public class UIController
    {
        private readonly ApplicationStateMachine _stateMachine;
        private readonly MainUI _mainUI;

        public event Action LoadingErrorClosed;

        public UIController(
            ApplicationStateMachine stateMachine,
            MainUI mainUI)
        {
            _stateMachine = stateMachine;
            _mainUI = mainUI;
        }

        public void Initialize()
        {
            SetInitialVisibility();
            SetInteractivity();
            ListenToUI();
            ListenToStateMachine();
            //todo verify that indeed removing event listeners is not needed
        }

        public void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame == false)
                return;

            if (_mainUI.IsLayerShown(_mainUI.HelpOverlay))
                _mainUI.HideLayer(
                    _mainUI.HelpOverlay,
                    TransitionDuration
                );

            if (_mainUI.IsLayerShown(_mainUI.ErrorOverlay))
                CloseLoadingError();
        }

        #region Initialization

        private void SetInitialVisibility()
        {
            _mainUI.Hud.style.display = DisplayStyle.Flex;
            _mainUI.MissingModelUnderlay.style.display = DisplayStyle.None;
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
            _mainUI.MissingModelUnderlay.pickingMode = PickingMode.Ignore;
            _mainUI.ButtonContainer.pickingMode = PickingMode.Ignore;
        }

        private void ListenToUI()
        {
            _mainUI.HelpButtonClicked += ShowHelp;
            _mainUI.CloseHelpButtonClicked += HideHelp;
            _mainUI.CloseErrorButtonClicked += CloseLoadingError;
        }

        private void ListenToStateMachine()
        {
            _stateMachine.Configure(ApplicationState.AwaitingModel)
                .OnEntry(ShowNoModelLoaded)
                .OnExit(HideNoModelLoaded)
                ;

            _stateMachine.Configure(ApplicationState.LoadingModel)
                .OnEntry(ShowLoading)
                .OnExit(HideLoading)
                ;

            _stateMachine.Configure(ApplicationState.LoadingError)
                .OnEntryFrom(
                    _stateMachine.ModelLoadingErrorTrigger,
                    SetErrorMessage
                )
                .OnEntry(ShowError)
                ;
        }

        #endregion

        #region Event Listeners

        private const int TransitionDuration = 100;

        private void SetErrorMessage(
            string errorMessage) =>
            _mainUI.ErrorMessageLabel.text = errorMessage;

        private void ShowNoModelLoaded() =>
            _mainUI.ShowLayer(
                _mainUI.MissingModelUnderlay,
                TransitionDuration
            );

        private void HideNoModelLoaded() =>
            _mainUI.HideLayer(
                _mainUI.MissingModelUnderlay,
                TransitionDuration
            );

        private void ShowLoading()
        {
            _mainUI.ShowLayer(
                _mainUI.LoadingOverlay,
                TransitionDuration
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
                TransitionDuration
            );

        private void ShowError() =>
            _mainUI.ShowLayer(
                _mainUI.ErrorOverlay,
                TransitionDuration
            );

        private void CloseLoadingError()
        {
            HideError();
            LoadingErrorClosed?.Invoke();
        }

        private void HideError() =>
            _mainUI.HideLayer(
                _mainUI.ErrorOverlay,
                TransitionDuration
            );

        private void ShowHelp() =>
            _mainUI.ShowLayer(
                _mainUI.HelpOverlay,
                TransitionDuration
            );

        private void HideHelp() =>
            _mainUI.HideLayer(
                _mainUI.HelpOverlay,
                TransitionDuration
            );

        #endregion
    }
}