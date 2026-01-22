using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Planetfall.Command;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

/// <summary>
/// Manages the chase scene logic after the player enters the Bio Lab.
/// This actor runs every turn and tracks mutant movement.
/// </summary>
public class ChaseSceneManager : ItemBase, ITurnBasedActor
{
    public override string[] NounsForMatching => [];

    [UsedImplicitly] public IRandomChooser Chooser { get; set; } = new RandomChooser();

    private readonly string[] _monsterEntranceMessages =
    [
        "The growling humanoid is charging straight at you, waving his axe-like implement! ",
        "A pair of slavering fangs removes part of your clothing! ",
        "Needle-sharp mandibles nip at your arms! ",
        "The mobile plant whips its poisonous tentacles against your ankles! "
    ];

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (context is not PlanetfallContext pfContext)
            return Task.FromResult(string.Empty);

        var ratAnt = Repository.GetItem<RatAnt>();
        var bioLab = Repository.GetLocation<BioLabLocation>();

        // Only run if chase is enabled
        if (!bioLab.ChaseSceneEnabled)
            return Task.FromResult(string.Empty);

        // CONDITION 1: Mutants in same room + no fungicide = death
        if (ratAnt.CurrentLocation == context.CurrentLocation && !pfContext.LabFlooded)
        {
            var deathResult = new DeathProcessor().Process(
                "\nDozens of hungry eyes fix on you as the mutations surround you and begin feasting.", pfContext);
            return Task.FromResult(deathResult.InteractionMessage);
        }

        // CONDITION 2: No fungicide - chase is active
        if (!pfContext.LabFlooded)
        {
            var result = HandleChaseLogic(pfContext);

            // Update tracking (always runs, even if LAB-FLOODED = true)
            pfContext.SecondToLastRoom = pfContext.LastChaseRoom;
            pfContext.LastChaseRoom = context.CurrentLocation;

            return Task.FromResult(result);
        }

        // UPDATE TRACKING (always runs, even if LAB-FLOODED = true)
        pfContext.SecondToLastRoom = pfContext.LastChaseRoom;
        pfContext.LastChaseRoom = context.CurrentLocation;

        return Task.FromResult(string.Empty);
    }

    private string HandleChaseLogic(PlanetfallContext context)
    {
        var bioLockWest = Repository.GetLocation<BioLockWest>();
        var cryoElevator = Repository.GetLocation<CryoElevatorLocation>();

        // CASE A: Player in BIO-LOCK-WEST, first time
        if (context.CurrentLocation == bioLockWest && !context.ExtraMoveFlag)
        {
            context.ExtraMoveFlag = true;
            return "\nThe monsters gallop toward you, smacking their lips. ";
        }

        // CASE B: Player in CRYO-ELEVATOR, first time
        if (context.CurrentLocation == cryoElevator && !context.CryoMoveFlag)
        {
            context.CryoMoveFlag = true;
            return "\nThe monsters are storming straight toward the elevator door! ";
        }

        // CASE C: Player backtracked into mutants
        if (context.CurrentLocation == context.SecondToLastRoom)
        {
            var deathResult = new DeathProcessor().Process(
                "\nYou stupidly run right into the jaws of the pursuing mutants.", context);
            return deathResult.InteractionMessage;
        }

        // CASE D: Normal chase progression
        // Special case: Player lingered in cryo-elevator
        if (context.CurrentLocation == cryoElevator)
        {
            var deathResult = new DeathProcessor().Process(
                "The biological nightmares reach you. Gripping coils wrap around your limbs as powerful teeth begin tearing at your flesh. " +
                "Something bites your leg, and you feel a powerful poison begin to work its numbing effects...", context);
            return deathResult.InteractionMessage;
        }

        // Move all mutants to player's location
        var ratAnt = Repository.GetItem<RatAnt>();
        var triffid = Repository.GetItem<Triffid>();
        var troll = Repository.GetItem<Troll>();
        var grue = Repository.GetItem<Grue>();

        var currentLoc = context.CurrentLocation as ICanContainItems;
        if (currentLoc == null)
            return string.Empty;

        ratAnt.CurrentLocation?.RemoveItem(ratAnt);
        ratAnt.CurrentLocation = currentLoc;
        context.CurrentLocation.ItemPlacedHere(ratAnt);

        triffid.CurrentLocation?.RemoveItem(triffid);
        triffid.CurrentLocation = currentLoc;
        context.CurrentLocation.ItemPlacedHere(triffid);

        troll.CurrentLocation?.RemoveItem(troll);
        troll.CurrentLocation = currentLoc;
        context.CurrentLocation.ItemPlacedHere(troll);

        grue.CurrentLocation?.RemoveItem(grue);
        grue.CurrentLocation = currentLoc;
        context.CurrentLocation.ItemPlacedHere(grue);

        // Print chase message
        var message = "\nThe mutants ";
        if (context.CurrentLocation == bioLockWest)
        {
            message += "are almost upon you now! ";
        }
        else
        {
            message += "burst into the room right on your heels! ";
            message += Chooser.Choose(_monsterEntranceMessages.ToList());
        }

        return message;
    }
}
