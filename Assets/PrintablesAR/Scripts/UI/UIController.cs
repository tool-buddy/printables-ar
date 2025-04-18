using ToolBuddy.PrintablesAR.Application;
using UnityEngine.UIElements;
using static ToolBuddy.PrintablesAR.Application.ApplicationStateMachine;

namespace ToolBuddy.PrintablesAR.UI
{
    public class UIController
    {
        private readonly ApplicationStateMachine _stateMachine;

        private readonly MainUI _mainUI;

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
        }

        private void ListenToStateMachine()
        {
            _stateMachine.Configure(ApplicationState.NoModelLoaded)
                .OnEntry(ShowNoModelLoaded)
                .OnExit(HideNoModelLoaded)
                ;

            //todo handle animation
            _stateMachine.Configure(ApplicationState.ModelLoading)
                .OnEntry(ShowLoading)
                .OnExit(HideLoading)
                ;

            _stateMachine.Configure(ApplicationState.LoadingError)
                .OnEntryFrom(
                    _stateMachine.ModelLoadingErrorTrigger,
                    SetErrorMessage
                )
                .OnEntry(ShowError)
                .OnExit(HideError)
                ;
        }

        #endregion

        #region Event Listeners

        private void SetErrorMessage(
            string errorMessage) =>
            _mainUI.ErrorMessageLabel.text = errorMessage;

        private void ShowNoModelLoaded() =>
            _mainUI.MissingModelUnderlay.style.display = DisplayStyle.Flex;

        private void HideNoModelLoaded() =>
            _mainUI.MissingModelUnderlay.style.display = DisplayStyle.None;

        private void ShowLoading() =>
            _mainUI.LoadingOverlay.style.display = DisplayStyle.Flex;

        private void HideLoading() =>
            _mainUI.LoadingOverlay.style.display = DisplayStyle.None;

        private void ShowError() =>
            _mainUI.ErrorOverlay.style.display = DisplayStyle.Flex;

        private void HideError() =>
            _mainUI.ErrorOverlay.style.display = DisplayStyle.None;

        private void ShowHelp() =>
            _mainUI.HelpOverlay.style.display = DisplayStyle.Flex;

        private void HideHelp() =>
            //todo handle the go back button in the mobile interface
            _mainUI.HelpOverlay.style.display = DisplayStyle.None;

        #endregion
    }
}