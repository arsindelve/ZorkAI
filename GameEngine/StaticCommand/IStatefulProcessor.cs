using Model.Interface;

namespace GameEngine.StaticCommand;

/// <summary>
///     A stateful global command processor is one that will take over the processing of input
///     for the engine for a brief time because it needs special input from the user to process
///     the request. An example is a "quit" processor. It will need to confirm that the user really
///     wants to quit or not. If not, it will hand back execution to the engine.
/// </summary>
internal interface IStatefulProcessor : IGlobalCommand
{
    /// <summary>
    ///     This property indicates whether the stateful processor has completed its execution.
    /// </summary>
    /// <value>
    ///     True if the stateful processor has completed its execution; otherwise, false.
    /// </value>
    public bool Completed { get; }

    public bool ContinueProcessing { get; }
}