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

    private static string OperateHatch(bool open, IContext context)
    {
        var hatch = Repository.GetItem<SpacetruckHatch>();

        if (hatch.IsOpen == open)
            return open ? "It is already open. " : "It is already closed. ";

        var refusal = open ? hatch.CannotBeOpenedDescription(context) : null;
        if (refusal is not null)
            return refusal;

        hatch.IsOpen = open;
        return open ? hatch.NowOpen(context.CurrentLocation) : hatch.NowClosed(context.CurrentLocation);
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.MatchNounAndAdjective(NounsForMatching))
        {
            // Opening "the truck" means opening its hatch; there is nothing else on it to open.
            // Acting on the hatch directly rather than forwarding the intent: the intent names the
            // truck, which the hatch does not answer to, so forwarding it just fell through to the
            // narrator - the very gap the sweep caught.
            if (action.MatchVerb(["open", "unseal"]))
                return new PositiveInteractionResult(OperateHatch(true, context));

            if (action.MatchVerb(["close", "shut"]))
                return new PositiveInteractionResult(OperateHatch(false, context));

            if (action.MatchVerb(["search", "look in", "look inside"]))
                return new PositiveInteractionResult(
                    Repository.GetItem<SpacetruckHatch>().IsOpen
                        ? "Through the open hatch you can see two seats and an empty cargo section. "
                        : "The hatch is closed, and the viewport shows you nothing but your own " +
                          "reflection. ");

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
