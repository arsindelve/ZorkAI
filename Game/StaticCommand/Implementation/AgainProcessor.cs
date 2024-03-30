namespace Game.StaticCommand.Implementation;

/// <summary>
///     Class for processing the player's input and determining if the
///     player wants to repeat the previous action.
/// </summary>
internal class AgainProcessor
{
    public (string response, bool returnResult) Process(string input, IContext context)
    {
        string[] inputMatches = ["again", "g", "go again", "do again"];
        if (!inputMatches.Contains(input.ToLowerInvariant().Trim()))
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
}