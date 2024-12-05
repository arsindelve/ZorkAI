namespace Model.AIGeneration.Requests;

public class WonTheGameRequest : Request
{
    public WonTheGameRequest()
    {
        UserMessage =
            "The player has won the game. Tell the user they can restore, restart of go do something else like go play outside, or make a snack, but there is nothing else left to do in this game. Congratulate them. ";
    }
}