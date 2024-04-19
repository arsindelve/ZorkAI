using Model.AIGeneration;

namespace Game.StaticCommand;

public class SimpleResponseCommand(string response) : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        return Task.FromResult(response);
    }
}