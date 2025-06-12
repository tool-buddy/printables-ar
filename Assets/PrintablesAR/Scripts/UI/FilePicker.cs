//todo add copyright

using System;
using System.Threading.Tasks;

namespace ToolBuddy.PrintablesAR.UI
{
    public static class FilePicker
    {
        public static async Task<bool> Show(
            NativeFilePicker.FilePickedCallback filePickedCallback)
        {
            bool hadPermission;
            if (NativeFilePicker.CheckPermission(true) == NativeFilePicker.Permission.Granted)
            {
                PickFile(filePickedCallback);
                hadPermission = true;
            }
            else
            {
                NativeFilePicker.Permission permission = await NativeFilePicker.RequestPermissionAsync(true);
                switch (permission)
                {
                    case NativeFilePicker.Permission.Granted:
                        PickFile(filePickedCallback);
                        hadPermission = true;
                        break;
                    case NativeFilePicker.Permission.ShouldAsk:
                        // The permission dialog was shown again by RequestPermissionAsync. Retry
                        hadPermission = await Show(filePickedCallback);
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
            NativeFilePicker.FilePickedCallback filePickedCallback) =>
            NativeFilePicker.PickFile(
                filePickedCallback,
                "*/*"
            );
    }
}