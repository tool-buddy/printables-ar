namespace ToolBuddy.PrintableAR
{
    public partial class UIController
    {
        /// <summary>
        /// Centralized error message templates for UI.
        /// </summary>
        private class ErrorMessages
        {
            public const string FileNotExisting = "The file '{0}' does not exist.";

            public const string UnsupportedFileFormat =
                "The file format '{0}' is not supported by the model loader.\nSupport formats are: {1}";

            public const string LoadingError = "An error occurred when loading file:\n'{0}'.\n\nThe error was:\n{1}";
        }
    }
}