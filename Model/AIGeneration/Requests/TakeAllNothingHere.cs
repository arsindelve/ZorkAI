namespace Model.AIGeneration.Requests;

public class TakeAllNothingHere : Request
{
    public TakeAllNothingHere()
    {
        UserMessage =
            "The player asked to 'take everything', but there is nothing here to take. Provide the narrator's very succinct but sarcastic response";
    }
}
