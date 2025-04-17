//todo add copyright

using System;

namespace ToolBuddy.PrintablesAR.UI
{
    public static class FilePicker
    {
        public static void Show(
            NativeFilePicker.FilePickedCallback filePickedCallback)
        {
            NativeFilePicker.Permission filePickerPermission = NativeFilePicker.CheckPermission(true);
            //todo handle all permissions
            switch (filePickerPermission)
            {
                case NativeFilePicker.Permission.Denied:
                    break;
                case NativeFilePicker.Permission.Granted:
                    break;
                case NativeFilePicker.Permission.ShouldAsk:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            NativeFilePicker.PickFile(
                filePickedCallback,
                "*/*"
            );
        }
    }
}