using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace ToolBuddy.PrintableAR.ModelImporting
{
    /// <summary>
    /// Central model importer that delegates to specific format importers discovered via reflection.
    /// </summary>
    public class ModelImporter : MonoBehaviour, IModelImporter
    {
        private readonly Dictionary<string, IModelImporter> extensionToLoader = new();

        /// <inheritdoc/>
        public event Action<GameObject, string> ImportSucceeded;

        /// <inheritdoc/>
        public event Action<string, string> ImportFailed;

        /// <summary>
        /// List of supported file extensions.
        /// </summary>
        public IEnumerable<string> SupportedFileFormats => extensionToLoader.Keys;

        private void Awake()
        {
            // TODO: Cache loader discovery statically to avoid repeated reflection?
            IEnumerable<(Type Type, ModelImporterAttribute Attribute)> loaderTypes = GetLoaderTypes();

            foreach ((Type Type, ModelImporterAttribute Attribute) loaderInfo in loaderTypes)
            {
                string ext = loaderInfo.Attribute.FileExtension;

                if (extensionToLoader.ContainsKey(ext))
                {
                    Debug.LogWarning($"Duplicate model loader for extension '{ext}' ignored: {loaderInfo.Type.FullName}");
                    continue;
                }

                IModelImporter loaderInstance = GetLoaderInstance(loaderInfo);

                if (loaderInstance == null)
                {
                    Debug.LogWarning($"Failed to instantiate loader: {loaderInfo.Type.FullName}");
                    continue;
                }

                extensionToLoader[ext] = loaderInstance;
            }
        }

        private static IEnumerable<(Type Type, ModelImporterAttribute Attribute)> GetLoaderTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
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
                    t => (Type: t, Attribute: t.GetCustomAttribute<ModelImporterAttribute>())
                )
                .Where(x => x.Attribute != null);
        }

        [CanBeNull]
        private IModelImporter GetLoaderInstance(
            (Type Type, ModelImporterAttribute Attribute) loaderInfo)
        {
            IModelImporter loaderInstance;

            if (typeof(MonoBehaviour).IsAssignableFrom(loaderInfo.Type))
            {
                Component existing = GetComponent(loaderInfo.Type);
                if (existing == null)
                    existing = gameObject.AddComponent(loaderInfo.Type);
                loaderInstance = existing as IModelImporter;
            }
            else
                loaderInstance = Activator.CreateInstance(loaderInfo.Type) as IModelImporter;

            return loaderInstance;
        }

        private void OnEnable()
        {
            //todo handle multiple loadings of different files
            //todo handle multiple loadings of same file
            //todo handle loading of invalid file (probably reset loadedMaterial and loaded mesh)

            foreach (KeyValuePair<string, IModelImporter> pair in extensionToLoader)
            {
                pair.Value.ImportSucceeded += OnImportSucceeded;
                pair.Value.ImportFailed += OnImportFailed;
            }
        }

        private void OnDisable()
        {
            foreach (KeyValuePair<string, IModelImporter> pair in extensionToLoader)
            {
                pair.Value.ImportSucceeded -= OnImportSucceeded;
                pair.Value.ImportFailed -= OnImportFailed;
            }
        }

        /// <inheritdoc/>
        public bool TryImport(
            string filePath) =>
            extensionToLoader.TryGetValue(
                PathToExtension(filePath),
                out IModelImporter loader
            )
            && loader.TryImport(filePath);

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
    }
}