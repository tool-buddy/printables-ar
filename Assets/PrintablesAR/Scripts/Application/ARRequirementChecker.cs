using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine.XR.ARFoundation;

namespace ToolBuddy.PrintablesAR.Application
{
    /// <summary>
    /// A class responsible for checking AR hardware and software requirements.
    /// </summary>
    public class ARRequirementChecker
    {
        [NotNull]
        private readonly ARSession _session;

        [NotNull]
        private readonly ApplicationStateMachine _stateMachine;

        private bool IsHardwareAvailable => ARSession.state >= ARSessionState.NeedsInstall;

        private bool IsSoftwareAvailable => ARSession.state >= ARSessionState.Ready;

        /// <summary>
        /// Initializes a new instance of the <see cref="ARRequirementChecker"/> class.
        /// </summary>
        /// <param name="stateMachine">The application's state machine.</param>
        /// <param name="session">The AR session.</param>
        public ARRequirementChecker(
            [NotNull] ApplicationStateMachine stateMachine,
            [NotNull] ARSession session)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        /// <summary>
        /// Starts the process of checking for AR availability and initializes the session.
        /// </summary>
        /// <returns>An enumerator that can be used to run this as a coroutine.</returns>
        public IEnumerator Initialize()
        {
            // ARSession needs to be disables to be able to provide the ARSessionState.Unsupported state. Otherwise, it goes directly to ARSessionState.SessionInitializing.
            _session.enabled = false;

            _stateMachine.Configure(ApplicationStateMachine.ApplicationState.CheckingSoftware)
                .OnExit(
                    () =>
                        _session.enabled = true
                );


            yield return ARSession.CheckAvailability();

            if (_stateMachine.IsInState(ApplicationStateMachine.ApplicationState.CheckingHardware)
                && IsHardwareAvailable)
                _stateMachine.Fire(
                    ApplicationStateMachine.Trigger.RequiredHardwareFound
                );

            if (_stateMachine.IsInState(ApplicationStateMachine.ApplicationState.CheckingSoftware))
            {
                if (ARSession.state == ARSessionState.NeedsInstall)
                    yield return ARSession.Install();


                if (IsSoftwareAvailable)
                    _stateMachine.Fire(
                        ApplicationStateMachine.Trigger.RequiredSoftwareFound
                    );
            }
        }
    }
}