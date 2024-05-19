using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace Game.StaticCommand.Implementation;

/// <summary>
///     Represents a processor for taking everything available in the current location.
/// </summary>
public class TakeEverythingProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        var sb = new StringBuilder();
        var items = ((ICanHoldItems)context.CurrentLocation).GetAllItemsRecursively;

        foreach (var nextItem in items)
        {
            if (!string.IsNullOrEmpty(nextItem.CannotBeTakenDescription))
            {
                sb.AppendLine($"{nextItem.Name}: {nextItem.CannotBeTakenDescription}");
                continue;
            }

            if (nextItem is ICanBeTakenAndDropped)
            {
                sb.AppendLine($"{nextItem.Name}: Taken.");
                context.ItemPlacedHere(nextItem);
            }
        }

        return Task.FromResult(sb.ToString());
    }
}