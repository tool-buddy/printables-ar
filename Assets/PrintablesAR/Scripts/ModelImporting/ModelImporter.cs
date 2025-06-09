using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace ToolBuddy.PrintablesAR.ModelImporting
{
    /// <summary>
    /// Central model importer that delegates to specific format importers discovered via reflection.
    /// </summary>
    public class ModelImporter : MonoBehaviour, IModelImporter
    {
        private readonly Dictionary<string, IModelImporter> extensionToImporter = new();

        /// <inheritdoc/>
        public event Action<GameObject, string> ImportSucceeded;

        /// <inheritdoc/>
        public event Action<string, string> ImportFailed;

        /// <inheritdoc/>
        public event Action<float> OnProgress;

        /// <summary>
        /// List of supported file extensions.
        /// </summary>
        public IEnumerable<string> SupportedFileFormats => extensionToImporter.Keys;

        #region Unity callbacks

        private void Awake()
        {
            IEnumerable<(Type Type, IEnumerable<ModelImporterAttribute> Attributes)> importerInfos = GetImporterInfos();

            foreach ((Type Type, IEnumerable<ModelImporterAttribute> Attributes) importerInfo in importerInfos)
            {
                foreach (ModelImporterAttribute importerAttribute in importerInfo.Attributes)
                {
                    string ext = importerAttribute.FileExtension;

                    if (extensionToImporter.ContainsKey(ext))
                    {
                        Debug.LogWarning($"Duplicate model importer for extension '{ext}' ignored: {importerInfo.Type.FullName}");
                        continue;
                    }

                    IModelImporter importerInstance = GetImporterInstance(
                        importerInfo.Type
                    );

                    if (importerInstance == null)
                    {
                        Debug.LogWarning($"Failed to instantiate importer: {importerInfo.Type.FullName}");
                        continue;
                    }

                    extensionToImporter[ext] = importerInstance;
                }
            }
        }

        private void OnEnable()
        {
            //todo handle multiple loadings of different files
            //todo handle multiple loadings of same file
            //todo handle loading of invalid file (probably reset loadedMaterial and loaded mesh)

            foreach (IModelImporter importer in extensionToImporter.Values.Distinct())
            {
                importer.ImportSucceeded += OnImportSucceeded;
                importer.ImportFailed += OnImportFailed;
                importer.OnProgress += OnOnProgress;
            }
        }

        private void OnDisable()
        {
            foreach (IModelImporter importer in extensionToImporter.Values.Distinct())
            {
                importer.ImportSucceeded -= OnImportSucceeded;
                importer.ImportFailed -= OnImportFailed;
                importer.OnProgress -= OnOnProgress;
                try
                {
                    if (importer.IsImporting)
                        importer.CancelImport();
                }
                catch (OperationCanceledException e)
                {
                    Debug.LogException(e);
                }
            }
        }

        #endregion

        #region Dynamic importer discovery

        private static IEnumerable<(Type Type, IEnumerable<ModelImporterAttribute> Attribute)> GetImporterInfos() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(
                    a =>
                    {
                        try
                        {
                            return a.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            return e.Types.Where(t => t != null);
                        }
                    }
                )
                .Where(t => typeof(IModelImporter).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(
                    t => (Type: t, Attributes: t.GetCustomAttributes<ModelImporterAttribute>())
                )
                .Where(x => x.Attributes != null && x.Attributes.Any());

        [CanBeNull]
        private IModelImporter GetImporterInstance(
            Type importerInfoType)
        {
            IModelImporter importerInstance;

            if (typeof(MonoBehaviour).IsAssignableFrom(importerInfoType))
            {
                Component existing = GetComponent(importerInfoType);
                if (existing == null)
                    existing = gameObject.AddComponent(importerInfoType);
                importerInstance = existing as IModelImporter;
            }
            else
                importerInstance = Activator.CreateInstance(importerInfoType) as IModelImporter;

            return importerInstance;
        }

        #endregion

        #region IModelImporter

        public bool IsImporting => extensionToImporter.Values.Any(i => i.IsImporting);

        /// <inheritdoc/>
        public bool TryImport(
            string filePath) =>
            extensionToImporter.TryGetValue(
                PathToExtension(filePath),
                out IModelImporter importer
            )
            && importer.TryImport(filePath);

        public void CancelImport()
        {
            foreach (IModelImporter modelImporter in extensionToImporter.Values)
                if (modelImporter.IsImporting)
                    modelImporter.CancelImport();
        }

        #endregion

        private static string PathToExtension(
            string filePath) =>
            Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();

        private void OnImportSucceeded(
            GameObject go,
            string path) =>
            ImportSucceeded?.Invoke(
                go,
                path
            );

        private void OnImportFailed(
            string err,
            string path) =>
            ImportFailed?.Invoke(
                err,
                path
            );

        private void OnOnProgress(
            float progress)
        {
            OnProgress?.Invoke(progress);
        }
    }
}
