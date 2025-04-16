using Stateless;
using UnityEngine.UIElements;

namespace ToolBuddy.PrintableAR
{
    /// <summary>
    /// State machine that manages UI screen transitions and visibility
    /// </summary>
    public class UIStateMachine : StateMachine<UIState, UITrigger>
    {
        private VisualElement baseUI;
        private VisualElement noModelUnderlay;
        private VisualElement loadingOverlay;
        private VisualElement errorOverlay;
        private VisualElement helpOverlay;

        public UIStateMachine(
            UIDocument uiDocument) : base(UIState.Initialization)
        {
            GetElements(uiDocument);

            SetElementsInitialVisibility();

            SetElementsInteractivity(uiDocument);

            ConfigureStateMachine();

            Fire(UITrigger.Initialized);
        }

        /// <summary>
        /// Get references to UI elements from the document
        /// </summary>
        private void GetElements(
            UIDocument uiDocument)
        {
            VisualElement rootVisualElement = uiDocument.rootVisualElement;

            // Get screen elements
            baseUI = rootVisualElement.Q<VisualElement>("base-ui");
            noModelUnderlay = rootVisualElement.Q<VisualElement>("no-model-underlay");
            loadingOverlay = rootVisualElement.Q<VisualElement>("loading-overlay");
            errorOverlay = rootVisualElement.Q<VisualElement>("error-overlay");
            helpOverlay = rootVisualElement.Q<VisualElement>("help-overlay");
        }

        private void SetElementsInitialVisibility()
        {
            baseUI.style.display = DisplayStyle.Flex;
            noModelUnderlay.style.display = DisplayStyle.None;
            loadingOverlay.style.display = DisplayStyle.None;
            errorOverlay.style.display = DisplayStyle.None;
            helpOverlay.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Set some elements to not pickable, so that UI does not block interactions with the scene.
        /// Picking Mode is settable in the UXML, but for some reason it is ignored by Unity. So I ended up setting it up here.
        /// </summary>
        /// <param name="uiDocument"></param>
        private void SetElementsInteractivity(
            UIDocument uiDocument)
        {
            VisualElement rootVisualElement = uiDocument.rootVisualElement;

            rootVisualElement.pickingMode = PickingMode.Ignore;
            rootVisualElement.Q<VisualElement>("root").pickingMode = PickingMode.Ignore;
            baseUI.pickingMode = PickingMode.Ignore;
            noModelUnderlay.pickingMode = PickingMode.Ignore;
            rootVisualElement.Q<VisualElement>("button-container").pickingMode = PickingMode.Ignore;
        }

        /// <summary>
        /// Initialize the state machine with states and transitions
        /// </summary>
        private void ConfigureStateMachine()
        {
            //todo handle model loading cancellation
            Configure(UIState.Initialization)
                .Permit(
                    UITrigger.Initialized,
                    UIState.NoModelLoaded
                );

            Configure(UIState.NoModelLoaded)
                .OnEntry(() => noModelUnderlay.style.display = DisplayStyle.Flex)
                .OnExit(() => noModelUnderlay.style.display = DisplayStyle.None)
                .Permit(
                    UITrigger.LoadModel,
                    UIState.ModelLoading
                );

            //todo handle animation
            Configure(UIState.ModelLoading)
                .OnEntry(() => loadingOverlay.style.display = DisplayStyle.Flex)
                .OnExit(() => loadingOverlay.style.display = DisplayStyle.None)
                .Permit(
                    UITrigger.ModelLoadingSuccess,
                    UIState.ModelPlacement
                )
                .Permit(
                    UITrigger.ModelLoadingError,
                    UIState.LoadingError
                );

            Configure(UIState.LoadingError)
                .OnEntryFrom(
                    SetTriggerParameters<string>(UITrigger.ModelLoadingError),
                    errorMessage => { errorOverlay.Q<Label>("error-message").text = errorMessage; }
                )
                .OnEntry(() => errorOverlay.style.display = DisplayStyle.Flex)
                .OnExit(() => errorOverlay.style.display = DisplayStyle.None)
                .Permit(
                    UITrigger.Reset,
                    UIState.NoModelLoaded
                );

            Configure(UIState.ModelPlacement)
                .Permit(
                    UITrigger.LoadModel,
                    UIState.ModelLoading
                );
        }
    }
}