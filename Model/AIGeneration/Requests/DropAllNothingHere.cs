namespace Model.AIGeneration.Requests;

public class DropAllNothingHere : Request
{
    public DropAllNothingHere()
    {
        UserMessage =
            "The player asked to 'drop everything', but they don't have anything to drop. Provide the narrator's very succinct but sarcastic response";
    }
}