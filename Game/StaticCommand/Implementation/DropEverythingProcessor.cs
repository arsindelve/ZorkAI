using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace Game.StaticCommand.Implementation;

public class DropEverythingProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        var sb = new StringBuilder();
        var items = context.Items;

        foreach (var nextItem in items.ToList())
            if (nextItem is ICanBeTakenAndDropped)
            {
                sb.AppendLine($"{nextItem.Name}: Dropped.");
                ((ICanHoldItems)context.CurrentLocation).ItemPlacedHere(nextItem);
            }

        return Task.FromResult(sb.ToString());
    }
}