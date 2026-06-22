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
        var dropped = new List<string>();

        foreach (var nextItem in items.ToList())
            if (nextItem is ICanBeTakenAndDropped)
            {
                sb.AppendLine(
                    $"{nextItem.Name}: {TakeOrDropInteractionProcessor.DropIt(context, nextItem).InteractionMessage}");
                RememberDropped(dropped, nextItem);
            }

        SetAntecedent(context, dropped);
        return sb.ToString();
    }

    /// <summary>
    /// Records the items just dropped as the "them" antecedent so a following "take them" can pick
    /// them back up. A batch drop overwrites the set wholesale rather than appending (issue #248).
    /// </summary>
    private static void RememberDropped(List<string> dropped, IItem item)
    {
        var noun = item.NounsForMatching.FirstOrDefault();
        if (!string.IsNullOrEmpty(noun))
            dropped.Add(noun);
    }

    private static void SetAntecedent(IContext context, List<string> nouns)
    {
        if (nouns.Count > 0)
            context.LastNouns = nouns.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// Drops multiple items, providing feedback for each item including those that don't exist or can't be dropped.
    /// </summary>
    /// <param name="context">The game context containing the player's inventory and current state.</param>
    /// <param name="itemsWithNouns">A list of tuples containing the original noun from user input and the corresponding item (null if not found).</param>
    /// <param name="client">The generation client for AI-generated snarky responses.</param>
    /// <returns>A formatted string with the result of attempting to drop each item.</returns>
    public static async Task<string> DropAll(IContext context, List<(string noun, IItem? item)> itemsWithNouns, IGenerationClient client)
    {
        var sb = new StringBuilder();
        var dropped = new List<string>();

        foreach (var (noun, item) in itemsWithNouns)
        {
            if (item is null)
            {
                var message = await client.GenerateNarration(
                    new DropSomethingTheyDoNotHave(noun), context.SystemPromptAddendum);
                sb.AppendLine($"{noun}: {message}");
                continue;
            }

            // Check if the item is actually in the inventory
            if (!context.Items.Contains(item))
            {
                var message = await client.GenerateNarration(
                    new DropSomethingTheyDoNotHave(noun), context.SystemPromptAddendum);
                sb.AppendLine($"{noun}: {message}");
                continue;
            }

            if (item is ICanBeTakenAndDropped)
            {
                sb.AppendLine(
                    $"{item.Name}: {TakeOrDropInteractionProcessor.DropIt(context, item).InteractionMessage}");
                RememberDropped(dropped, item);
            }
        }

        SetAntecedent(context, dropped);
        return sb.ToString();
    }
}
