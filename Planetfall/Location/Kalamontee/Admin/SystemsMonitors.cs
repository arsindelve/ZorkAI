using GameEngine.Location;
using Model.AIGeneration;
using Utilities;

namespace Planetfall.Location.Kalamontee.Admin;

public class SystemsMonitors : LocationWithNoStartingItems
{
    public override string Name => "Systems Monitors";

    [UsedImplicitly]
    public List<string> Fixed { get; set; } = [];

    [UsedImplicitly]
    public List<string> Busted { get; set; } = [];

    private string Green => Fixed.SingleLineListWithAndNoArticle();
    
    private string Red => Busted.SingleLineListWithAndNoArticle();

    private string Monitors()
    {
        if (Busted.Count == 1)
            return
                $"The far wall is filled with a number of monitors. Of these, the ones labelled {Green} are green, but " +
                $"the one labelled {Red} indicates a malfunctioning condition. ";

        return
            $"The far wall is filled with a number of monitors. Of these, the ones labelled {Green} are green, but " +
            $"the ones labelled {Red} indicate a malfunctioning condition. ";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<AdminCorridor>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return $"This is a large room filled with tables full of strange equipment. {Monitors()}";
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        string[] verbs = ["examine", "look at"];

        if (!action.MatchVerb(verbs))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        if (action.MatchNoun(["screens", "monitors"]))
            return new PositiveInteractionResult(Monitors());

        if (action.MatchNoun(["equipment"]))
            return new PositiveInteractionResult(
                "The equipment here is so complicated that you couldn't even begin to figure out how to operate it. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    internal void MarkCommunicationsFixed()
    {
        Busted.Remove("KUMUUNIKAASHUNZ");
        Fixed.Add("KUMUUNIKAASHUNZ");
    }
    
    internal void MarkCourseControlFixed()
    {
        Busted.Remove("PLANATEREE KORS KUNTROOL");
        Fixed.Add("PLANATEREE KORS KUNTROOL");
    }
    
    internal void MarkPlanetaryDefenseFixed()
    {
        Busted.Remove("PLANATEREE DEFENS");
        Fixed.Add("PLANATEREE DEFENS");
    }

    public override void Init()
    {
        Fixed =
        [
            "LIIBREREE",
            "REEAKTURZ",
            "LIIF SUPORT"
        ];

        Busted =
        [
            "PLANATEREE DEFENS",
            "PLANATEREE KORS KUNTROOL",
            "KUMUUNIKAASHUNZ",
            "PRAJEKT KUNTROOL"
        ];
    }
}