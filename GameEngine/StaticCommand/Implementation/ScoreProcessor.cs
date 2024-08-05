using Model.AIGeneration;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

internal class ScoreProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        return Task.FromResult(context.CurrentScore);
    }
}