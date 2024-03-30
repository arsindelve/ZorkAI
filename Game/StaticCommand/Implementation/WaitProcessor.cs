using Model.AIGeneration;

namespace Game.StaticCommand.Implementation;

internal class WaitProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        return Task.FromResult("Time passes...");
    }
}