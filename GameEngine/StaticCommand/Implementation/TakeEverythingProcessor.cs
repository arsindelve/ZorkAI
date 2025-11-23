using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;

namespace GameEngine.StaticCommand.Implementation;

/// <summary>
///     Represents a processor for taking everything available in the current location.
/// </summary>
public class TakeEverythingProcessor : IGlobalCommand
{
    public async Task<string> Process(
        string? input,
        IContext context,
        IGenerationClient client,
        Runtime runtime
        )
    {
        var items = ((ICanContainItems)context.CurrentLocation).GetAllItemsRecursively;

        if (!items.Any(s => s is ICanBeTakenAndDropped || !string.IsNullOrEmpty(s.CannotBeTakenDescription)))
            return await client.GenerateNarration(new TakeAllNothingHere(), context.SystemPromptAddendum);
        
        return TakeAll(context, items!);
    }

    public static string TakeAll(IContext context, List<IItem?> items)
    {
        var sb = new StringBuilder();
        foreach (var nextItem in items)
        {
            if(nextItem is null) continue;

            if (!string.IsNullOrEmpty(nextItem.CannotBeTakenDescription))
            {
                sb.AppendLine($"{nextItem.Name}: {nextItem.CannotBeTakenDescription}");
                continue;
            }

            if (nextItem is not ICanBeTakenAndDropped)
                continue;

            sb.AppendLine($"{nextItem.Name}: Taken. ");
            context.ItemPlacedHere(nextItem);
        }

        return sb.ToString();
    }

    public static string TakeAll(IContext context, List<(string noun, IItem? item)> itemsWithNouns)
    {
        var sb = new StringBuilder();
        foreach (var (noun, item) in itemsWithNouns)
        {
            if (item is null)
            {
                sb.AppendLine($"{noun}: You can't see that here.");
                continue;
            }

            if (!string.IsNullOrEmpty(item.CannotBeTakenDescription))
            {
                sb.AppendLine($"{item.Name}: {item.CannotBeTakenDescription}");
                continue;
            }

            if (item is not ICanBeTakenAndDropped)
                continue;

            sb.AppendLine($"{item.Name}: Taken. ");
            context.ItemPlacedHere(item);
        }

        return sb.ToString();
    }
}