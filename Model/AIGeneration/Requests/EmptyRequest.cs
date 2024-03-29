namespace Model.AIGeneration.Requests;

public class EmptyRequest : Request
{
    public EmptyRequest()
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            "The player entered no command by accident. Provide the narrator's very succinct but sarcastic response";
    }
}