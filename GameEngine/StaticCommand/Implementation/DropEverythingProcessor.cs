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
        var dropped = new List<string?>();

        foreach (var nextItem in items.ToList())
            if (nextItem is ICanBeTakenAndDropped)
            {
                sb.AppendLine(
                    $"{nextItem.Name}: {TakeOrDropInteractionProcessor.DropIt(context, nextItem).InteractionMessage}");
                dropped.Add(nextItem.NounsForMatching.FirstOrDefault());
            }

        // The items just dropped become the "them" antecedent so a following "take them" can pick
        // them back up; a batch drop overwrites the set wholesale rather than appending (issue #248).
        context.RememberAntecedentNouns(dropped);
        return sb.ToString();
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

        foreach (var (noun, item) in itemsWithNouns)
        {
            if (item is null)
            {
                var message = await client.GenerateNarration(
                    new DropSomethingTheyDoNotHave(noun), context.SystemPromptAddendum);
                sb.AppendLine($"{noun}: {message}");
                continue;
            }

            // Issue #362: use the canonical possession check (walks the containment hierarchy) rather
            // than a flat context.Items.Contains, which misses an item nested inside a worn/open
            // container - the same class of bug fixed for the single-item drop/give/show call sites.
            if (!Repository.IsItemPossessedBy(item, context))
            {
                var message = await client.GenerateNarration(
                    new DropSomethingTheyDoNotHave(noun), context.SystemPromptAddendum);
                sb.AppendLine($"{noun}: {message}");
                continue;
            }

            if (item is ICanBeTakenAndDropped)
                // DropIt records each item in the "them" group (append), so a "drop X and Y" command
                // extends the player's contiguous drop run rather than replacing it (issue #248).
                sb.AppendLine(
                    $"{item.Name}: {TakeOrDropInteractionProcessor.DropIt(context, item).InteractionMessage}");
        }

        return sb.ToString();
    }
}
