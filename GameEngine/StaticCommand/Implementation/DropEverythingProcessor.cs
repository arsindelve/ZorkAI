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
        var sb = new StringBuilder();
        var items = context.Items;

        if (!items.Any())
            return await client.GenerateNarration(new DropAllNothingHere(), context.SystemPromptAddendum);

        foreach (var nextItem in items.ToList())
            if (nextItem is ICanBeTakenAndDropped)
                sb.AppendLine(
                    $"{nextItem.Name}: {TakeOrDropInteractionProcessor.DropIt(context, nextItem).InteractionMessage}");

        return sb.ToString();
    }
}