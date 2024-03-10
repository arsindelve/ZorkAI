namespace OpenAI.Requests;

public class MissingFirstNounMultiNounOperationRequest : Request
{
    public MissingFirstNounMultiNounOperationRequest(string location, string? presentNoun, string? absentNoun, string? preposition, string verb)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            $"The player is in this location: \"{location}\". " +
            $"They wrote \"{verb} the {absentNoun} {preposition} the {presentNoun}\", and while " +
            $"there is a \"{presentNoun}\" here, there is no \"{absentNoun}\" here " +
            $"so that action has no effect on the story. Provide the narrator's response";
    }
}