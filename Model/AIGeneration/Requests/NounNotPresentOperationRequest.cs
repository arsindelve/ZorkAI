namespace Model.AIGeneration.Requests;

public class NounNotPresentOperationRequest : Request
{
    public NounNotPresentOperationRequest(string location, string? noun)
    {
        UserMessage = $"The player is in this location: \"{location}\". " +
                      $"They reference the object: \"{noun}\", but there is no \"{noun}\" here. " +
                      $"Provide the narrator's very succinct response telling them it isn't here. " +
                      // Anti-hallucination guard: the narrator must stay inside the room state and
                      // not substitute a made-up object for the one the player named.
                      "Do not invent, name, substitute, or describe any object, item, scenery, exit, " +
                      "or character that is not already explicitly present in the location description " +
                      "above. Refer only to things actually present, or keep it generic. Do not alter " +
                      "the state of the game or add new information.";

        // Deflection responses should not embellish; keep creativity low to avoid invented detail.
        Temperature = 0.4f;
    }
}