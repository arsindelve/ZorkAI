namespace Model.AIGeneration.Requests;

public class CannotGoThatWayRequest : Request
{
    public CannotGoThatWayRequest(string location, string direction)
    {
        UserMessage =
            $"The player is in this location: \"{location}\". They tried to go {direction} but going that way is  " +
            $"not possible from this location. Respond with a very short, sarcastic and simple message informing them " +
            $"that they cannot go that way. Do not be creative about why or what is preventing them, do not alter the state of the game or provide additional information. ";
    }
}