//todo add copyright

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToolBuddy.PrintablesAR.Application;

namespace ToolBuddy.PrintablesAR.UI
{
    public static class FilePicker
    {
        public static async Task<bool> Show(
            NativeFilePicker.FilePickedCallback filePickedCallback,
            IEnumerable<string> supportedFileFormats)
        {
            bool hadPermission;
            if (PermissionManager.IsFilePermissionGranted())
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
                        // The permission dialog was shown again by RequestPermissionAsync. Retry
                        hadPermission = await Show(
                            filePickedCallback,
                            supportedFileFormats
                        );
                        break;
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
            IEnumerable<string> supportedFileFormats)
        {
            List<string> allowedMimeTypes;
            {
                allowedMimeTypes = supportedFileFormats.Select(NativeFilePicker.ConvertExtensionToFileType).ToList();
                // This bellow necessary because ConvertExtensionToFileType does not handle at least "obj" and "3mf". The above is necessary because "application/octet-stream" does not handle at least "stl".
                allowedMimeTypes.Add("application/octet-stream");
            }

            NativeFilePicker.PickFile(
                filePickedCallback,
                allowedMimeTypes.ToArray()
            );
        }
    }
}