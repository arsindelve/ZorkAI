using Model.AIGeneration;

namespace Planetfall.Location.Lawanda;

internal class BoothThree : BoothBase
{
    public override string Name => "Booth 3";
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<LibraryLobby>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a tiny room with a large \"3\" painted on the wall. A panel contains a slot about ten centimeters " +
            "wide, a brown button labelled \"1\" and a beige button labelled \"2.\"";
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.PushVerbs, ["button"]))
            return new DisambiguationInteractionResult("Which button do you mean, the brown button or the beige button?",
                new Dictionary<string, string>
                {
                    { "brown", "brown button" },
                    { "beige", "beige button" },
                    { "1", "brown button" },
                    { "2", "beige button" },
                    { "one", "brown button" },
                    { "two", "beige button" },
                    { "beige button", "beige button" },
                    { "brown button", "brown button" }
                }, "press the {0}");

        if (action.Match(Verbs.PushVerbs, ["brown button", "brown", "beige button", "beige", "1", 
                "2", "one", "two", "1 button", "2 button", "one button", "two button"]))
        {
            if (action.MatchNoun(["beige button", "beige", "2", "two", "2 button", "two button"]))
                return GoTwo(context);

            if (action.MatchNoun(["brown button", "brown", "1", "one", "1 button", "one button"]))
                return GoOne(context);
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
    
    public override void Init()
    {
        StartWithItem<TeleportationSlot<BoothThree>>();
    }
}