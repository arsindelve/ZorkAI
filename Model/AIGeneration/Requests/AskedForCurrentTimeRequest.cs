namespace Model.AIGeneration.Requests;

/// <summary>
/// Represents a request where the player asks for the current time.
/// </summary>
/// <remarks>
/// This request is used to handle situations where the player inquires about the current time,
/// which is irrelevant in the context of the game. The narrator is expected to provide a succinct
/// but sarcastic response to indicate that the time is not a concern.
/// </remarks>
public class AskedForCurrentTimeRequest : Request
{
    public AskedForCurrentTimeRequest()
    {
        UserMessage =
            "The player asked what time it is, but that has no meaning in this game, and they should not be worried about that. Provide the narrator's succinct but sarcastic response";
    }
}