using System;
using System.Linq;
using TriLibCore;
using UnityEngine;

namespace ToolBuddy.PrintablesAR.ModelImporting.Importers
{
    /// <summary>
    ///     Handles importing OBJ model files
    /// </summary>
    [ModelImporter("obj")]
    [ModelImporter("stl")]
    //todo add all relevant formats
    //todo needs to be a MonoBehaviour?
    public class TriLibImporter : MonoBehaviour, IModelImporter
    {
        //todo renaming and doc
        /// <inheritdoc/>
        public event Action<GameObject, string> ImportSucceeded;

        /// <inheritdoc/>
        public event Action<string, string> ImportFailed;

        /// <inheritdoc/>
        public bool TryImport(
            string filePath)
        {
            LoadObjAsync(
                filePath
            );

            return true;
        }

        /// <summary>
        /// Asynchronously loads the OBJ file and creates mesh.
        /// </summary>
        /// <param name="filePath">Path to the OBJ file.</param>
        private void LoadObjAsync(
            string filePath) =>
            AssetLoader.LoadModelFromFile(
                filePath,
                null,
                OnMaterialsLoad,
                null,
                OnError,
                null,
                GetLoaderOptions()
            );

        private static AssetLoaderOptions GetLoaderOptions()
        {
            AssetLoaderOptions loaderOptions = AssetLoader.CreateDefaultLoaderOptions();

            loaderOptions.MeshWorldTransform = Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.AngleAxis(
                    90,
                    Vector3.left
                ),
                Vector3.one * 0.01f
            );

            return loaderOptions;
        }


        private void OnError(
            IContextualizedError obj)
        {
            Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
            ImportFailed?.Invoke(
                obj.ToString(),
                obj.Context.PersistentDataPath
            );
        }


        private void OnMaterialsLoad(
            AssetLoaderContext assetLoaderContext)
        {
            GameObject importedGameObject = assetLoaderContext.RootGameObject;
            string modelFilePath = assetLoaderContext.PersistentDataPath;
            Debug.Log("Materials loaded. Model fully loaded.");
            if (importedGameObject.GetComponentsInChildren<MeshFilter>().Any())
                ImportSucceeded?.Invoke(
                    importedGameObject,
                    modelFilePath
                );
            else
                ImportFailed?.Invoke(
                    "File has no 3D models.",
                    modelFilePath
                );
        }
    }
}