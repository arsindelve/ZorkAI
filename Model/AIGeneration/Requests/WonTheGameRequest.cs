namespace Model.AIGeneration.Requests;

public class WonTheGameRequest : Request
{
    public WonTheGameRequest(int variation = 0)
    {
        UserMessage =
            $"The player has won the game. Tell the user they can restore, restart or go do something else like go play outside, or make a snack, but there is nothing else left to do in this game. Congratulate them. (Variation seed: {variation} - use this to vary your response style and wording)";
    }
}