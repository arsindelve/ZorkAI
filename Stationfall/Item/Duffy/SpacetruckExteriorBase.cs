using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     The spacetruck seen from outside — from the cargo bay before you leave, and from the docking bay
///     after you arrive (ship.zil SPACETRUCK-OBJECT-F). The room's prose names it, so the player will
///     type it, and it answers for the verbs you would expect to aim at a parked vehicle.
///     Two objects rather than one, for the same reason the hatch is three: one object cannot honestly
///     be in two rooms. It holds no state, so nothing is split by splitting it.
/// </summary>
public abstract class SpacetruckExteriorBase : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["spacetruck", "truck", "spacecraft", "rig"];

    public override string CannotBeTakenDescription => "It's a twelve-meter spacecraft. ";

    public string ExaminationDescription
    {
        get
        {
            var open = Repository.GetItem<SpacetruckHatch>().IsOpen;

            return "A twelve-meter rig, the largest Class Three spacecraft made — a working truck, " +
                   $"scuffed and unglamorous. Its hatch is {(open ? "open" : "closed")}. ";
        }
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.MatchNounAndAdjective(NounsForMatching))
        {
            // Opening "the truck" means opening its hatch; there is nothing else on it to open.
            if (action.MatchVerb(["open", "close", "shut", "unseal"]))
                return await Repository.GetItem<SpacetruckHatch>()
                    .RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

            if (action.MatchVerb(["launch", "start", "drive", "fly", "turn on"]))
                return new PositiveInteractionResult("You're not even in it! ");

            if (action.MatchVerb(["enter", "board", "get in", "climb in"]))
                return new PositiveInteractionResult(
                    Repository.GetItem<SpacetruckHatch>().IsOpen
                        ? "You'll have to go in. "
                        : "The spacetruck's hatch is closed. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
