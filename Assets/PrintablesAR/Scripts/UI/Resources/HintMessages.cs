namespace ToolBuddy.PrintablesAR.UI.Resources
{
    /// <summary>
    /// A static class containing predefined hint message strings.
    /// </summary>
    public static class HintMessages
    {
        /// <summary>
        /// Title for the hint when camera permission is not granted.
        /// </summary>
        public const string CameraPermissionNotGrantedTitle = "Camera Permission Not Granted";

        /// <summary>
        /// Description for the hint when camera permission is not granted.
        /// </summary>
        public const string CameraPermissionNotGrantedDescription =
            "Grant 'Camera' permission in the App Permissions settings to use AR features.";

        /// <summary>
        /// Title for the hint when a model is not loaded.
        /// </summary>
        public const string ModelNotLoadedTitle = "Model Not Loaded";
        
        /// <summary>
        /// Description for the hint when a model is not loaded.
        /// </summary>
        public const string ModelNotLoadedDescription = "Tap the 'Load' button to load a model.";
        
        /// <summary>
        /// Title for the hint when the environment is not scanned.
        /// </summary>
        public const string EnvironmentNotScannedTitle = "Environment Not Scanned";

        /// <summary>
        /// Description for the hint when the environment is not scanned.
        /// </summary>
        public const string EnvironmentNotScannedDescription =
            "Slowly move your phone around to scan your surroundings. This may take several seconds.";

        /// <summary>
        /// Title for the hint when the device does not support AR.
        /// </summary>
        public const string ArUnsupportedTitle = "Device Not Supported";

        /// <summary>
        /// Description for the hint when the device does not support AR.
        /// </summary>
        public const string ArUnsupportedDescription =
            "This device does not support Augmented Reality applications.";

        /// <summary>
        /// Title for the hint when the required AR software is not installed.
        /// </summary>
        public const string ArSoftwareNorInstalledTitle = "AR Software Missing";

        /// <summary>
        /// Description for the hint when the required AR software is not installed.
        /// </summary>
        public const string ArSoftwareNorInstalledDescription =
            "This device lacks the necessary software to run an Augmented Reality application. Please install it.";
    }
}