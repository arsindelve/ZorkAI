namespace Game.StaticCommand.Implementation;

internal class ScoreProcessor : IGlobalCommand
{
    public string Process(string? input, IContext context)
    {
        return $"""
                   Your score would be {context.Score} (total of 350 points), in {context.Moves} moves.
                   This score gives you the rank of {GetScoreDescription(context.Score)}.
                """;
    }

    public string GetScoreDescription(int score)
    {
        if (score < 25)
            return "Beginner";
        if (score < 50)
            return "Amateur Adventurer";
        if (score < 100)
            return "Novice Adventurer";
        if (score < 200)
            return "Junior Adventurer";
        if (score < 300)
            return "Adventurer";
        if (score < 330)
            return "Master";
        if (score < 350)
            return "Wizard";
        return "Master Adventurer";
    }

    // https://ganelson.github.io/inform-website/book/WI_9_3.html
}