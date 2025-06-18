//todo add copyright

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using ToolBuddy.PrintablesAR.Application;

namespace ToolBuddy.PrintablesAR.UI
{
    public static class FilePicker
    {
        /// <summary>
        /// Displays a file picker dialog to the user and invokes the specified callback with the selected file.
        /// </summary>
        /// <remarks>If the user does not have file read permissions, the method attempts to request the
        /// necessary permissions. If the permission request is denied, the file picker will not be displayed, and the
        /// method will return <see langword="false"/>.</remarks>
        /// <param name="filePickedCallback">A callback that is invoked when the user selects a file. The callback receives the path of the selected
        /// file.</param>
        /// <param name="supportedFileFormats">A collection of file extensions that specifies the types of files the user can select. If empty, all file types are allowed.</param>
        /// <returns><see langword="true"/> if the file picker was successfully displayed and the user had the necessary
        /// permissions; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if an unexpected permission state is encountered during the permission request process.</exception>
        public static async Task<bool> Show(
            NativeFilePicker.FilePickedCallback filePickedCallback,
            [NotNull] IEnumerable<string> supportedFileFormats)
        {
            bool hadPermission;
            if (PermissionManager.IsFileReadPermissionGranted())
            {
                PickFile(
                    filePickedCallback,
                    supportedFileFormats
                );
                hadPermission = true;
            }
            else
            {
                NativeFilePicker.Permission permission = await PermissionManager.RequestFileReadPermissionAsync();
                switch (permission)
                {
                    case NativeFilePicker.Permission.Granted:
                        PickFile(
                            filePickedCallback,
                            supportedFileFormats
                        );
                        hadPermission = true;
                        break;
                    case NativeFilePicker.Permission.ShouldAsk:
                    case NativeFilePicker.Permission.Denied:
                        hadPermission = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return hadPermission;
        }

        private static void PickFile(
            NativeFilePicker.FilePickedCallback filePickedCallback,
            [NotNull] IEnumerable<string> supportedFileFormats) =>
            NativeFilePicker.PickFile(
                filePickedCallback,
                GetAllowedMimeTypes(supportedFileFormats)
            );

        [NotNull]
        private static string[] GetAllowedMimeTypes(
            [NotNull] IEnumerable<string> supportedFileFormats)
        {
            if (supportedFileFormats.Any() == false)
                return Array.Empty<string>();

            List<string> allowedMimeTypes;
            {
                allowedMimeTypes = supportedFileFormats.Select(NativeFilePicker.ConvertExtensionToFileType).ToList();
                // This bellow necessary because ConvertExtensionToFileType does not handle at least "obj" and "3mf". The above is necessary because "application/octet-stream" does not handle at least "stl".
                allowedMimeTypes.Add("application/octet-stream");
            }

            return allowedMimeTypes.ToArray();
        }
    }
}