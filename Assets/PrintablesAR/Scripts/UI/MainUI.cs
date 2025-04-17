using System;
using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace ToolBuddy.PrintablesAR.UI
{
    /// <summary>
    /// C# wrapper around the MainUI uxml document
    /// </summary>
    public class MainUI
    {
        [NotNull]
        public VisualElement DocumentRoot { get; private set; }

        [NotNull]
        public VisualElement RootElement { get; private set; }

        [NotNull]
        public VisualElement BaseUi { get; private set; }

        [NotNull]
        public VisualElement NoModelUnderlay { get; private set; }

        [NotNull]
        public VisualElement LoadingOverlay { get; private set; }

        [NotNull]
        public VisualElement ErrorOverlay { get; private set; }

        [NotNull]
        public VisualElement HelpOverlay { get; private set; }

        [NotNull]
        public Label ErrorMessageLabel { get; private set; }

        [NotNull]
        public VisualElement ButtonContainer { get; private set; }

        [NotNull]
        private Button LoadButton { get; set; }

        [NotNull]
        private Button HelpButton { get; set; }

        [NotNull]
        private Button CreatorButton { get; set; }

        [NotNull]
        private Button CloseErrorButton { get; set; }

        [NotNull]
        private Button CloseHelpButton { get; set; }

        public event Action LoadButtonClicked;
        public event Action HelpButtonClicked;
        public event Action CreatorButtonClicked;
        public event Action CloseErrorButtonClicked;
        public event Action CloseHelpButtonClicked;

        public void Initialize(
            [NotNull] UIDocument uiDocument)
        {
            if (uiDocument == null)
                throw new ArgumentNullException(nameof(uiDocument));

            DocumentRoot = uiDocument.rootVisualElement;
            if (DocumentRoot == null)
                throw new NullReferenceException("Root VisualElement is null.");

            RetrieveUIElements();
            RegisterCallbacks();
        }

        [NotNull]
        private T MandatoryQ<T>(
            [NotNull] string name) where T : VisualElement
            =>
                DocumentRoot.Q<T>(name) ?? throw new InvalidOperationException($"Element with name {name} not found.");


        private void RetrieveUIElements()
        {
            RootElement = MandatoryQ<VisualElement>("root");
            BaseUi = MandatoryQ<VisualElement>("base-ui");
            NoModelUnderlay = MandatoryQ<VisualElement>("no-model-underlay");
            LoadingOverlay = MandatoryQ<VisualElement>("loading-overlay");
            ErrorOverlay = MandatoryQ<VisualElement>("error-overlay");
            ErrorMessageLabel = MandatoryQ<Label>("error-message");
            HelpOverlay = MandatoryQ<VisualElement>("help-overlay");
            LoadButton = MandatoryQ<Button>("load-button");
            HelpButton = MandatoryQ<Button>("help-button");
            CreatorButton = MandatoryQ<Button>("creator-button");
            CloseErrorButton = MandatoryQ<Button>("close-error-button");
            CloseHelpButton = MandatoryQ<Button>("close-help-button");
            ButtonContainer = MandatoryQ<VisualElement>("button-container");
        }

        private void RegisterCallbacks()
        {
            // Use ?. operator for safety
            LoadButton?.RegisterCallback<ClickEvent>(evt => LoadButtonClicked?.Invoke());
            HelpButton?.RegisterCallback<ClickEvent>(evt => HelpButtonClicked?.Invoke());
            CreatorButton?.RegisterCallback<ClickEvent>(evt => CreatorButtonClicked?.Invoke());
            CloseErrorButton?.RegisterCallback<ClickEvent>(evt => CloseErrorButtonClicked?.Invoke());
            CloseHelpButton?.RegisterCallback<ClickEvent>(evt => CloseHelpButtonClicked?.Invoke());
        }
    }
}