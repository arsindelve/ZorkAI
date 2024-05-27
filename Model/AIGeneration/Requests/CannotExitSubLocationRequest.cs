namespace Model.AIGeneration.Requests;

public class CannotExitSubLocationRequest : Request
{
    public CannotExitSubLocationRequest(string location, string subLocationName)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The player is in this location: \"{location}\". They tried to leave some kind of sub-location called {subLocationName}" +
            $"Respond with a very short, sarcastic and simple message informing them " +
            $"that they cannot do this. Do not be creative about why, do not alter the state of the game or provide additional information. ";
    }
}