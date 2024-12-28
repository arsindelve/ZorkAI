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
        var sb = new StringBuilder();
        var items = ((ICanHoldItems)context.CurrentLocation).GetAllItemsRecursively;

        if (!items.Any(s => s is ICanBeTakenAndDropped || !string.IsNullOrEmpty(s.CannotBeTakenDescription)))
            return await client.GenerateNarration(new TakeAllNothingHere());

        foreach (var nextItem in items)
        {
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
}
