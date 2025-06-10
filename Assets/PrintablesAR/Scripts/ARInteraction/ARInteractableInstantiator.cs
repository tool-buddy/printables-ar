using System;
using Assets.PrintablesAR.Scripts.Sound;
using JetBrains.Annotations;
using ToolBuddy.PrintablesAR.Application;
using ToolBuddy.PrintablesAR.ModelImporting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.EnhancedTouch;
using Object = UnityEngine.Object;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public class ARInteractableInstantiator : IDisposable
    {
        [NotNull]
        private readonly ModelImporter _modelImporter;

        [NotNull]
        private readonly Raycaster _raycaster;

        [NotNull]
        private readonly AudioPlayer _audioPlayer;

        [CanBeNull]
        private GameObject _currentInstance;

        public event Action<GameObject> InteractableInstantiated;

        public ARInteractableInstantiator(
            [NotNull] ModelImporter modelImporter,
            [NotNull] ApplicationStateMachine stateMachine,
            [NotNull] Raycaster raycaster,
            [NotNull] AudioPlayer audioPlayer)
        {
            stateMachine.Configure(ApplicationStateMachine.ApplicationState.SpawningModel)
                .OnEntry(Enable)
                .OnExit(Disable);

            _modelImporter = modelImporter;
            _modelImporter.ImportSucceeded += OnModelImportSucceeded;
            _modelImporter.ImportFailed += OnModelImportFailed;

            _raycaster = raycaster ?? throw new ArgumentNullException(nameof(raycaster));
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
        }

        public void Dispose()
        {
            Disable();
            _modelImporter.ImportSucceeded -= OnModelImportSucceeded;
            _modelImporter.ImportFailed -= OnModelImportFailed;
            DeleteInstance();
        }


        private void Enable() =>
            Touch.onFingerDown += ProcessFingerDown;


        private void Disable() =>
            Touch.onFingerDown -= ProcessFingerDown;


        #region Touch processing

        private void ProcessFingerDown(
            Finger finger)
        {
            //todo better error handling
            Assert.IsNotNull(
                _currentInstance,
                "Loaded model is null, cannot instantiate object."
            );

            Assert.IsFalse(
                _currentInstance.activeSelf,
                "Model already instantiated."
            );

            bool placementSucceeded = TransformManipulator.TryPlace(
                finger.screenPosition,
                _currentInstance.transform,
                Camera.main.transform.position,
                _raycaster
            );

            if (placementSucceeded)
            {
                _currentInstance.gameObject.SetActive(true);
                InteractableInstantiated?.Invoke(_currentInstance);
            }
            else if (_raycaster.IsFingerOnUI(finger.screenPosition) == false)
                _audioPlayer.PlayOneShot(
                    Sounds.FailedPlacement,
                    true
                );
        }

        #endregion


        #region Model importing

        //todo move to another file

        private void OnModelImportFailed(
            string errorMessage,
            string filePath) =>
            DeleteInstance();

        private void DeleteInstance()
        {
            if (_currentInstance == null)
                return;

            Object.Destroy(_currentInstance);
            _currentInstance = null;
        }

        private void OnModelImportSucceeded(
            [NotNull] GameObject loadedModel,
            string filePath)
        {
            DeleteInstance();
            SetUpInstance(loadedModel);
        }

        private void SetUpInstance(
            GameObject loadedModel)
        {
            _currentInstance = new GameObject($"{loadedModel.name} Model");
            _currentInstance.gameObject.SetActive(false);
            ARInteractable arInteractable = _currentInstance.AddComponent<ARInteractable>();
            arInteractable.Raycaster = _raycaster;
            arInteractable.AudioPlayer = _audioPlayer;

            loadedModel.transform.SetParent(_currentInstance.transform);
        }

        #endregion
    }
}