namespace Game.StaticCommand.Implementation;

internal class AgainProcessor
{
    public (string?, bool) Process(string input, IContext context)
    {
        string[] inputMatches = ["again", "g", "go again", "do again"];
        if (!inputMatches.Contains(input.ToLowerInvariant().Trim()))
            return (input, true);

        return (context.Inputs.Last(), false);
    }
}