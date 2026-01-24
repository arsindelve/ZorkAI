using Model.AIGeneration;
using Newtonsoft.Json;
using Planetfall.Command;
using Planetfall.Location.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.BioLab;

/// <summary>
/// Manages the chase scene after the player leaves BioLockWest.
/// Player dies if they pause (do anything) or backtrack.
/// Exception: One free turn in BioLockWest and BioLab to open doors.
/// </summary>
public class ChaseSceneManager : ItemBase, ITurnBasedActor
{
    public static readonly List<string> ChaseMessages =
    [
        "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms! ",
        "The mutants burst into the room right on your heels! The mobile plant whips its poisonous tentacles against your ankles! ",
        "The mutants burst into the room right on your heels! A pair of slavering fangs removes part of your clothing! ",
        "The mutants burst into the room right on your heels! The growling humanoid is charging straight at you, waving his axe-like implement! ",
        "The monsters gallop toward you, smacking their lips. "
    ];

    public override string[] NounsForMatching => [];

    [UsedImplicitly] [JsonIgnore]
    public IRandomChooser Chooser { get; set; } = new RandomChooser();

    [UsedImplicitly]
    public bool ChaseActive { get; set; }

    [UsedImplicitly]
    public ILocation? LastLocation { get; set; }

    [UsedImplicitly]
    public ILocation? PreviousLocation { get; set; }

    [UsedImplicitly]
    public bool UsedBioLockWestFreeTurn { get; set; }

    [UsedImplicitly]
    public bool UsedBioLabFreeTurn { get; set; }

    public void StartChase(ILocation startingLocation, ILocation? previousLocation = null)
    {
        ChaseActive = true;
        LastLocation = startingLocation;
        PreviousLocation = previousLocation;
        UsedBioLockWestFreeTurn = false;
        UsedBioLabFreeTurn = false;
    }

    public void StopChase()
    {
        ChaseActive = false;
        LastLocation = null;
        PreviousLocation = null;
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!ChaseActive)
            return Task.FromResult(string.Empty);

        var currentLoc = context.CurrentLocation;

        // Death: Player paused (stayed in same location)
        if (currentLoc == LastLocation)
        {
            // Exception: One free turn in BioLockWest to open the door
            if (currentLoc is BioLockWest && !UsedBioLockWestFreeTurn)
            {
                UsedBioLockWestFreeTurn = true;
                return Task.FromResult("The mutants are almost upon you now! ");
            }

            // Exception: One free turn in BioLab to open the door
            if (currentLoc is BioLabLocation && !UsedBioLabFreeTurn)
            {
                UsedBioLabFreeTurn = true;
                return Task.FromResult(
                    "The air is filled with mist, which is affecting the mutants. " +
                    "They appear to be stunned and confused, but are slowly recovering. ");
            }

            // CryoElevator: If they paused here without pushing button, instant death
            if (currentLoc is CryoElevatorLocation)
            {
                StopChase();
                context.RemoveActor(this);
                return Task.FromResult(
                    new DeathProcessor().Process(
                        "The biological nightmares reach you. Gripping coils wrap around your limbs as powerful " +
                        "teeth begin tearing at your flesh. Something bites your leg, and you feel a powerful " +
                        "poison begin to work its numbing effects... ",
                        context).InteractionMessage);
            }

            StopChase();
            context.RemoveActor(this);
            return Task.FromResult(
                new DeathProcessor().Process(
                    "Dozens of hungry eyes fix on you as the mutations surround you and begin feasting. ",
                    context).InteractionMessage);
        }

        // Death: Player backtracked to previous location
        if (currentLoc == PreviousLocation)
        {
            StopChase();
            context.RemoveActor(this);
            return Task.FromResult(
                new DeathProcessor().Process(
                    "You stupidly run right into the jaws of the pursuing mutants. ",
                    context).InteractionMessage);
        }

        // Update location history
        PreviousLocation = LastLocation;
        LastLocation = currentLoc;

        // CryoElevator has its own entry message, don't duplicate
        if (currentLoc is CryoElevatorLocation)
            return Task.FromResult(string.Empty);

        // Player is successfully fleeing - show random chase message
        return Task.FromResult(Chooser.Choose(ChaseMessages));
    }
}
