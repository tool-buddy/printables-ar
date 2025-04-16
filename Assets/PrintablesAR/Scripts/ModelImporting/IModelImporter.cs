using System;
using UnityEngine;

namespace ToolBuddy.PrintableAR.ModelImporting
{
    public interface IModelImporter
    {
        //todo doc
        /// <summary>
        ///  
        /// </summary>
        event Action<GameObject, string> ImportSucceeded;

        /// <summary>
        /// 
        /// </summary>
        event Action<string, string> ImportFailed;

        /// <summary>
        ///     Start loading a 3d model file from the given path
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>false if file is not compatible with the importer</returns>
        bool TryImport(
            string filePath);
    }
}