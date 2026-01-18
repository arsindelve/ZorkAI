using GameEngine;

namespace EscapeRoom;

/// <summary>
///     Context for the Escape Room tutorial game.
///     Tracks whether the player has escaped.
/// </summary>
public class EscapeRoomContext : Context<EscapeRoomGame>
{
    /// <summary>
    ///     Set to true when the player opens the exit door and escapes.
    /// </summary>
    public bool HasEscaped { get; set; }

    public override string CurrentScore =>
        HasEscaped
            ? $"Your score is {Score} (total of 100 points), in {Moves} moves.\nYou have escaped!"
            : $"Your score is {Score} (total of 100 points), in {Moves} moves.\nThis score gives you the rank of {Game.GetScoreDescription(Score)}.";
}
