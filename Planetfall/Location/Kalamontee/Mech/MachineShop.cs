using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech;
using Utilities;

namespace Planetfall.Location.Kalamontee.Mech;

internal class MachineShop : LocationWithNoStartingItems
{
    public override string Name => "Machine Shop";

    // "workshop" lives only here, not on the Tool Room / Robot Shop next door. All three are exits of
    // Mech Corridor South, so a shared "workshop" alias produced a dead 3-way "which one?" that could
    // never move (issue #268 review). Keeping it on the actual machine shop resolves to a single hop.
    public override string[] NounsForMatching => ["workshop"];

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
                        { "round", "round button" },
                        // The room names the last two buttons by color and by label, so a player answering
                        // this prompt is as likely to say "white" or "asid" as "round" (issue #419).
                        { "white", "white button" },
                        { "baas", "square button" },
                        { "base", "square button" },
                        { "asid", "round button" },
                        { "acid", "round button" }
                    },
                    "press {0}"
                );
        }

        // Issue #419: the description tells the player the last two buttons are white and prints their
        // labels ("BAAS" on the square one, "ASID" on the round one), but only the shapes used to be
        // matched. Every label the room itself printed fell through to the AI narrator, which cheerfully
        // narrated a dispense that never happened while the flask stayed empty — the same trap as #412.
        // Worse, "white" had been pasted onto the *brown* case, so "press white button" dispensed the
        // brown KATALIST and bare "brown" matched nothing at all.
        //
        // The parser hands the same button back in several shapes: a whole noun ("round white button"),
        // a bare adjective ("round"), or a multi-word adjective phrase ("round white", "white round").
        // Enumerating those permutations is whack-a-mole and is how the labels got missed in the first
        // place, so match on the distinguishing *word* instead — any phrasing the room's own text invites
        // then lands on the right button, in any word order.
        var words = noun?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word != "button")
            .ToArray() ?? [];

        // ASID (round) and BAAS (square) both dispense the same "clear" fluid: in the original, COLOR-LTBL
        // entries 8 and 9 are both "clear" (planetfall-source/compone.zil:1773-1785) and the one place the
        // game reads the two apart treats them identically. Acid and base are deliberately
        // indistinguishable here — that is not the bug.
        string[] whiteButtons = ["round", "asid", "acid", "square", "baas", "base"];

        if (words.Any(whiteButtons.Contains))
            return Click("clear");

        // A shape or a label picks out one of the two white buttons; bare "white" describes both, so ask
        // which one rather than silently picking (or, as before, reaching for the brown button).
        if (words.Contains("white"))
            return WhichWhiteButton();

        string[] coloredButtons = ["blue", "red", "yellow", "green", "brown", "gray", "black"];
        var color = coloredButtons.FirstOrDefault(words.Contains);

        return color is not null
            ? Click(color)
            : await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    private static InteractionResult WhichWhiteButton()
    {
        return new DisambiguationInteractionResult(
            "Which white button do you mean, the square white button or the round white button?",
            new Dictionary<string, string>
            {
                { "square", "square button" },
                { "baas", "square button" },
                { "base", "square button" },
                { "round", "round button" },
                { "asid", "round button" },
                { "acid", "round button" }
            },
            "press {0}"
        );
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