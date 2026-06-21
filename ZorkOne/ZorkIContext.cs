using GameEngine;
using Model.Interface;
using Model.Location;
using Newtonsoft.Json;
using ZorkOne.Command;

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
}