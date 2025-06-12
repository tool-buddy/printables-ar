namespace ToolBuddy.PrintablesAR.UI.Resources
{
    public static class ErrorMessages
    {
        public const string FileNotExisting = "The file '{0}' does not exist.";

        public const string UnsupportedFileFormat =
            "The file format '{0}' is not supported by the model loader.\nSupport formats are: {1}";

        public const string LoadingError = "An error occurred when loading file:\n'{0}'.\n\nThe error was:\n{1}";

        public const string PermissionError =
            "The application does not have permission to access your device's files.\nPlease grant ‘Storage’ permission in the App Permissions settings.";
    }
}