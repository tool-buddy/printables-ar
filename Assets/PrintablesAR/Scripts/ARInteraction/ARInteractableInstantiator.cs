using System;
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

        [CanBeNull]
        private GameObject _currentInstance;

        public event Action<GameObject> InteractableInstantiated;

        public ARInteractableInstantiator(
            [NotNull] ModelImporter modelImporter,
            [NotNull] ApplicationStateMachine stateMachine,
            [NotNull] Raycaster raycaster)
        {
            stateMachine.Configure(ApplicationStateMachine.ApplicationState.ModelSpawn)
                .OnEntry(Enable)
                .OnExit(Disable);

            //todo thorough test, and check model's visibility while loading, once you have replaced the current obj loader

            _modelImporter = modelImporter;
            _modelImporter.ImportSucceeded += OnModelImportSucceeded;
            _modelImporter.ImportFailed += OnModelImportFailed;

            _raycaster = raycaster;
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
                finger,
                _currentInstance.transform,
                Camera.main.transform.position,
                _raycaster
            );

            if (placementSucceeded)
            {
                _currentInstance.gameObject.SetActive(true);
                InteractableInstantiated?.Invoke(_currentInstance);
            }
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
            loadedModel.transform.SetParent(_currentInstance.transform);
        }

        #endregion
    }
}