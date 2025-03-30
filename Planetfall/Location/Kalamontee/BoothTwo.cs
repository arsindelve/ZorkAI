using Model.AIGeneration;

namespace Planetfall.Location.Kalamontee;

internal class BoothTwo : BoothBase
{
    public override string Name => "Booth 2";
    
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<ElevatorLobby>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a tiny room with a large \"2\" painted on the wall. A panel contains a " +
               "slot about ten centimeters wide, a brown button labelled \"1\" and a tan button labelled\n\"3.\"";
    }
    
    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.PushVerbs, ["button"]))
            return new DisambiguationInteractionResult("Which button do you mean, the brown button or the tan button?",
                new Dictionary<string, string>
                {
                    { "brown", "brown button" },
                    { "tan", "tan button" },
                    { "1", "brown button" },
                    { "3", "tan button" },
                    { "one", "brown button" },
                    { "three", "tan button" },
                    { "beige button", "tan button" },
                    { "brown button", "brown button" }
                }, "press the {0}");

        if (action.Match(Verbs.PushVerbs, ["brown button", "brown", "tan button", "tan", "1", 
                "3", "one", "three", "1 button", "3 button", "one button", "three button"]))
        {
            if (action.MatchNoun(["tan button", "tan", "3", "two", "3 button", "three button"]))
                return GoThree(context);

            if (action.MatchNoun(["brown button", "brown", "1", "one", "1 button", "one button"]))
                return GoOne(context);
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
    
    public override void Init()
    {
        StartWithItem<TeleportationSlot<BoothTwo>>();
    }
}