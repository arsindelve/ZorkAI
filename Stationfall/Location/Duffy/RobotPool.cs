using GameEngine.Item;
using Model.AIGeneration;

namespace Stationfall.Location.Duffy;

/// <summary>
///     Where a Lieutenant requisitions a robot for an assignment (ship.zil:232-243). Feeding the Robot
///     Use Authorization Form into the slot unlocks the keypad; typing a bin number then assigns that
///     robot permanently (verbs.zil ROBOT-TYPE).
///     Only bin three — Floyd — leads anywhere good: he is the sole robot who will take the
///     spacetruck's second seat, and the autopilot will not accept a course without it.
/// </summary>
public class RobotPool : LocationBase
{
    /// <summary>
    ///     Set once the authorization form has been accepted. The keypad is inert until then.
    /// </summary>
    [UsedImplicitly]
    public bool IsAuthorized { get; set; }

    public override string Name => "Robot Pool";

    public override string[] NounsForMatching => ["robot pool", "pool"];

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["keypad", "keys", "console", "equipment", "selection equipment"],
            "A numeric keypad beside the slot, for typing the bin number of the robot you want. ",
            "The keypad is bolted to the console. ")
    ];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<CargoBayEntrance>() },
            { Direction.Out, Go<CargoBayEntrance>() }
        };
    }

    /// <summary>
    ///     The keypad is read from raw input so that any typed number is recognized; see
    ///     <see cref="KeypadInput" />.
    /// </summary>
    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        if (!KeypadInput.TryParse(input, out var binNumber))
            return base.RespondToSpecificLocationInteraction(input, context, client);

        return Task.FromResult<InteractionResult>(new PositiveInteractionResult(SelectRobot(binNumber, context)));
    }

    /// <summary>
    ///     Looking into a bin shows the robot still waiting in it.
    /// </summary>
    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        string[] lookVerbs = ["look in", "look into", "examine", "look", "search", "inspect", "check"];

        if (action.MatchVerb(lookVerbs))
        {
            if (action.MatchNoun(["first bin", "bin one", "bin 1"]))
                return DescribeBin(Repository.GetItem<Rex>());

            if (action.MatchNoun(["second bin", "bin two", "bin 2"]))
                return DescribeBin(Repository.GetItem<Helen>());

            if (action.MatchNoun(["third bin", "bin three", "bin 3"]))
                return DescribeBin(Repository.GetItem<Floyd>());

            if (action.MatchNoun(["bin", "bins"]))
                return new PositiveInteractionResult(
                    "There are three bins. You'll have to say which one you mean. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    private static InteractionResult DescribeBin(ShipRobot robot)
    {
        return new PositiveInteractionResult(
            robot.IsSelected ? "The bin is empty. " : robot.InTheBinDescription);
    }

    /// <summary>
    ///     Assigns the robot in the numbered bin. The choice is permanent — the original allows exactly
    ///     one selection (verbs.zil ROBOT-TYPE).
    /// </summary>
    private string SelectRobot(int binNumber, IContext context)
    {
        if (!IsAuthorized)
            return "A recorded voice says, \"Keyboard is active only following authorization.\" ";

        ShipRobot[] robots =
            [Repository.GetItem<Rex>(), Repository.GetItem<Helen>(), Repository.GetItem<Floyd>()];

        if (robots.Any(robot => robot.IsSelected))
            return "A recorded voice says, \"You have already made your selection.\" ";

        if (binNumber < 1)
            return "A recorded voice says, \"Error.\" ";

        if (binNumber > 3)
            return "A recorded voice says, \"That bin is unoccupied.\" ";

        var chosen = robots.Single(robot => robot.BinNumber == binNumber);
        chosen.IsSelected = true;
        context.RegisterActor(chosen);

        if (chosen is Floyd)
            return "\"Yippee!\" yells Floyd, bounding out of his bin and hugging you around the knees. ";

        return $"{chosen.Name} rolls up to you, ready to follow. In bin three, Floyd's lower jaw begins " +
               "to quiver, as though he were about to cry. ";
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is where a Lieutenant comes to draw a robot for an important assignment — or, in " +
               "your case, for this one. Three bins line the far wall. Set into the requisition console " +
               "are a slot, for your form, and a keypad, for your selection. You can exit aft. ";
    }

    public override void Init()
    {
        StartWithItem<RobotPoolSlot>();
        StartWithItem<Rex>();
        StartWithItem<Helen>();
        StartWithItem<Floyd>();
    }
}
