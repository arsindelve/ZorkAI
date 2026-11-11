using Model.AIGeneration;
namespace Stationfall.Item.Duffy;

/// <summary>
///     The spacetruck's survival kit (ship.zil:1228-1236). Worth taking before you leave the truck: the
///     Thermos inside is the only thing that keeps the station's explosive from subliming away, much
///     later in the game.
/// </summary>
public class SurvivalKit : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["survival kit", "kit", "survival"];

    public override int Size => 10;

    protected override int SpaceForItems => 20;

    public string ExaminationDescription => IsOpen
        ? ItemListDescription("survival kit", null)
        : "The survival kit is closed. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A survival kit is stowed here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    /// <summary>
    ///     Emptying the kit amounts to trying to get the goo out of it, which is the one thing the goo
    ///     will not allow (ship.zil FOOD-KIT-F hands EMPTY straight to the goo).
    /// </summary>
    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.MatchNounAndAdjective(NounsForMatching) &&
            action.MatchVerb(["empty", "dump", "pour out", "tip out"]))
            return new PositiveInteractionResult(
                "You tip the kit up. The goo stays exactly where it is, and the Thermos rolls back " +
                "into the corner. You'll have to eat the goo right out of the kit. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override void Init()
    {
        ItemPlacedHere<Thermos>();
        ItemPlacedHere<GrayGoo>();
        ItemPlacedHere<OrangeGoo>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return !IsOpen ? "A survival kit" : $"A survival kit\n{ItemListDescription("survival kit", null)}";
    }
}
