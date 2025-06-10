using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using TriLibCore;
using TriLibCore.Extensions;
using UnityEngine;

namespace ToolBuddy.PrintablesAR.ModelImporting.Importers
{
    /// <summary>
    ///     Handles importing OBJ model files
    /// </summary>
    [ModelImporter("obj")]
    [ModelImporter("stl")]
    //todo add all relevant formats
    public class TriLibImporter : IModelImporter
    {
        //todo renaming and doc
        /// <inheritdoc/>
        public event Action<GameObject, string> ImportSucceeded;

        /// <inheritdoc/>
        public event Action<string, string> ImportFailed;

        [CanBeNull]
        private AssetLoaderContext _currentLoadingContext;

        public bool IsImporting => _currentLoadingContext != null;

        /// <inheritdoc/>
        public bool TryImport(
            string filePath)
        {
            LoadObjAsync(
                filePath
            );
            //todo avoid returning true?
            return true;
        }

        public void CancelImport()
        {
            if (_currentLoadingContext == null)
            {
                Debug.LogWarning("No import in progress to cancel.");
                return;
            }

            _currentLoadingContext.CancellationTokenSource?.Cancel();
            _currentLoadingContext = null;
        }

        /// <summary>
        /// Asynchronously loads the OBJ file and creates mesh.
        /// </summary>
        /// <param name="filePath">Path to the OBJ file.</param>
        private void LoadObjAsync(
            string filePath) =>
            _currentLoadingContext = AssetLoader.LoadModelFromFile(
                filePath,
                null,
                OnMaterialsLoad,
                null,
                OnError,
                null,
                GetLoaderOptions(Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant())
            ); //todo handle callbacks possibly called after destroying the object

        private static AssetLoaderOptions GetLoaderOptions(
            string fileExtension)
        {
            AssetLoaderOptions loaderOptions = AssetLoader.CreateDefaultLoaderOptions(
                false,
                true
            );
            //to be able to get the model's bounds and center it
            loaderOptions.ReadEnabled = true;
            loaderOptions.LoadTexturesAsSRGB = false;
            loaderOptions.UseUnityNativeTextureLoader = true;
            loaderOptions.UseUnityNativeNormalCalculator = true;
            loaderOptions.GetCompatibleTextureFormat = false;
            loaderOptions.EnforceAlphaChannelTextures = false;
            //forcing GC leads to huge freezes. Letting GC decide when to collect.
            loaderOptions.CollectCG = false;

            switch (fileExtension)
            {
                case "obj":
                    loaderOptions.MeshWorldTransform = Matrix4x4.TRS(
                        Vector3.zero,
                        Quaternion.AngleAxis(
                            90,
                            Vector3.left
                        ),
                        Vector3.one * 0.001f
                    );
                    break;
                case "stl":
                    loaderOptions.ScaleFactor = 0.001f;
                    break;
            }

            return loaderOptions;
        }


        private void OnError(
            IContextualizedError obj)
        {
            _currentLoadingContext = null;

            ImportFailed?.Invoke(
                obj.ToString(),
                obj.Context?.PersistentDataPath
            );
        }


        private void OnMaterialsLoad(
            AssetLoaderContext assetLoaderContext)
        {
            _currentLoadingContext = null;

            GameObject importedGameObject = assetLoaderContext.RootGameObject;
            string modelFilePath = assetLoaderContext.PersistentDataPath;

            if (importedGameObject.GetComponentsInChildren<MeshFilter>().Any())
            {
                CenterModel(importedGameObject);

                ImportSucceeded?.Invoke(
                    importedGameObject,
                    modelFilePath
                );
            }
            else
                ImportFailed?.Invoke(
                    "File has no 3D models.",
                    modelFilePath
                );
        }

        private static void CenterModel(
            GameObject loadedModel)
        {
            Bounds loadedModelBounds = loadedModel.CalculateBounds();
            loadedModel.transform.position = new Vector3(
                -loadedModelBounds.center.x,
                -loadedModelBounds.center.y + loadedModelBounds.extents.y,
                -loadedModelBounds.center.z
            );
        }
    }
}