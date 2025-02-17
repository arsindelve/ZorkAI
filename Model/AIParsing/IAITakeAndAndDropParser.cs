namespace Model.AIParsing;

/// <summary>
/// Defines an interface for AI parsers that determine lists of items to be taken or dropped based on user inputs, location details, or inventory descriptions.
/// </summary>
public interface IAITakeAndAndDropParser
{
    /// <summary>
    /// Asynchronously retrieves a list of items that the AI parser suggests to take based on the user's input, location description, and session context.
    /// </summary>
    /// <param name="input">The user's input to be analyzed by the AI parser.</param>
    /// <param name="locationDescription">A description providing context about the current location.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is an array of strings containing the items to take.</returns>
    Task<string[]> GetListOfItemsToTake(string input, string locationDescription);

    /// <summary>
    /// Asynchronously analyzes the input along with the inventory description and session context to determine the list of items that should be dropped.
    /// </summary>
    /// <param name="input">The user's input specifying actions or commands related to dropping items.</param>
    /// <param name="inventoryDescription">A description of the user's current inventory state.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of strings representing the items to drop based on the input and inventory description.</returns>
    Task<string[]> GetListOfItemsToDrop(string input, string inventoryDescription);
}