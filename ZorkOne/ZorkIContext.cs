using GameEngine;
using Model.Interface;
using Model.Location;
using Newtonsoft.Json;
using ZorkOne.Command;
using ZorkOne.Location;

namespace ZorkOne;

/// <summary>
///     This extends the context with properties that are specific to Zork, and would not apply to other
///     Infocom games. Other Infocom games might not have combat and wounds, for example.
/// </summary>
public class ZorkIContext : Context<ZorkI>
{
    /// <summary>
    ///     Gets or sets the Death Counter of the game.
    /// </summary>
    /// <remarks>
    ///     The Death Counter keeps track of the number of times the player character has died.
    /// </remarks>
    public int DeathCounter { get; set; }

    /// <summary>
    ///     True once the player has died "as a spirit" — i.e. after visiting the altar, death turns them
    ///     into a ghost at the Entrance to Hades instead of reincarnating them in the forest. While a
    ///     spirit, most verbs are overridden (see <see cref="SpiritProcessor" />) and the player is
    ///     always-lit; resurrection comes from praying at the altar. (Original ZIL: the DEAD flag.)
    /// </summary>
    public bool IsDead { get; set; }

    /// <summary>
    ///     True once the player has died permanently — killed while already a spirit, or a third death.
    ///     The original game would FINISH here (offer restart/restore/quit). This engine has no such
    ///     terminal state, so instead every subsequent command is intercepted with a "the adventure is
    ///     over" message (see <see cref="InterceptPlayerCommand" />), which is the closest faithful
    ///     equivalent: the player can no longer act — not even pray their way back — but system commands
    ///     (save/restore/quit) still work, just as the original's end-of-game prompt allowed (issue #17).
    /// </summary>
    public bool HasPermanentlyDied { get; set; }

    /// <summary>
    ///     Has the player ever entered the altar room (South Temple)? This is the trigger for the
    ///     spirit-death mechanic. No new flag is needed — every location already tracks how many times
    ///     it has been visited, so a non-zero visit count means "touched" (issue #17).
    /// </summary>
    public bool HasVisitedAltar => Repository.GetLocation<Altar>().VisitCount > 0;

    /// <summary>
    ///     Source of randomness for the grue. Mockable so darkness deaths are deterministic in tests.
    /// </summary>
    [JsonIgnore]
    public IRandomChooser Chooser { get; set; } = new RandomChooser();

    // Snapshot of where we were (and whether it was dark) at the start of the current turn,
    // captured in ProcessBeginningOfTurn and read in ProcessEndOfTurn to detect movement
    // through darkness. Transient (not serialized): it is re-captured every turn.
    private ILocation? _locationAtTurnStart;
    private bool _inDarknessAtTurnStart;

    /// <summary>
    ///     This flag is set once the score hits 350. The score can still go down again, if you take
    ///     something out of the case, but this flag will never get turned off.
    /// </summary>
    public bool GameOver { get; set; }

    public override string CurrentScore =>
        $"""
            Your score would be {Score} (total of 350 points), in {Moves} moves.
            This score gives you the rank of {Game.GetScoreDescription(Score)}.
         """;

