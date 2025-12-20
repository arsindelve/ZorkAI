namespace Model.Intent;

/// <summary>
/// Represents the parser's determination that the user is querying their inventory
/// </summary>
public record InventoryIntent : IntentBase
{
    public static string TagName => "inventory";
}