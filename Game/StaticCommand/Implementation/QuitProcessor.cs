using Utilities;

namespace Game.StaticCommand.Implementation;

/// <summary>
///     Represents a processor for the "quit" command in the game.
/// </summary>
internal class QuitProcessor : IStatefulProcessor
{
    private bool _firstPass = true;

    protected virtual string ReturnValue => "-1";

    protected virtual string Verb => "leave";

    public bool ContinueProcessing => false;

    /// <summary>
    ///     Represents a processor for the "quit" command in the game.
    /// </summary>
    /// <param name="input">The input string to process.</param>
    /// <param name="context">The game context.</param>
    /// <returns>The output string based on the input and game context.</returns>
    public string Process(string? input, IContext context)
    {
        if (string.IsNullOrEmpty(input)) return CancelQuitting();

        Completed = false;

        if (_firstPass)
        {
            _firstPass = false;
            return $"""
                    Your score would be 0 (total of 350 points), in {context.Moves} moves.
                    This score gives you the rank of Beginner.
                    Do you wish to {Verb} the game? (Y is affirmative): >
                    """;
        }

        switch (input.ToLowerInvariant().StripNonChars().Trim())
        {
            case "y":
            case "yes i do":
            case "yes":
            case "sure":
            case "yup":
            case "yep":
            case "yeah":
            case "ok":
            {
                Completed = true;
                return ReturnValue;
            }

            default:
                return CancelQuitting();
        }
    }

    public bool Completed { get; private set; }

    private string CancelQuitting()
    {
        Completed = true;
        return "Ok";
    }
}