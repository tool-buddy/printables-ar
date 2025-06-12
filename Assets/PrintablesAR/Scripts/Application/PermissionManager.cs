using System.Threading.Tasks;

namespace ToolBuddy.PrintablesAR.Application
{
    public static class PermissionManager
    {
        public static bool IsCameraPermissionGranted()
        {
#if UNITY_EDITOR
            return true;
#elif UNITY_ANDROID
            return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
#else
            throw new System.NotSupportedException("Platform not supported");
#endif
        }

        public static NativeFilePicker.Permission GetFileReadPermission() =>
            NativeFilePicker.CheckPermission(true);

        public static async Task<NativeFilePicker.Permission> RequestFileReadPermissionAsync() =>
            await NativeFilePicker.RequestPermissionAsync(true);
    }
}