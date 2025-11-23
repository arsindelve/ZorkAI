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

    /// <summary>
    /// Takes multiple items, providing feedback for each item including those that don't exist or can't be taken.
    /// </summary>
    /// <param name="context">The game context containing the player's inventory and current location.</param>
    /// <param name="itemsWithNouns">A list of tuples containing the original noun from user input and the corresponding item (null if not found).</param>
    /// <param name="client">The generation client for AI-generated snarky responses.</param>
    /// <returns>A formatted string with the result of attempting to take each item.</returns>
    public static async Task<string> TakeAll(IContext context, List<(string noun, IItem? item)> itemsWithNouns, IGenerationClient client)
    {
        var sb = new StringBuilder();
        foreach (var (noun, item) in itemsWithNouns)
        {
            if (item is null)
            {
                var message = await client.GenerateNarration(
                    new TakeSomethingThatIsNotPortable(noun), context.SystemPromptAddendum);
                sb.AppendLine($"{noun}: {message}");
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