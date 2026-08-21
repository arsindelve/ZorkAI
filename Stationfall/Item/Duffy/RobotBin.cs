using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     One of the three bins in the Robot Pool (ship.zil BIN-F). A bin is a window onto whichever robot
///     is waiting in it: looking inside is how a player is meant to shop before committing to a
///     requisition, and getting that choice wrong is one of the ways the opening can go badly.
/// </summary>
public abstract class RobotBin : ItemBase, ICanBeExamined
{
    /// <summary>
    ///     Which bin this is, and therefore which robot stands in it.
    /// </summary>
    protected abstract int BinNumber { get; }

    /// <summary>
    ///     The robot waiting here, or null once it has been requisitioned and walked out.
    /// </summary>
    private ShipRobot? Occupant =>
        Repository.GetItem<Rex>().BinNumber == BinNumber && !Repository.GetItem<Rex>().IsSelected
            ? Repository.GetItem<Rex>()
            : Repository.GetItem<Helen>().BinNumber == BinNumber && !Repository.GetItem<Helen>().IsSelected
                ? Repository.GetItem<Helen>()
                : Repository.GetItem<Floyd>().BinNumber == BinNumber && !Repository.GetItem<Floyd>().IsSelected
                    ? Repository.GetItem<Floyd>()
                    : null;

    public string ExaminationDescription => Occupant?.InTheBinDescription ?? "The bin is empty. ";

    public override string CannotBeTakenDescription => "The bin is part of the wall. ";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNounAndAdjective(NounsForMatching))
            return new NoNounMatchInteractionResult();

        if (action.MatchVerb(["look in", "look inside", "search", "examine", "inspect", "look at"]))
            return new PositiveInteractionResult(ExaminationDescription);

        // The original refuses to let you climb in or use a bin as storage, in as many words.
        if (action.MatchVerb(["enter", "get in", "climb in", "sit in", "put", "put in"]))
            return new PositiveInteractionResult("The bin is only for robots. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
