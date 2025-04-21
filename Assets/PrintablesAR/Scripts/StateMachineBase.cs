using System.Diagnostics;
using Stateless;
using Stateless.Graph;
using Debug = UnityEngine.Debug;

namespace ToolBuddy.PrintablesAR
{
    /// <summary>
    /// Abstract base class for state machines.
    /// Handles printing the state machine graph, and logging its transitions, when in debug mode.
    /// </summary>
    /// <typeparam name="TState">The type representing the states.</typeparam>
    /// <typeparam name="TTrigger">The type representing the triggers.</typeparam>
    public abstract class StateMachineBase<TState, TTrigger>
        : StateMachine<TState, TTrigger> where TState : struct where TTrigger : struct
    {
        /// <summary>
        /// Constructor for the base state machine.
        /// </summary>
        /// <param name="initialState">The initial state of the state machine.</param>
        protected StateMachineBase(
            TState initialState) : base(initialState)
        {
            SetTriggerParameters();
            Configure();
            GenerateUmlDotGraph();
        }

        /// <summary>
        /// Sets up trigger parameters for the state machine.
        /// </summary>
        protected abstract void SetTriggerParameters();

        /// <summary>
        /// Configures the state machine transitions and behaviors.
        /// </summary>
        protected virtual void Configure() =>
            ConfigureStateTransitionLogging();

        [Conditional("DEBUG")]
        private void ConfigureStateTransitionLogging() =>
            OnTransitioned(
                t => { Debug.Log($"{GetType().Name}: {t.Source} -> {t.Destination}"); }
            );

        /// <summary>
        /// Generates a UML Dot Graph representation of the state machine for debugging purposes.
        /// </summary>
        [Conditional("DEBUG")]
        protected virtual void GenerateUmlDotGraph() =>
            Debug.Log(
                $@"{GetType().Name} state machine graph:

{UmlDotGraph.Format(GetInfo())}

Past the generated UML Dot Graph in http://www.webgraphviz.com/ to visualize it."
            );
    }
}