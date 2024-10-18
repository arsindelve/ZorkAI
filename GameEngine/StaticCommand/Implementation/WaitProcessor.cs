using Model.AIGeneration;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

internal class WaitProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        context.CurrentLocation.OnWaiting(context);
        return Task.FromResult("Time passes...\n\n");
    }
}