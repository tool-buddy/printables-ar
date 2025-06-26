using System;
using UnityEngine;

namespace ToolBuddy.PrintablesAR.ModelImporting
{
    /// <summary>
    /// An interface for classes that can import 3D models.
    /// </summary>
    public interface IModelImporter
    {
        /// <summary>
        /// Event triggered when a model is successfully imported.
        /// The first parameter is the imported GameObject, and the second is the file path.
        /// </summary>
        event Action<GameObject, string> ImportSucceeded;

        /// <summary>
        /// Event triggered when a model import fails.
        /// The first parameter is the error message, and the second is the file path.
        /// </summary>
        event Action<string, string> ImportFailed;

        /// <summary>
        /// Gets a value indicating whether an import is currently in progress.
        /// </summary>
        bool IsImporting { get; }

        /// <summary>
        ///     Start loading a 3d model file from the given path
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>false if file is not compatible with the importer</returns>
        bool TryImport(
            string filePath);

        /// <summary>
        /// Cancels the current import operation.
        /// </summary>
        void CancelImport();
    }
}