using System;
using ToolBuddy.PrintablesAR.Application;
using ToolBuddy.PrintablesAR.UI.Resources;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ApplicationState = ToolBuddy.PrintablesAR.Application.ApplicationStateMachine.ApplicationState;

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
            if (_stateMachine.IsInState(ApplicationState.CheckingHardware)
                || _stateMachine.IsInState(ApplicationState.CheckingSoftware))
                DisplayInitializationHints();
            else if (PermissionManager.IsCameraPermissionGranted() == false)
                ShowHint(
                    HintMessages.CameraPermissionNotGrantedTitle,
                    HintMessages.CameraPermissionNotGrantedDescription
                );
            else if (_stateMachine.IsInState(ApplicationState.AwaitingModel)
                     || _stateMachine.IsInState(ApplicationState.LoadingModel)
                     || _stateMachine.IsInState(ApplicationState.ShowingError))
                ShowHint(
                    HintMessages.ModelNotLoadedTitle,
                    HintMessages.ModelNotLoadedDescription
                );
            else if (HasValidEnvironmentScan() == false)
                ShowHint(
                    HintMessages.EnvironmentNotScannedTitle,
                    HintMessages.EnvironmentNotScannedDescription
                );
            else
                HideHintLayer();
        }

        private void ShowHint(
            string title,
            string description)
        {
            _mainUI.SetHint(
                title,
                description
            );
            ShowHintLayer();
        }

        private void DisplayInitializationHints()
        {
            switch (ARSession.state)
            {
                case ARSessionState.Unsupported:
                    ShowHint(
                        HintMessages.ArUnsupportedTitle,
                        HintMessages.ArUnsupportedDescription
                    );
                    break;
                case ARSessionState.NeedsInstall:
                    ShowHint(
                        HintMessages.ArSoftwareNorInstalledTitle,
                        HintMessages.ArSoftwareNorInstalledDescription
                    );
                    break;
                case ARSessionState.Ready:
                case ARSessionState.SessionInitializing:
                case ARSessionState.SessionTracking:
                case ARSessionState.None:
                case ARSessionState.Installing:
                case ARSessionState.CheckingAvailability:
                    //nothing
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool HasValidEnvironmentScan()
        {
            bool isEnvironmentScanned = false;
            TrackableCollection<ARPlane> trackableCollection = _arPlaneManager.trackables;
            foreach (ARPlane arPlane in trackableCollection)
                if (arPlane.trackingState == TrackingState.Tracking)
                {
                    isEnvironmentScanned = true;
                    break;
                }

            return isEnvironmentScanned;
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