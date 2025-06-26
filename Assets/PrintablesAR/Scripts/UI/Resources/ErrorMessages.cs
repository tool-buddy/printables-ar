namespace ToolBuddy.PrintablesAR.UI.Resources
{
    /// <summary>
    /// A static class containing predefined error message strings.
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>
        /// Error message for attempting to load a file that does not exist.
        /// </summary>
        public const string FileNotExisting = "The file '{0}' does not exist.";

        /// <summary>
        /// Error message for attempting to load an unsupported file format.
        /// </summary>
        public const string UnsupportedFileFormat =
            "The file format '{0}' is not supported by the model loader.\nSupport formats are: {1}";

        /// <summary>
        /// Error message for a general loading error.
        /// </summary>
        public const string LoadingError = "An error occurred when loading file:\n'{0}'.\n\nThe error was:\n{1}";

        /// <summary>
        /// Error message for a permission error.
        /// </summary>
        public const string PermissionError =
            "The application does not have permission to access your device's files.\nPlease grant ‘Storage’ permission in the App Permissions settings.";
    }
}