    /// <summary>
    ///     The player can only have one light wound. A second will kill them. This counter tracks how much
    ///     longer until they heal. At zero, they are healed and the wound is gone.
    /// </summary>
    public int LightWoundCounter { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the player has a weapon.
    /// </summary>
    /// <remarks>
    ///     The HasWeapon property returns true if the player has a weapon in the ZorkIContext.
    ///     It checks if the GetWeapon() method returns a non-null value, indicating the
    ///     presence of a weapon.
    /// </remarks>
    /// <returns>
    ///     Returns true if the player has a weapon; otherwise, false.
    /// </returns>
    public bool HasWeapon => GetWeapon() != null;

    /// <summary>
    ///     When stunned, their next attack in combat is ineffective.
    /// </summary>
    public bool IsStunned { get; set; }

    public IItem? GetWeapon()
    {
        return Items.OfType<IWeapon>().Cast<IItem>().FirstOrDefault();
    }

    public override string? ProcessBeginningOfTurn()
    {
        if (LightWoundCounter > 0)
            LightWoundCounter--;

        // Remember where we are (and whether it's pitch black) before this turn's command runs,
        // so ProcessEndOfTurn can tell whether we moved from one dark place into another.
        _locationAtTurnStart = CurrentLocation;
        _inDarknessAtTurnStart = ItIsDarkHere;

        return base.ProcessBeginningOfTurn();
    }

    public override string? ProcessEndOfTurn()
    {
        // Grue: stumbling out of one pitch-black room into another with no light source gets you eaten.
        // Faithful to the original, which makes a dark->dark move an 80% chance of death (zork1/gverbs.zil:2095).
        if (_inDarknessAtTurnStart
            && ItIsDarkHere
            && !ReferenceEquals(CurrentLocation, _locationAtTurnStart)
            && Chooser.RollDice(100) <= 80)
            return new DeathProcessor()
                .Process("Oh, no! A lurking grue slithered into the room and devoured you!", this)
                .InteractionMessage;

        if (Score == 350 && !GameOver)
        {
            Repository.GetItem<TrophyCase>().ItemPlacedHere(Repository.GetItem<Map>());
            GameOver = true;
            return "\nAn almost inaudible voice whispers in your ear, \"Look to your treasures for the final secret.\"";
        }

        return base.ProcessEndOfTurn();
    }

    public override bool HaveRoomForItem(IItem item)
    {
        return CalculateTotalSize() < 21;
    }

    /// <summary>
    ///     While a spirit, the dungeon "seems dimly illuminated" — the player needs no light source.
    ///     Overriding this keeps a dead player from being eaten by a grue as they wander dark rooms
    ///     back toward the altar.
    /// </summary>
    /// <remarks>
    ///     Verified against the ZIL: becoming a spirit sets the global ALWAYS-LIT flag
    ///     (<c>&lt;SETG ALWAYS-LIT T&gt;</c>, zork1/1actions.zil:4086), the DEAD-FUNCTION LOOK branch
    ///     prints "Although there is no light, the room seems dimly illuminated." (:3157), and
    ///     resurrection clears the flag again (<c>&lt;SETG ALWAYS-LIT &lt;&gt;&gt;</c>, :3159). See issue #17.
    /// </remarks>
    public override bool ItIsDarkHere => !IsDead && base.ItIsDarkHere;

    /// <summary>
    ///     The in-character "the game is over" line shown for every (non-system) command once the player
    ///     has died permanently. The original game's FINISH prompt is not expressible here, so this is the
    ///     stand-in: it ends the player's ability to act while leaving save/restore/quit available.
    /// </summary>
    public const string PermanentDeathMessage =
        "You are dead, and your adventure has come to an end. There is nothing more you can do. ";

    /// <summary>
    ///     Intercepts the raw command for a player who is no longer playing normally:
    ///     <list type="bullet">
    ///         <item>Permanently dead → every command returns the game-over message (no resurrection).</item>
    ///         <item>
    ///             A spirit → route through the <see cref="SpiritProcessor" /> (the DEAD-FUNCTION
    ///             equivalent), which overrides most verbs and handles resurrection but returns null for
    ///             movement so the player can still walk back to the altar.
    ///         </item>
    ///         <item>Otherwise → null, and the engine processes the command normally.</item>
    ///     </list>
    ///     System commands (save/restore/quit) run earlier in the engine, so they still work in every
    ///     case (issue #17).
    ///
    ///     Note: an intercepted command short-circuits the turn, so NPC actors/daemons do not advance on
    ///     it — a spirit that merely waits/looks is in no danger. This is intentional: the dungeon is
    ///     "frozen and unreal" to a spirit, and the player only re-engages the live world by moving
    ///     (which falls through to normal turn processing). Movement is also the only way a spirit can be
    ///     killed again, which is exactly when the permanent-death path matters.
    /// </summary>
    public override string? InterceptPlayerCommand(string? input)
    {
        if (HasPermanentlyDied)
            return PermanentDeathMessage;

        return IsDead ? new SpiritProcessor().Process(input, this) : null;
    }
}
