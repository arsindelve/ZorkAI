namespace Model.AIGeneration.Requests;

public class CannotGoThatWayRequest : Request
{
    public CannotGoThatWayRequest(string location)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The player is in this location: \"{location}\". They tried to move in a direction that is " +
            $"blocked or not available from this location. Provide the narrator's response";
    }
}