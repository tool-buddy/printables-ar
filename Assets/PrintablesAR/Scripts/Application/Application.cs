using System;
using System.IO;
using JetBrains.Annotations;
using ToolBuddy.PrintablesAR.ARInteraction;
using ToolBuddy.PrintablesAR.ModelImporting;
using ToolBuddy.PrintablesAR.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using static ToolBuddy.PrintablesAR.Application.ApplicationStateMachine;

namespace ToolBuddy.PrintablesAR.Application
{
    /// <summary>
    ///     Controls the UI for the 3D Printing Model Displayer
    /// </summary>
    public class Application : MonoBehaviour
    {
        [NotNull]
        private readonly MainUI _mainUI = new MainUI();

        private readonly ApplicationStateMachine _stateMachine = new ApplicationStateMachine();

        private ARInteractableInstantiator _interactableInstantiator;

        private ModelImporter _modelImporter;

        private UIController _uiController;


        #region Unity callbacks

        private void Awake()
        {
            _mainUI.Initialize(FindFirstObjectByType<UIDocument>());

            if (!EnhancedTouchSupport.enabled) EnhancedTouchSupport.Enable();

            _modelImporter = FindFirstObjectByType<ModelImporter>();

            _interactableInstantiator = new ARInteractableInstantiator(
                _modelImporter,
                _stateMachine,
                new Raycaster(
                    FindFirstObjectByType<ARRaycastManager>(),
                    _mainUI
                )
            );

            _interactableInstantiator.InteractableInstantiated += OnInteractableInstantiated;

            _uiController = new UIController(
                _stateMachine,
                _mainUI
            );

            _uiController.Initialize();

            _stateMachine.Configure(ApplicationState.Quitting)
                .OnEntry(UnityEngine.Application.Quit);

            _stateMachine.Fire(Trigger.ApplicationInitialized);
        }

        private void OnEnable()
        {
            _mainUI.LoadButtonClicked += OnLoadButtonClicked;

            _mainUI.CreatorButtonClicked += OnCreatorButtonClicked;
            _mainUI.HelpButtonClicked += OnHelpButtonClicked;
            _mainUI.CloseHelpButtonClicked += OnPopUpCloseButtonClicked;
            _mainUI.CloseErrorButtonClicked += OnPopUpCloseButtonClicked;

            _modelImporter.ImportSucceeded += OnModelImportSucceeded;
            _modelImporter.ImportFailed += OnModelImportFailed;
        }

        private void OnDisable()
        {
            _mainUI.LoadButtonClicked -= OnLoadButtonClicked;

            _mainUI.CreatorButtonClicked -= OnCreatorButtonClicked;
            _mainUI.HelpButtonClicked -= OnHelpButtonClicked;
            _mainUI.CloseHelpButtonClicked -= OnPopUpCloseButtonClicked;
            _mainUI.CloseErrorButtonClicked -= OnPopUpCloseButtonClicked;

            _modelImporter.ImportSucceeded -= OnModelImportSucceeded;
            _modelImporter.ImportFailed -= OnModelImportFailed;
        }

        private void OnDestroy()
        {
            _interactableInstantiator.InteractableInstantiated -= OnInteractableInstantiated;
            _interactableInstantiator.Dispose();
        }


        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame
                && _stateMachine.CanFire(Trigger.BackButtonPressed))
                _stateMachine.Fire(Trigger.BackButtonPressed);
        }

        #endregion

        private void OnInteractableInstantiated(
            GameObject obj) =>
            _stateMachine.Fire(Trigger.ModelSpawned);

        /// <summary>
        ///     Handles load button click
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
        ///     Handles model import errors.
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
        ///     Handles successful model import.
        /// </summary>
        private void OnModelImportSucceeded(
            GameObject loadedObj,
            string filePath) =>
            _stateMachine.Fire(Trigger.ModelLoadingSuccess);

        /// <summary>
        ///     Opens the ToolBuddy website
        /// </summary>
        private void OnCreatorButtonClicked() =>
            UnityEngine.Application.OpenURL("https://toolbuddy.net");

        private void OnPopUpCloseButtonClicked() =>
            _stateMachine.Fire(Trigger.CloseButtonPressed);

        private void OnHelpButtonClicked() =>
            _stateMachine.Fire(Trigger.HelpButtonPressed);
    }
}