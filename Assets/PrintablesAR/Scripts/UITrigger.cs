namespace ToolBuddy.PrintableAR
{
    /// <summary>
    /// Triggers for state transitions
    /// </summary>
    public enum UITrigger
    {
        Initialized,
        LoadModel,
        ModelLoadingSuccess,
        ModelLoadingError,

        //todo rename
        Reset
    }
}