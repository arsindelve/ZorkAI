namespace Model.AIGeneration.Requests;

public class CannotEnterSubLocationRequest : Request
{
    public CannotEnterSubLocationRequest(string location, string subLocationName)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The player is in this location: \"{location}\". They tried to enter some kind of sub-location called {subLocationName}" +
            $"that is not available from this location. Respond with a very short, sarcastic and simple message informing them " +
            $"that they cannot do this. Do not be creative about why, do not alter the state of the game or provide additional information. ";
    }
}