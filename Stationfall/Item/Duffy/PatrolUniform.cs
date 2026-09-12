using Model.AIGeneration;
namespace Stationfall.Item.Duffy;

/// <summary>
///     The standard-issue Stellar Patrol uniform, worn from the start, with a single pocket holding the
///     player's ID card (ship.zil:120-145).
/// </summary>
public class PatrolUniform : ContainerBase, ICanBeTakenAndDropped, ICanBeExamined, IAmClothing
{
    public override string[] NounsForMatching => ["patrol uniform", "uniform"];

    // The pocket has no lid: you can always see what's in it.
    public override bool IsTransparent => true;

    public override int Size => 8;

    // Anything the player puts "in the uniform" belongs in the pocket.
    public override ICanContainItems ForwardingContainer => Repository.GetItem<PatrolUniformPocket>();

    public bool BeingWorn { get; set; } = true;

    public string ExaminationDescription =>
        "A one-pocket Stellar Patrol uniform. Patrol literature claims it warms you when it's cold, cools " +
        "you when it's hot, shrugs off mild radiation, discourages insects, and flatters the wearer's " +
        "complexion. It is, at minimum, comfortable. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A rumpled Patrol uniform lies here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }
    /// <summary>
    ///     The pocket is part of the garment - there is nothing to work (ship.zil PATROL-UNIFORM-F).
    /// </summary>
    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.MatchNounAndAdjective(NounsForMatching) &&
            action.MatchVerb(["open", "close", "shut", "unfasten", "fasten"]))
            return new PositiveInteractionResult(
                "There's no way to open or close the pocket of the uniform. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }


    public override void Init()
    {
        StartWithItemInside<PatrolUniformPocket>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A Patrol uniform" + (BeingWorn ? " (being worn)" : "") +
               (Repository.GetItem<PatrolUniformPocket>().Items.Any()
                   ? $"\n{ItemListDescription("Patrol uniform", null)}"
                   : "");
    }
}
