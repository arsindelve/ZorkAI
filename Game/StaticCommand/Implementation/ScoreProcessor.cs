using Model.AIGeneration;
using Model.Interface;

namespace Game.StaticCommand.Implementation;

internal class ScoreProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        return Task.FromResult($"""
                                   Your score would be {context.Score} (total of 350 points), in {context.Moves} moves.
                                   This score gives you the rank of {context.Game.GetScoreDescription(context.Score)}.
                                """);
    }
}