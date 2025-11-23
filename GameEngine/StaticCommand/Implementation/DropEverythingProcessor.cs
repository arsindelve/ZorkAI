using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;

namespace GameEngine.StaticCommand.Implementation;

public class DropEverythingProcessor : IGlobalCommand
{
    public async Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        List<IItem> items = context.Items;

        if (!items.Any())
            return await client.GenerateNarration(new DropAllNothingHere(), context.SystemPromptAddendum);
        
        return DropAll(context, items!);
    }

    public static string DropAll(IContext context, List<IItem?> items)
    {
        var sb = new StringBuilder();

        foreach (var nextItem in items.ToList())
            if (nextItem is ICanBeTakenAndDropped)
                sb.AppendLine(
                    $"{nextItem.Name}: {TakeOrDropInteractionProcessor.DropIt(context, nextItem).InteractionMessage}");

        return sb.ToString();
    }

    /// <summary>
    /// Drops multiple items, providing feedback for each item including those that don't exist or can't be dropped.
    /// </summary>
    /// <param name="context">The game context containing the player's inventory and current state.</param>
    /// <param name="itemsWithNouns">A list of tuples containing the original noun from user input and the corresponding item (null if not found).</param>
    /// <returns>A formatted string with the result of attempting to drop each item.</returns>
    public static string DropAll(IContext context, List<(string noun, IItem? item)> itemsWithNouns)
    {
        var sb = new StringBuilder();

        foreach (var (noun, item) in itemsWithNouns)
        {
            if (item is null)
            {
                sb.AppendLine($"{noun}: You don't have that!");
                continue;
            }

            if (item is ICanBeTakenAndDropped)
                sb.AppendLine(
                    $"{item.Name}: {TakeOrDropInteractionProcessor.DropIt(context, item).InteractionMessage}");
        }

        return sb.ToString();
    }
}