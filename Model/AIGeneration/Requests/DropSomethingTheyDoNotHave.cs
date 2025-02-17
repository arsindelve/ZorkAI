namespace Model.AIGeneration.Requests;

public sealed class DropSomethingTheyDoNotHave : Request
{
    public DropSomethingTheyDoNotHave(string input)
    {
        UserMessage =
            $"The player said '{input}', but the thing or things they asked to drop is/are not in their inventory. Provide the narrator's very succinct but sarcastic response";
    }
}