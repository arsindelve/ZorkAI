namespace Model.AIGeneration.Requests;

/// <summary>
/// Engine safety-net request (issue #271). Used when an unhandled exception is caught at the
/// turn-processing chokepoint. The narrator briefly and in-character acknowledges that something
/// went wrong and the player's action could not be completed — without revealing the error and
/// without inventing or altering any world state. Follows the existing deflection pattern
/// (<see cref="CommandHasNoEffectOperationRequest" />): low <see cref="Request.Temperature" /> and
/// the shared anti-hallucination guard.
/// </summary>
public class EngineErrorRequest : Request
{
    public EngineErrorRequest()
    {
        UserMessage =
            "Something went wrong inside the game and the player's last action could not be completed. " +
            "Provide a short, in-character narrator response, in the voice and tone of the story, that " +
            "acknowledges the action could not be carried out and gently invites the player to try " +
            "something else. Do not mention errors, bugs, exceptions, code, or anything technical, and " +
            "do not blame the player. Keep it to one or two sentences. " + NoInventionGuard;

        Temperature = DeflectionTemperature;
    }
}
