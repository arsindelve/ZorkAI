using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech;
using Utilities;

namespace Planetfall.Location.Kalamontee.Mech;

internal class MachineShop : LocationWithNoStartingItems
{
    public override string Name => "Machine Shop";

    [UsedImplicitly] public bool FlaskUnderSpout { get; set; }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<MechCorridorSouth>() },
            { Direction.W, Go<ToolRoom>() },
            { Direction.E, Go<RobotShop>() }
        };
    }

    public override Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        var flask = Repository.GetItem<Flask>();
        string[] verbs = ["place", "put", "move"];
        string[] spoutNouns = ["spout", "machine", "dispenser", "dispensing machine", "dispensing machine spout"];

        if (action.Match(verbs, flask.NounsForMatching, spoutNouns, ["under", "underneath"]))
        {
            if (flask.IsHereButNotInInventory(context))
                return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(
                    "You don't have the flask. "));

            if (!context.HasItem<Flask>())
                return Task.FromResult<InteractionResult?>(new NoNounMatchInteractionResult());

            FlaskUnderSpout = true;
            ItemPlacedHere<Flask>();
            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("The glass flask is now sitting under the spout. "));
        }

        return base.RespondToMultiNounInteraction(action, context);
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        var verb = action.Verb.ToLowerInvariant().Trim();
        var noun = action.Noun?.ToLowerInvariant().ToLowerInvariant().Trim();

        if (!Verbs.PushVerbs.Contains(verb))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        // If they said "blue button", simplify and replace "button" with "blue"
        if (action.MatchNoun(["button"]))
        {
            if (!string.IsNullOrEmpty(action.Adjective))
                noun = action.Adjective.ToLowerInvariant();
            else
                return new DisambiguationInteractionResult(
                    $"Which button do you mean, {new List<string>
                    {
                        "blue button",
                        "red button",
                        "yellow button",
                        "green button",
                        "brown button",
                        "gray button",
                        "black button",
                        "square button",
                        "round button"
                    }.SingleLineListWithOr()}?",
                    new Dictionary<string, string>
                    {
                        { "green", "green button" },
                        { "yellow", "yellow button" },
                        { "red", "red button" },
                        { "blue", "blue button" },
                        { "brown", "brown button" },
                        { "gray", "gray button" },
                        { "black", "black button" },
                        { "square", "square button" },
                        { "round", "round button" }
                    },
                    "press {0}"
                );
        }

        return noun switch
        {
            "blue button" or "blue" => Click("blue"),
            "red button" or "red" => Click("red"),
            "yellow button" or "yellow" => Click("yellow"),
            "green button" or "green" => Click("green"),
            "brown button" or "white" => Click("brown"),
            "gray button" or "gray" => Click("gray"),
            "black button" or "black" => Click("black"),
            "square button" or "square" => Click("clear"),
            "round button" or "round" => Click("clear"),
            _ => await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory)
        };
    }

    private InteractionResult Click(string color)
    {
        if (!FlaskUnderSpout)
            return new PositiveInteractionResult(
                "Some sort of chemical fluid pours out of the spout, spills all over the floor, and dries up. ");

        var flask = Repository.GetItem<Flask>();

        if (!string.IsNullOrEmpty(flask.LiquidColor))
            return new PositiveInteractionResult(
                "Another dose of the chemical fluid pours out of the spout, splashes over the already-full flask, spills onto the floor, and dries up.");

        flask.LiquidColor = color;

        return new PositiveInteractionResult(
            $"The flask fills with some {color} chemical fluid. The fluid gradually turns milky white. ");
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This room is probably some sort of machine shop filled with a variety of unusual machines. Doorways lead " +
            "north, east, and west.\n\nStanding against the rear wall is a large dispensing machine with a spout. " +
            (FlaskUnderSpout ? "Sitting under the spout is a glass flask. " : "") +
            "The dispenser is lined with brightly colored buttons. The first four buttons, labelled \"KUULINTS 1 - 4\", " +
            "are colored red, blue, green, and yellow. The next three buttons, labelled \"KATALISTS 1 - 3\", are colored " +
            "gray, brown, and black. The last two buttons are both white. One of these is square and says \"BAAS.\" The " +
            "other white button is round and says \"ASID.\" ";
    }
}