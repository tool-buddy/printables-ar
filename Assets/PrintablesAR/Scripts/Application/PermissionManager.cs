using System.Threading.Tasks;

namespace ToolBuddy.PrintablesAR.Application
{
    /// <summary>
    /// A class for handling runtime permissions.
    /// </summary>
    public static class PermissionManager
    {
        /// <summary>
        /// Checks if the application has been granted camera permission.
        /// </summary>
        /// <returns><see langword="true"/> if camera permission is granted; otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Checks if the application has been granted permission to read external storage.
        /// </summary>
        /// <returns><see langword="true"/> if file read permission is granted; otherwise, <see langword="false"/>.</returns>
        public static bool IsFileReadPermissionGranted() =>
            NativeFilePicker.CheckPermission(true);

        /// <summary>
        /// Asynchronously requests permission to read from external storage.
        /// </summary>
        /// <returns>A task that represents the asynchronous permission request operation.</returns>
        public static async Task<NativeFilePicker.Permission> RequestFileReadPermissionAsync() =>
            await NativeFilePicker.RequestPermissionAsync(true);
    }
}