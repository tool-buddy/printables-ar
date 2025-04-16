#define LOG

using System;
using System.Linq;
using AsImpL;
using JetBrains.Annotations;
using UnityEngine;

namespace ToolBuddy.PrintableAR.ModelImporting.Importers
{
    /// <summary>
    ///     Handles importing OBJ model files
    /// </summary>
    [ModelImporter("obj")]
    [RequireComponent(typeof(ObjectImporter))]
    //todo rename
    //todo needs to be a MonoBehaviour?
    public class ObjImporter : MonoBehaviour, IModelImporter
    {
        //todo renaming and doc
        /// <inheritdoc/>
        public event Action<GameObject, string> ImportSucceeded;

        /// <inheritdoc/>
        public event Action<string, string> ImportFailed;

        [NotNull] private ObjectImporter objImporter => GetComponent<ObjectImporter>();

        private void Awake()
        {
            ObjectImporter importer = objImporter;
            importer.ImportedModel += OnModelImported;
            importer.ImportError += OnImportError;
        }

        private void OnDestroy()
        {
            ObjectImporter importer = objImporter;
            importer.ImportedModel -= OnModelImported;
            importer.ImportError -= OnImportError;
        }

        /// <summary>
        ///    Called when a model has been successfully imported
        /// </summary>
        /// <param name="importedGameObject"></param>
        /// <param name="modelFilePath"></param>
        private void OnModelImported(
            GameObject importedGameObject,
            string modelFilePath)
        {
            if (importedGameObject.GetComponentInChildren<ModelReferences>().MeshReferences.Any())
                ImportSucceeded?.Invoke(
                    importedGameObject,
                    modelFilePath
                );
            else
                //todo should destroy object? Destroy(importedGameObject);
                ImportFailed?.Invoke(
                    "File has no 3D models.",
                    modelFilePath
                );
#if LOG
            Debug.Log(" OnModelImported " + modelFilePath);
#endif
        }

        private void OnImportError(
            string modelFilePath)
        {
            ImportFailed?.Invoke(
                "File could not be loaded.",
                modelFilePath
            );
            //todo destroy obj?
#if LOG
            Debug.Log(" OnImportError ");
#endif
        }

        /// <inheritdoc/>
        public bool TryImport(
            string filePath)
        {
            LoadOBJAsync(
                filePath
            );

            return true;
        }

        /// <summary>
        /// Asynchronously loads the OBJ file and creates mesh.
        /// </summary>
        /// <param name="filePath">Path to the OBJ file.</param>
        private void LoadOBJAsync(
            string filePath)
        {
            ImportOptions importOptions = new ImportOptions
            {
                zUp = true,
                //todo test this
                modelScaling = 0.01f,
                //todo build collider things if needed
                //buildColliders = true,
                //colliderConvex = true,
                reuseLoaded = true,
                //todo fix hideWhileLoading set to true if needed, and create a push request
                //hideWhileLoading = true,
                convertToDoubleSided = true
                //todo fix inheritLayer set to true bugging when parent is null if needed, and create a push request
            };

            objImporter.ImportModelAsync(
                "imported_model",
                filePath,
                transform,
                importOptions
            );
        }
    }
}