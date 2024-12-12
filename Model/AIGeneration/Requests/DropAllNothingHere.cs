namespace Model.AIGeneration.Requests;

/// <summary>
/// Represents a request where the player attempts to drop all items, but has no items to drop.
/// </summary>
public class DropAllNothingHere : Request
{
    public DropAllNothingHere()
    {
        UserMessage =
            "The player asked to 'drop everything', but they don't have anything to drop. Provide the narrator's very succinct but sarcastic response";
    }
}