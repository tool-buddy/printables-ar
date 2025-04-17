using System;
using JetBrains.Annotations;
using ToolBuddy.PrintablesAR.ModelImporting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;

namespace ToolBuddy.PrintablesAR.ARInteraction
{
    public class ARInteractableInstantiator : TouchMonoBehaviour
    {
        private void Awake() =>
            _modelImporter = FindFirstObjectByType<ModelImporter>();

        protected override void OnEnable()
        {
            base.OnEnable();
            _modelImporter.ImportSucceeded += OnModelImportSucceeded;
            _modelImporter.ImportFailed += OnModelImportFailed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _modelImporter.ImportSucceeded -= OnModelImportSucceeded;
            _modelImporter.ImportFailed -= OnModelImportFailed;
        }

        #region Touch processing

        protected override void ProcessFingerDown(
            Finger finger)
        {
            bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1);

            if (IsModelSpawned || isPointerOverUI)
                return;

            //todo better error handling
            Assert.IsNotNull(
                _importedModel,
                "Loaded model is null, cannot spawn object."
            );

            bool placementSucceeded = TransformManipulator.TryPlace(
                finger,
                _importedModel.transform
            );

            if (placementSucceeded)
                _importedModel.gameObject.SetActive(true);
        }

        protected override void ProcessFingerMove(
            Finger finger) { }

        protected override void ProcessFingerUp(
            Finger finger) { }

        #endregion


        #region Model importing

        //todo move to another file

        [CanBeNull]
        private GameObject _importedModel;

        private ModelImporter _modelImporter;

        private bool IsModelSpawned => _importedModel && _importedModel.activeSelf;

        private static Lazy<Material> DefaultMaterial =>
            new Lazy<Material>(Resources.Load<Material>("Grid"));

        private void OnModelImportSucceeded(
            [NotNull] GameObject loadedObj,
            string filePath)
        {
            //todo delete previous instance

            _importedModel = loadedObj;

            SetupMaterial(_importedModel);
            _importedModel.gameObject.SetActive(false);
            _importedModel.AddComponent<ARInteractable>();
        }

        private void SetupMaterial(
            GameObject @object)
        {
            MeshRenderer meshRenderer = @object.GetComponent<MeshRenderer>();
            Material[] materials = meshRenderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
                materials[i] = DefaultMaterial.Value;
        }

        private void OnModelImportFailed(
            string errorMessage,
            string filePath) =>
            _importedModel = null;

        #endregion
    }
}