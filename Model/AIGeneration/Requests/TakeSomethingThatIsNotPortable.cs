namespace Model.AIGeneration.Requests;

public class TakeSomethingThatIsNotPortable : Request
{
    public TakeSomethingThatIsNotPortable(string input)
    {
        UserMessage =
            $"The player said '{input}', but the thing they asked to take is not portable and cannot be taken. Provide the narrator's very succinct but sarcastic response";
    }
}