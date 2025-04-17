#define LOG

using System;
using System.Collections;
using System.Linq;
using System.Threading;
using Parabox.Stl;
using UnityEngine;

namespace ToolBuddy.PrintablesAR.ModelImporting.Importers
{
    /// <summary>
    /// Handles importing STL model files.
    /// </summary>
    //todo needs to be a MonoBehaviour?
    [ModelImporter("stl")]
    public class StlImporter : MonoBehaviour, IModelImporter
    {
        /// <inheritdoc/>
        public event Action<GameObject, string> ImportSucceeded;

        /// <inheritdoc/>
        public event Action<string, string> ImportFailed;

        /// <inheritdoc/>
        public bool TryImport(
            string filePath)
        {
            //StartCoroutine(LoadStlAsync(filePath));
            LoadStlSync(filePath);
            return true;
        }

        private IEnumerator LoadStlAsync(
            string filePath)
        {
            //Todo Make LoadStlAsync work with Unity

            Mesh[] meshes = null;
            Exception caughtException = null;

            yield return new WaitForBackgroundThread(
                () =>
                {
                    try
                    {
                        meshes = Importer.Import(
                            filePath
                        );
                    }
                    catch (Exception ex)
                    {
                        caughtException = ex;
                    }
                }
            );

            if (caughtException != null)
            {
                ImportFailed?.Invoke(
                    $"Error importing STL: {caughtException.Message}",
                    filePath
                );
#if LOG
                Debug.Log($"[StlImporter] Import error: {caughtException}");
#endif
                yield break;
            }

            if (meshes == null || meshes.Length == 0 || meshes.All(m => m == null || m.vertexCount == 0))
            {
                ImportFailed?.Invoke(
                    "File has no 3D models or meshes are empty.",
                    filePath
                );
#if LOG
                Debug.Log("[StlImporter] No valid meshes found.");
#endif
                yield break;
            }

            GameObject rootObject = new GameObject("imported_stl_model");
            rootObject.transform.SetParent(
                transform,
                false
            );

            foreach (Mesh mesh in meshes)
            {
                if (mesh == null || mesh.vertexCount == 0)
                    continue;

                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.SetParent(
                    rootObject.transform,
                    false
                );

                MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
                meshFilter.mesh = mesh;

                MeshRenderer meshRenderer = meshObj.AddComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Standard"));
            }

            ImportSucceeded?.Invoke(
                rootObject,
                filePath
            );
#if LOG
            Debug.Log($"[StlImporter] Successfully imported STL: {filePath}");
#endif
        }

        private void LoadStlSync(
            string filePath)
        {
            //todo handle scale
            Mesh[] meshes = null;
            Exception caughtException = null;

            try
            {
                meshes = Importer.Import(
                    filePath
                );
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            if (caughtException != null)
            {
                ImportFailed?.Invoke(
                    $"Error importing STL: {caughtException.Message}",
                    filePath
                );
#if LOG
                Debug.Log($"[StlImporter] Import error: {caughtException}");
#endif
                return;
            }

            if (meshes == null || meshes.Length == 0 || meshes.All(m => m == null || m.vertexCount == 0))
            {
                ImportFailed?.Invoke(
                    "File has no 3D models or meshes are empty.",
                    filePath
                );
#if LOG
                Debug.Log("[StlImporter] No valid meshes found.");
#endif
                return;
            }

            GameObject rootObject = new GameObject("imported_stl_model");
            rootObject.transform.SetParent(
                transform,
                false
            );

            foreach (Mesh mesh in meshes)
            {
                if (mesh == null || mesh.vertexCount == 0)
                    continue;

                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.SetParent(
                    rootObject.transform,
                    false
                );

                MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
                meshFilter.mesh = mesh;
                meshObj.AddComponent<MeshRenderer>();
            }

            ImportSucceeded?.Invoke(
                rootObject,
                filePath
            );
#if LOG
            Debug.Log($"[StlImporter] Successfully imported STL: {filePath}");
#endif
        }
    }

    /// <summary>
    /// Utility to run code on a background thread and resume on main thread.
    /// </summary>
    public class WaitForBackgroundThread : CustomYieldInstruction
    {
        private bool isDone;

        public override bool keepWaiting => !isDone;

        public WaitForBackgroundThread(
            Action backgroundWork) =>
            ThreadPool.QueueUserWorkItem(
                _ =>
                {
                    try
                    {
                        backgroundWork();
                    }
                    finally
                    {
                        isDone = true;
                    }
                }
            );
    }
}