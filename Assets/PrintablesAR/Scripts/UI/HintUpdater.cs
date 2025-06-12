using ToolBuddy.PrintablesAR.Application;
using ToolBuddy.PrintablesAR.UI.Resources;
using UnityEngine.XR.ARFoundation;

namespace ToolBuddy.PrintablesAR.UI
{
    public class HintUpdater
    {
        private readonly ARPlaneManager _arPlaneManager;
        private readonly ApplicationStateMachine _stateMachine;
        private readonly MainUI _mainUI;

        private const int _transitionDuration = 100;

        public HintUpdater(
            ApplicationStateMachine stateMachine,
            MainUI mainUI,
            ARPlaneManager arPlaneManager)
        {
            _stateMachine = stateMachine;
            _mainUI = mainUI;
            _arPlaneManager = arPlaneManager;
        }

        public void Update()
        {
            if (PermissionManager.IsCameraPermissionGranted() == false)
            {
                _mainUI.SetHint(
                    HintMessages.CameraPermissionNotGrantedTitle,
                    HintMessages.CameraPermissionNotGrantedDescription
                );
                ShowHintLayer();
            }
            else if (_stateMachine.IsInState(ApplicationStateMachine.ApplicationState.AwaitingModel)
                     || _stateMachine.IsInState(ApplicationStateMachine.ApplicationState.LoadingModel)
                     || _stateMachine.IsInState(ApplicationStateMachine.ApplicationState.ShowingError))
            {
                _mainUI.SetHint(
                    HintMessages.ModelNotLoadedTitle,
                    HintMessages.ModelNotLoadedDescription
                );
                ShowHintLayer();
            }
            else if (_arPlaneManager.trackables.count == 0)
            {
                _mainUI.SetHint(
                    HintMessages.EnvironmentNotScannedTitle,
                    HintMessages.EnvironmentNotScannedDescription
                );
                ShowHintLayer();
            }
            else
                HideHintLayer();
        }

        private void ShowHintLayer()
        {
            if (_mainUI.IsLayerShown(_mainUI.HintLayer) == false)
                _mainUI.ShowLayer(
                    _mainUI.HintLayer,
                    _transitionDuration
                );
        }

        private void HideHintLayer()
        {
            if (_mainUI.IsLayerShown(_mainUI.HintLayer))
                //todo bug layer's opacity does not fade out
                _mainUI.HideLayer(
                    _mainUI.HintLayer,
                    _transitionDuration
                );
        }
    }
}