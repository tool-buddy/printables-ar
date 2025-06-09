using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace ToolBuddy.PrintablesAR.UI
{
    /// <summary>
    /// C# wrapper around the MainUI uxml document
    /// </summary>
    public class MainUI
    {
        public VisualElement DocumentRoot { get; private set; }

        public VisualElement RootElement { get; private set; }

        public VisualElement Hud { get; private set; }

        public VisualElement HintLayer { get; private set; }
        public Label HintMainLabel { get; private set; }
        public Label HintSubLabel { get; private set; }

        public VisualElement LoadingOverlay { get; private set; }

        public VisualElement ErrorOverlay { get; private set; }

        public VisualElement HelpOverlay { get; private set; }

        public Label ErrorMessageLabel { get; private set; }

        public VisualElement LoadingSpinner { get; private set; }

        public VisualElement ButtonContainer { get; private set; }

        private Button LoadButton { get; set; }

        private Button HelpButton { get; set; }

        private Button CreatorButton { get; set; }

        private Button CloseErrorButton { get; set; }

        private Button CloseHelpButton { get; set; }

        public event Action LoadButtonClicked;
        public event Action HelpButtonClicked;
        public event Action CreatorButtonClicked;
        public event Action CloseErrorButtonClicked;
        public event Action CloseHelpButtonClicked;
        public event Action ButtonClicked;

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
            [NotNull] VisualElement visualElement,
            [NotNull] string name) where T : VisualElement
            =>
                visualElement.Q<T>(name) ?? throw new InvalidOperationException($"Element with name {name} not found.");


        private void RetrieveUIElements()
        {
            RootElement = MandatoryQ<VisualElement>(
                DocumentRoot,
                "root"
            );

            Hud = MandatoryQ<VisualElement>(
                RootElement,
                "hud"
            );
            HintLayer = MandatoryQ<VisualElement>(
                RootElement,
                "hint-underlay"
            );
            HintMainLabel = MandatoryQ<Label>(
                HintLayer,
                "main-message"
            );
            HintSubLabel = MandatoryQ<Label>(
                HintLayer,
                "sub-message"
            );

            LoadingOverlay = MandatoryQ<VisualElement>(
                RootElement,
                "loading-overlay"
            );

            ErrorOverlay = MandatoryQ<VisualElement>(
                RootElement,
                "error-overlay"
            );
            HelpOverlay = MandatoryQ<VisualElement>(
                RootElement,
                "help-overlay"
            );

            LoadButton = MandatoryQ<Button>(
                Hud,
                "load-button"
            );
            HelpButton = MandatoryQ<Button>(
                Hud,
                "help-button"
            );
            CreatorButton = MandatoryQ<Button>(
                Hud,
                "creator-button"
            );
            ButtonContainer = MandatoryQ<VisualElement>(
                Hud,
                "button-container"
            );

            CloseHelpButton = MandatoryQ<Button>(
                HelpOverlay,
                "close-help-button"
            );

            CloseErrorButton = MandatoryQ<Button>(
                ErrorOverlay,
                "close-error-button"
            );

            ErrorMessageLabel = MandatoryQ<Label>(
                ErrorOverlay,
                "error-message"
            );

            LoadingSpinner = MandatoryQ<VisualElement>(
                LoadingOverlay,
                "loading-spinner"
            );
        }

        private void RegisterCallbacks()
        {
            LoadButton.RegisterCallback<ClickEvent>(_ => LoadButtonClicked?.Invoke());
            HelpButton.RegisterCallback<ClickEvent>(_ => HelpButtonClicked?.Invoke());
            CreatorButton.RegisterCallback<ClickEvent>(_ => CreatorButtonClicked?.Invoke());
            CloseErrorButton.RegisterCallback<ClickEvent>(_ => CloseErrorButtonClicked?.Invoke());
            CloseHelpButton.RegisterCallback<ClickEvent>(_ => CloseHelpButtonClicked?.Invoke());

            foreach (Button button in DocumentRoot.Query<Button>().ToList())
                button.RegisterCallback<ClickEvent>(_ => ButtonClicked?.Invoke());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="transitionDuration">in milliseconds, should be strictly positive</param>
        public void ShowLayer(
            [NotNull] VisualElement layer,
            int transitionDuration)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));
            if (transitionDuration <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(transitionDuration),
                    "Transition duration must be strictly positive."
                );
            if (layer.style.display == DisplayStyle.Flex)
                throw new InvalidOperationException(
                    "Layer is already visible. Cannot show it again."
                );

            //layer visibility
            layer.style.display = DisplayStyle.Flex;
            layer.style.opacity = 0;
            ValueAnimation<float> opacityAnimation = layer.experimental.animation.Start(
                layer.style.opacity.value,
                1,
                transitionDuration,
                (
                    element,
                    value) =>
                {
                    element.style.opacity = value;
                }
            );
            opacityAnimation.easingCurve = Easing.OutQuad;

            //child scale
            VisualElement firstChild = layer.Children().FirstOrDefault();
            if (firstChild != null)
            {
                firstChild.transform.scale = Vector3.zero;
                ValueAnimation<float> scaleAnimation = firstChild.experimental.animation.Scale(
                    1,
                    transitionDuration
                );
                scaleAnimation.easingCurve = Easing.OutQuad;
            }
        }

        public bool IsLayerShown(
            [NotNull] VisualElement layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            return layer.style.display == DisplayStyle.Flex;
        }

        public void HideLayer(
            [NotNull] VisualElement layer,
            int transitionDuration
        )
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));
            if (transitionDuration <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(transitionDuration),
                    "Transition duration must be strictly positive."
                );

            if (layer.style.display == DisplayStyle.None)
                throw new InvalidOperationException(
                    "Layer is already hidden. Cannot hide it again."
                );

            //layer visibility
            ValueAnimation<float> opacityAnimation = layer.experimental.animation.Start(
                layer.style.opacity.value,
                0,
                transitionDuration,
                (
                    element,
                    value) =>
                {
                    element.style.opacity = value;
                }
            );
            opacityAnimation.easingCurve = Easing.OutQuad;
            opacityAnimation.onAnimationCompleted = () => layer.style.display = DisplayStyle.None;

            //child scale
            VisualElement firstChild = layer.Children().FirstOrDefault();
            if (firstChild != null)
            {
                ValueAnimation<float> scaleAnimation = firstChild.experimental.animation.Scale(
                    0,
                    transitionDuration
                );
                scaleAnimation.easingCurve = Easing.OutQuad;
            }
        }


        public bool IsFingerOnUI(
            Vector2 touchPosition)
        {
            Vector2 convertedFingerPosition =
                new Vector2(
                    touchPosition.x,
                    Screen.height - touchPosition.y
                );
            Vector2 testedPosition = RuntimePanelUtils.ScreenToPanel(
                DocumentRoot.panel,
                convertedFingerPosition
            );
            return DocumentRoot.panel.Pick(testedPosition) != null;
        }

        public void SetHint(
            string message,
            string subMessage)
        {
            HintMainLabel.text = message;
            HintSubLabel.text = subMessage;
        }
    }
}