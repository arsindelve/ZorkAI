using Model.AIGeneration;
using Model.Interface;

namespace Game.StaticCommand.Implementation;

public class TakeEverythingProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        var sb = new StringBuilder();
        var items = context.Items;

        foreach (var nextItem in items)
            if (!string.IsNullOrEmpty(nextItem.CannotBeTakenDescription))
                sb.AppendLine($"{nextItem.Name}: {nextItem.CannotBeTakenDescription}");

        return Task.FromResult(sb.ToString());
    }
}