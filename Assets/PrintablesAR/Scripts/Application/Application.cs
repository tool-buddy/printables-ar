using System;
using System.IO;
using JetBrains.Annotations;
using ToolBuddy.PrintablesAR.ARInteraction;
using ToolBuddy.PrintablesAR.ModelImporting;
using ToolBuddy.PrintablesAR.UI;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UIElements;
using static ToolBuddy.PrintablesAR.Application.ApplicationStateMachine;

namespace ToolBuddy.PrintablesAR.Application
{
    /// <summary>
    /// Controls the UI for the 3D Printing Model Displayer
    /// </summary>
    public class Application : MonoBehaviour
    {
        private readonly ApplicationStateMachine _stateMachine = new ApplicationStateMachine();


        [NotNull]
        private readonly MainUI _mainUI = new MainUI();

        private UIController _uiController;

        private ModelImporter _modelImporter;

        private ARInteractableInstantiator _interactableInstantiator;


        private void Awake()
        {
            if (!EnhancedTouchSupport.enabled) EnhancedTouchSupport.Enable();

            _modelImporter = FindFirstObjectByType<ModelImporter>();
            _interactableInstantiator = new ARInteractableInstantiator(
                _modelImporter,
                _stateMachine
            );
            _mainUI.Initialize(FindFirstObjectByType<UIDocument>());
            _uiController = new UIController(
                _stateMachine,
                _mainUI
            );
            _uiController.Initialize();
            _stateMachine.Fire(Trigger.ApplicationInitialized);
        }

        private void OnEnable()
        {
            _mainUI.LoadButtonClicked += OnLoadButtonClicked;

            _mainUI.CreatorButtonClicked += OnCreatorButtonClicked;
            _mainUI.CloseErrorButtonClicked += OnCloseErrorButtonClicked;

            _modelImporter.ImportSucceeded += OnModelImportSucceeded;
            _modelImporter.ImportFailed += OnModelImportFailed;
        }

        private void OnDisable()
        {
            _mainUI.LoadButtonClicked -= OnLoadButtonClicked;

            _mainUI.CreatorButtonClicked -= OnCreatorButtonClicked;
            _mainUI.CloseErrorButtonClicked -= OnCloseErrorButtonClicked;

            _modelImporter.ImportSucceeded -= OnModelImportSucceeded;
            _modelImporter.ImportFailed -= OnModelImportFailed;
        }

        private void OnDestroy()
        {
            _interactableInstantiator.Dispose();
        }


        /// <summary>
        /// Handles load button click
        /// </summary>
        private void OnLoadButtonClicked() =>
            FilePicker.Show(
                FilePickedCallback
            );

        private void FilePickedCallback(
            string path)
        {
            if (String.IsNullOrEmpty(path))
                //user cancelled file selection
                return;

            _stateMachine.Fire(Trigger.ModelLoadingStarted);

            if (!File.Exists(path))
                _stateMachine.Fire(
                    _stateMachine.ModelLoadingErrorTrigger,
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
                            _stateMachine.ModelLoadingErrorTrigger,
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
                        _stateMachine.ModelLoadingErrorTrigger,
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
            string filePath) =>
            _stateMachine.Fire(
                _stateMachine.ModelLoadingErrorTrigger,
                String.Format(
                    ErrorMessages.LoadingError,
                    filePath,
                    errorMessage
                )
            );

        /// <summary>
        /// Handles successful model import.
        /// </summary>
        private void OnModelImportSucceeded(
            GameObject loadedObj,
            string filePath) =>
            _stateMachine.Fire(Trigger.ModelLoadingSuccess);

        /// <summary>
        /// Closes the error overlay and returns to initial state
        /// </summary>
        private void OnCloseErrorButtonClicked() =>
            _stateMachine.Fire(Trigger.Reset);


        /// <summary>
        /// Opens the ToolBuddy website
        /// </summary>
        private void OnCreatorButtonClicked() =>
            UnityEngine.Application.OpenURL("https://toolbuddy.net");
    }
}