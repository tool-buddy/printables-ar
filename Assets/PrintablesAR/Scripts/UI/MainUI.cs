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
        [NotNull]
        public VisualElement DocumentRoot { get; private set; }

        [NotNull]
        public VisualElement RootElement { get; private set; }

        [NotNull]
        public VisualElement Hud { get; private set; }

        [NotNull]
        public VisualElement MissingModelUnderlay { get; private set; }

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
            MissingModelUnderlay = MandatoryQ<VisualElement>(
                RootElement,
                "missing-model-underlay"
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
        }

        private void RegisterCallbacks()
        {
            LoadButton.RegisterCallback<ClickEvent>(evt => LoadButtonClicked?.Invoke());
            HelpButton.RegisterCallback<ClickEvent>(evt => HelpButtonClicked?.Invoke());
            CreatorButton.RegisterCallback<ClickEvent>(evt => CreatorButtonClicked?.Invoke());
            CloseErrorButton.RegisterCallback<ClickEvent>(evt => CloseErrorButtonClicked?.Invoke());
            CloseHelpButton.RegisterCallback<ClickEvent>(evt => CloseHelpButtonClicked?.Invoke());
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
    }
}