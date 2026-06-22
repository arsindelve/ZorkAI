namespace Model.AIGeneration.Requests;

/// <summary>
/// The player ran several commands together on one line without separating them with periods
/// (e.g. "look examine bulkhead open bulkhead"). We do not execute these. The narrator, in
/// character, tells the player they can only do one thing at a time and that multiple commands
/// must be separated with periods.
/// </summary>
public class MultipleCommandsRequest : Request
{
    public MultipleCommandsRequest(string location, string? command)
    {
        UserMessage =
            @$"The player is in this location: ""{location}"". They wrote ""{command}"", which is several " +
            @"separate commands strung together on a single line. In the voice of the game's narrator, briefly " +
            @"and in-character, tell the player that they can only do one thing at a time, and that if they want " +
            @"to enter more than one command they should separate them with periods (for example: " +
            @"""examine bulkhead. open bulkhead.""). Do not attempt to carry out any of the commands, and do not " +
            "change the state of the game. " + NoInventionGuard;

        Temperature = DeflectionTemperature;
    }
}
