namespace Model.AIGeneration.Requests;

public class NounNotPresentOperationRequest : Request
{
    public NounNotPresentOperationRequest(string location, string? noun)
    {
        UserMessage = $"The player is in this location: \"{location}\". " +
                      $"They reference the object: \"{noun}\", but there is no \"{noun}\" here. " +
                      $"Provide the narrator's very succinct response";
    }
}