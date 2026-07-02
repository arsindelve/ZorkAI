using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

/// <summary>
///     Class for processing the player's input and determining if the
///     player wants to repeat the previous action.
/// </summary>
internal class AgainProcessor
{
    private static readonly string[] InputMatches = ["again", "g", "go again", "do again"];

    public (string response, bool returnResult) Process(string input, IContext context)
    {
        if (!InputMatches.Contains(input.ToLowerInvariant().Trim()))
        {
            context.Inputs.Push(input);
            return (input, false);
        }

        var lastCommand = context.Inputs.Peek();

        if (string.IsNullOrEmpty(lastCommand))
        {
            context.Inputs.Push(input);
            return ("You haven't done anything yet. \n", true);
        }

        // Replace their input in the pipeline with their last command,
        // and return false so we don't add this input to the stack
        return (lastCommand, false);
    }

    /// <summary>
    ///     Read-only lookahead at what "again"/"g" would replay, without the side effects of
    ///     <see cref="Process" /> (pushing to <see cref="IContext.Inputs" />, or returning the
    ///     "nothing to repeat" message). The engine needs to classify a replayed command (e.g. as a
    ///     free global command, issue #354) before turn processing begins - i.e. before Process()
    ///     itself runs - so it peeks the replay target here first. Returns null when the input isn't
    ///     an "again" variant, or there's nothing to replay.
    /// </summary>
    public string? PeekReplayTarget(string input, IContext context)
    {
        if (!InputMatches.Contains(input.ToLowerInvariant().Trim()))
            return null;

        var lastCommand = context.Inputs.Peek();
        return string.IsNullOrEmpty(lastCommand) ? null : lastCommand;
    }
}