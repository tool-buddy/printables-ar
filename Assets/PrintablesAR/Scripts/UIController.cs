using System;
using System.IO;
using Stateless;
using ToolBuddy.PrintableAR.ModelImporting;
using UnityEngine;
using UnityEngine.UIElements;

namespace ToolBuddy.PrintableAR
{
    /// <summary>
    /// Controls the UI for the 3D Printing Model Displayer
    /// </summary>
    public partial class UIController : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement root;

        private Button loadButton;
        private Button helpButton;
        private Button creatorButton;
        private Button closeErrorButton;

        private VisualElement helpPanel;
        private Button closeHelpButton;

        private UIStateMachine stateMachine;

        private ModelImporter modelImporter;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            modelImporter = FindFirstObjectByType<ModelImporter>();

            stateMachine = new UIStateMachine(uiDocument);
        }

        private void OnEnable()
        {
            root = uiDocument.rootVisualElement;

            loadButton = root.Q<Button>("load-button");
            helpButton = root.Q<Button>("help-button");
            creatorButton = root.Q<Button>("creator-button");
            closeErrorButton = root.Q<Button>("close-error-button");

            helpPanel = root.Q<VisualElement>("help-overlay");
            closeHelpButton = root.Q<Button>("close-help-button");

            loadButton.clicked += OnLoadButtonClicked;
            helpButton.clicked += OnHelpButtonClicked;
            creatorButton.clicked += OnCreatorButtonClicked;
            closeHelpButton.clicked += OnCloseHelpButtonClicked;
            closeErrorButton.clicked += OnCloseErrorButtonClicked;

            modelImporter.ImportSucceeded += OnModelImportSucceeded;
            modelImporter.ImportFailed += OnModelImportFailed;

            stateMachine.Activate();
        }

        private void OnDisable()
        {
            if (loadButton != null) loadButton.clicked -= OnLoadButtonClicked;
            if (helpButton != null) helpButton.clicked -= OnHelpButtonClicked;
            if (creatorButton != null) creatorButton.clicked -= OnCreatorButtonClicked;
            if (closeHelpButton != null) closeHelpButton.clicked -= OnCloseHelpButtonClicked;
            if (closeErrorButton != null) closeErrorButton.clicked -= OnCloseErrorButtonClicked;

            modelImporter.ImportSucceeded -= OnModelImportSucceeded;
            modelImporter.ImportFailed -= OnModelImportFailed;

            stateMachine.Deactivate();
        }

        #region Event Handlers

        /// <summary>
        /// Handles load button click
        /// </summary>
        private void OnLoadButtonClicked()
        {
            FilePicker.Show(
                FilePickedCallback
            );
        }

        private void FilePickedCallback(
            string path)
        {
            if (String.IsNullOrEmpty(path))
                //user cancelled file selection
                return;

            stateMachine.Fire(UITrigger.LoadModel);

            if (!File.Exists(path))
                stateMachine.Fire(
                    new StateMachine<UIState, UITrigger>.TriggerWithParameters<string>(UITrigger.ModelLoadingError),
                    String.Format(
                        ErrorMessages.FileNotExisting,
                        path
                    )
                );
            else
                try
                {
                    bool startedImporting = modelImporter.TryImport(path);
                    if (startedImporting == false)
                        //todo Add a Fire(State,Data) method
                        stateMachine.Fire(
                            new StateMachine<UIState, UITrigger>.TriggerWithParameters<string>(UITrigger.ModelLoadingError),
                            String.Format(
                                ErrorMessages.UnsupportedFileFormat,
                                Path.GetExtension(path),
                                String.Join(
                                    ", ",
                                    modelImporter.SupportedFileFormats
                                )
                            )
                        );
                }
                catch (Exception e)
                {
                    stateMachine.Fire(
                        new StateMachine<UIState, UITrigger>.TriggerWithParameters<string>(UITrigger.ModelLoadingError),
                        String.Format(
                            ErrorMessages.LoadingError,
                            path,
                            e.Message
                        )
                    );
                }
        }

        /// <summary>
        /// Handles model import errors.
        /// </summary>
        private void OnModelImportFailed(
            string errorMessage,
            string filePath)
        {
            //todo test this
            stateMachine.Fire(
                new StateMachine<UIState, UITrigger>.TriggerWithParameters<string>(
                    UITrigger.ModelLoadingError
                ),
                String.Format(
                    ErrorMessages.LoadingError,
                    filePath,
                    errorMessage
                )
            );
            //todo improve error window
        }

        /// <summary>
        /// Handles successful model import.
        /// </summary>
        private void OnModelImportSucceeded(
            GameObject loadedOBJ,
            string filePath)
        {
            stateMachine.Fire(UITrigger.ModelLoadingSuccess);
        }

        /// <summary>
        /// Closes the error overlay and returns to initial state
        /// </summary>
        private void OnCloseErrorButtonClicked()
        {
            stateMachine.Fire(UITrigger.Reset);
        }

        /// <summary>
        /// Shows help panel with controls information
        /// </summary>
        private void OnHelpButtonClicked()
        {
            helpPanel.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Closes the help panel
        /// </summary>
        private void OnCloseHelpButtonClicked()
        {
            //todo handle the go back button in the mobile interface
            helpPanel.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Opens the ToolBuddy website
        /// </summary>
        private void OnCreatorButtonClicked()
        {
            Application.OpenURL("https://toolbuddy.net");
        }

        #endregion
    }
}