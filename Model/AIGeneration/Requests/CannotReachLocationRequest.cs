namespace Model.AIGeneration.Requests;

/// <summary>
/// The player asked to travel to a named place ("go to the reactor") that is not reachable directly
/// from the current room (issue #268). Mirrors <see cref="CannotGoThatWayRequest"/>: a short, in-character
/// refusal that changes nothing about the game state.
/// </summary>
public class CannotReachLocationRequest : Request
{
    public CannotReachLocationRequest(string location, string destination)
    {
        UserMessage =
            $"The player is in this location: \"{location}\". They tried to travel to a place called \"{destination}\", " +
            $"but it is not anywhere they can reach directly from here. Respond with a very short, sarcastic and simple " +
            $"message informing them that they can't get there from here. Do not be creative about why, do not alter the " +
            $"state of the game or provide additional information. ";

        // Same "stay in room state" deflection category as the other cannot-go prompts: keep creativity low.
        Temperature = DeflectionTemperature;
    }
}
