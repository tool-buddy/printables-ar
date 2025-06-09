using ToolBuddy.PrintablesAR.Application;
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
            if (_stateMachine.IsInState(ApplicationStateMachine.ApplicationState.AwaitingModel)
                || _stateMachine.IsInState(ApplicationStateMachine.ApplicationState.LoadingModel)
                || _stateMachine.IsInState(ApplicationStateMachine.ApplicationState.ShowingError))
            {
                _mainUI.SetHint(
                    "No model loaded",
                    "Tap 'Load' to load a model."
                );
                ShowHintLayer();
            }
            else if (_arPlaneManager.trackables.count == 0)
            {
                _mainUI.SetHint(
                    "No environment tracked",
                    "To scan your environment, slowly rotate and move your phone.\nThis may take several seconds."
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