using Model.AIGeneration;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

/// <summary>
///     Represents a processor for the "Inventory" global command.
/// </summary>
internal class InventoryProcessor : IFreeGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        return Task.FromResult(context.ItemListDescription(string.Empty + Environment.NewLine, null));
    }
}