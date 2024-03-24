namespace Game.StaticCommand;

public class SimpleResponseCommand(string response) : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        return Task.FromResult(response);
    }
}