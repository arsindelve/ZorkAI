namespace Game.StaticCommand.Implementation;

/// <summary>
/// Represents a processor for the "Inventory" global command.
/// </summary>
internal class InventoryProcessor : IGlobalCommand
{
    public string Process(string? input, IContext context)
    {
        return context.ItemListDescription(string.Empty);
    }
}