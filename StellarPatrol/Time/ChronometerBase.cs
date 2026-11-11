using Newtonsoft.Json;

namespace StellarPatrol;

/// <summary>
///     The wrist chronometer both Stellar Patrol protagonists start the game wearing. It is the
///     player's only window onto Galactic Standard Time, which is also the clock every survival timer
///     is scheduled against, so the reading it reports has to be the exact number those timers consume.
///     Each game derives to supply its own starting time, wording, and any behaviour of its own.
/// </summary>
public abstract class ChronometerBase : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead,
    IAmClothing
{
    /// <summary>
    ///     Galactic Standard Time, advanced every turn by the game's context. Seeded by the derived
    ///     class's constructor, since the two games start at different times of day.
    /// </summary>
    [UsedImplicitly]
    public int CurrentTime { get; set; }

    /// <summary>
    ///     Injectable randomness for anything a test needs to pin. Note that the *starting* time cannot
    ///     use this - it is chosen during construction, before anything could be injected - so tests
    ///     pin that by assigning <see cref="CurrentTime" /> directly.
    /// </summary>
    [UsedImplicitly]
    [JsonIgnore]
    public IRandomChooser Chooser { get; set; } = new RandomChooser();

    /// <summary>
    ///     Worn from the first turn in both games.
    /// </summary>
    public bool BeingWorn { get; set; } = true;

    public override string[] NounsForMatching => ["chronometer", "watch"];

    /// <summary>
    ///     What the player is told when they read or examine it. Separate from the interface members
    ///     because both routes must always give the same answer.
    /// </summary>
    protected abstract string TimeDescription { get; }

    public string ExaminationDescription => TimeDescription;

    public string ReadDescription => TimeDescription;

    public abstract string OnTheGroundDescription(ILocation currentLocation);

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A chronometer" + (BeingWorn ? " (being worn)" : "");
    }
}
