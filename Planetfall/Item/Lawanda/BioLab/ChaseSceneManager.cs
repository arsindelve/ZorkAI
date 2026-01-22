using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda.LabOffice;
using Planetfall.Location.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.BioLab;

/// <summary>
/// Manages the chase scene after the player enters the Bio Lab.
/// Tracks player movement and determines when mutants catch the player.
/// </summary>
public class ChaseSceneManager : ItemBase, ITurnBasedActor
{
    public override string[] NounsForMatching => [];

    [UsedImplicitly]
    public bool ChaseActive { get; set; } = false;

    [UsedImplicitly]
    public ILocation? LastLocation { get; set; }

    [UsedImplicitly]
    public ILocation? SecondToLastLocation { get; set; }

    [UsedImplicitly]
    public int TurnsInBioLockWest { get; set; } = 0;

    [UsedImplicitly]
    public int TurnsInCryoElevator { get; set; } = 0;

    public void StartChase()
    {
        ChaseActive = true;
        LastLocation = null;
        SecondToLastLocation = null;
        TurnsInBioLockWest = 0;
        TurnsInCryoElevator = 0;
    }

    public void StopChase()
    {
        ChaseActive = false;
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!ChaseActive)
            return Task.FromResult(string.Empty);

        var currentLoc = context.CurrentLocation;

        // Check if fungicide is active
        var fungicideTimer = Repository.GetItem<FungicideTimer>();
        bool fungicideActive = fungicideTimer.IsActive;

        // Death condition: Backtracking
        if (currentLoc == SecondToLastLocation && currentLoc != null)
        {
            context.RemoveActor(this);
            return Task.FromResult(
                new DeathProcessor().Process(
                    "As you backtrack, you run directly into the pursuing mutants! " +
                    "The rat-ant, troll, grue, and triffid overwhelm you before you can react. ",
                    context).InteractionMessage);
        }

        // Track turns in Bio-Lock-West for warning
        if (currentLoc is BioLockWest)
        {
            TurnsInBioLockWest++;
            if (TurnsInBioLockWest == 1)
            {
                return Task.FromResult(
                    "You hear terrible sounds behind you as the mutants give chase! ");
            }
            if (TurnsInBioLockWest >= 2)
            {
                context.RemoveActor(this);
                return Task.FromResult(
                    new DeathProcessor().Process(
                        "You lingered too long! The mutants catch up to you in the bio-lock. " +
                        "The rat-ant, troll, grue, and triffid tear you apart. ",
                        context).InteractionMessage);
            }
        }
        else
        {
            TurnsInBioLockWest = 0;
        }

        // Track turns in Cryo Elevator for warning
        if (currentLoc is CryoElevatorLocation)
        {
            TurnsInCryoElevator++;
            if (TurnsInCryoElevator == 1)
            {
                return Task.FromResult(
                    "The mutants are right behind you! You can hear them approaching the elevator! ");
            }
        }
        else
        {
            TurnsInCryoElevator = 0;
        }

        // Move mutants to player's location if fungicide is not active
        if (!fungicideActive && ChaseActive)
        {
            var bioLab = Repository.GetLocation<BioLabLocation>();
            var mutants = new List<ItemBase>
            {
                Repository.GetItem<RatAnt>(),
                Repository.GetItem<MutantTroll>(),
                Repository.GetItem<MutantGrue>(),
                Repository.GetItem<Triffid>()
            };

            foreach (var mutant in mutants)
            {
                mutant.CurrentLocation?.RemoveItem(mutant);
                currentLoc.ItemPlacedHere(mutant);
            }

            // Death if mutants catch player outside Bio Lab
            if (currentLoc != bioLab)
            {
                context.RemoveActor(this);
                return Task.FromResult(
                    new DeathProcessor().Process(
                        "The mutants catch up to you! The rat-ant, troll, grue, and triffid " +
                        "attack you from all sides. You don't stand a chance. ",
                        context).InteractionMessage);
            }
        }

        // Update location history
        SecondToLastLocation = LastLocation;
        LastLocation = currentLoc;

        return Task.FromResult(string.Empty);
    }
}
