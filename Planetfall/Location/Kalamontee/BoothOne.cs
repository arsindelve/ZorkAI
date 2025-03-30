using Model.AIGeneration;

namespace Planetfall.Location.Kalamontee;

internal class BoothOne : BoothBase
{
    public override string Name => "Booth 1";    
    
    public override void Init()
    {
        StartWithItem<TeleportationSlot<BoothOne>>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<ConferenceRoom>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a tiny room with a large \"1\" painted on the wall. A panel contains a slot about ten centimeters " +
            "wide, a beige button labelled \"2\" and a tan button labelled\n\"3.\"";
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.PushVerbs, ["button"]))
            return new DisambiguationInteractionResult("Which button do you mean, the tan button or the beige button?",
                new Dictionary<string, string>
                {
                    { "brown", "tan button" },
                    { "beige", "beige button" },
                    { "3", "tan button" },
                    { "2", "beige button" },
                    { "three", "tan button" },
                    { "two", "beige button" },
                    { "beige button", "beige button" },
                    { "tan button", "tan button" }
                }, "press the {0}");

        if (action.Match(Verbs.PushVerbs, ["tan button", "tan", "beige button", "beige", "3", 
                "2", "two", "three", "3 button", "2 button", "three button", "two button"]))
        {
            if (action.MatchNoun(["beige button", "beige", "2", "two", "2 button", "two button"]))
                return GoTwo(context);

            if (action.MatchNoun(["tan button", "tan", "3", "three", "3 button", "three button"]))
                return GoThree(context);
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}