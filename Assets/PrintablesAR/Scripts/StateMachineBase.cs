using JetBrains.Annotations;
using Stateless;
using Stateless.Graph;

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
        }

        /// <summary>
        /// Sets up trigger parameters for the state machine.
        /// </summary>
        protected abstract void SetTriggerParameters();

        /// <summary>
        /// Configures the state machine transitions and behaviors.
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Generates a Mermaid diagram representation of the state machine.
        /// </summary>
        /// <returns>A string containing the Mermaid diagram.</returns>
        [UsedImplicitly] //Used by the Tools/UpdateStateDiagram.csproj
        public string GetMermaidDiagram() =>
            MermaidGraph.Format(GetInfo());
    }
}