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
        private UIDocument _uiDocument;
        private VisualElement _root;

        private Button _loadButton;
        private Button _helpButton;
        private Button _creatorButton;
        private Button _closeErrorButton;

        private VisualElement _helpPanel;
        private Button _closeHelpButton;

        private UIStateMachine _stateMachine;

        private ModelImporter _modelImporter;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _modelImporter = FindFirstObjectByType<ModelImporter>();

            _stateMachine = new UIStateMachine(_uiDocument);
        }

        private void OnEnable()
        {
            _root = _uiDocument.rootVisualElement;

            _loadButton = _root.Q<Button>("load-button");
            _helpButton = _root.Q<Button>("help-button");
            _creatorButton = _root.Q<Button>("creator-button");
            _closeErrorButton = _root.Q<Button>("close-error-button");

            _helpPanel = _root.Q<VisualElement>("help-overlay");
            _closeHelpButton = _root.Q<Button>("close-help-button");

            _loadButton.clicked += OnLoadButtonClicked;
            _helpButton.clicked += OnHelpButtonClicked;
            _creatorButton.clicked += OnCreatorButtonClicked;
            _closeHelpButton.clicked += OnCloseHelpButtonClicked;
            _closeErrorButton.clicked += OnCloseErrorButtonClicked;

            _modelImporter.ImportSucceeded += OnModelImportSucceeded;
            _modelImporter.ImportFailed += OnModelImportFailed;

            _stateMachine.Activate();
        }

        private void OnDisable()
        {
            if (_loadButton != null) _loadButton.clicked -= OnLoadButtonClicked;
            if (_helpButton != null) _helpButton.clicked -= OnHelpButtonClicked;
            if (_creatorButton != null) _creatorButton.clicked -= OnCreatorButtonClicked;
            if (_closeHelpButton != null) _closeHelpButton.clicked -= OnCloseHelpButtonClicked;
            if (_closeErrorButton != null) _closeErrorButton.clicked -= OnCloseErrorButtonClicked;

            _modelImporter.ImportSucceeded -= OnModelImportSucceeded;
            _modelImporter.ImportFailed -= OnModelImportFailed;

            _stateMachine.Deactivate();
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

            _stateMachine.Fire(UITrigger.LoadModel);

            if (!File.Exists(path))
                _stateMachine.Fire(
                    new StateMachine<UIState, UITrigger>.TriggerWithParameters<string>(UITrigger.ModelLoadingError),
                    String.Format(
                        ErrorMessages.FileNotExisting,
                        path
                    )
                );
            else
                try
                {
                    bool startedImporting = _modelImporter.TryImport(path);
                    if (startedImporting == false)
                        //todo Add a Fire(State,Data) method
                        _stateMachine.Fire(
                            new StateMachine<UIState, UITrigger>.TriggerWithParameters<string>(UITrigger.ModelLoadingError),
                            String.Format(
                                ErrorMessages.UnsupportedFileFormat,
                                Path.GetExtension(path),
                                String.Join(
                                    ", ",
                                    _modelImporter.SupportedFileFormats
                                )
                            )
                        );
                }
                catch (Exception e)
                {
                    _stateMachine.Fire(
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
            _stateMachine.Fire(
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
            GameObject loadedObj,
            string filePath)
        {
            _stateMachine.Fire(UITrigger.ModelLoadingSuccess);
        }

        /// <summary>
        /// Closes the error overlay and returns to initial state
        /// </summary>
        private void OnCloseErrorButtonClicked()
        {
            _stateMachine.Fire(UITrigger.Reset);
        }

        /// <summary>
        /// Shows help panel with controls information
        /// </summary>
        private void OnHelpButtonClicked()
        {
            _helpPanel.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Closes the help panel
        /// </summary>
        private void OnCloseHelpButtonClicked()
        {
            //todo handle the go back button in the mobile interface
            _helpPanel.style.display = DisplayStyle.None;
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