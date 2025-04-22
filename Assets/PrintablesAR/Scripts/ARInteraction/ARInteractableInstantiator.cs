using System;
using JetBrains.Annotations;
using ToolBuddy.PrintablesAR.Application;
using ToolBuddy.PrintablesAR.ModelImporting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Object = UnityEngine.Object;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public class ARInteractableInstantiator : IDisposable
    {
        [CanBeNull]
        private GameObject _currentInstance;

        [NotNull]
        private readonly ModelImporter _modelImporter;

        public ARInteractableInstantiator(
            [NotNull] ModelImporter modelImporter,
            [NotNull] ApplicationStateMachine stateMachine)
        {
            stateMachine.Configure(ApplicationStateMachine.ApplicationState.ModelPlacement)
                .OnEntry(Enable)
                .OnExit(Disable);

            //todo thorough test, and check model's visibility while loading, once you have replaced the current obj loader

            _modelImporter = modelImporter;
            _modelImporter.ImportSucceeded += OnModelImportSucceeded;
            _modelImporter.ImportFailed += OnModelImportFailed;
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
                "Loaded model is null, cannot spawn object."
            );

            bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);
            bool isInstancePlaced = _currentInstance.activeSelf;

            if (isInstancePlaced || isPointerOverUI)
                return;

            bool placementSucceeded = TransformManipulator.TryPlace(
                finger,
                _currentInstance.transform,
                Camera.main.transform.position
            );

            if (placementSucceeded)
                _currentInstance.gameObject.SetActive(true);
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
            GameObject loadedObj)
        {
            _currentInstance = loadedObj;
            _currentInstance.gameObject.SetActive(false);
            _currentInstance.AddComponent<ARInteractable>();
        }

        #endregion
    }
}