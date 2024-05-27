namespace Model.AIGeneration.Requests;

public class CannotGoThatWayRequest : Request
{
    public CannotGoThatWayRequest(string location)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The player is in this location: \"{location}\". They tried to move in a direction that is " +
            $"not available from this location. Respond with a very short, sarcastic and simple message informing them " +
            $"that they cannot go that way. Do not be creative about why, do not alter the state of the game or provide additional information. ";
    }
}