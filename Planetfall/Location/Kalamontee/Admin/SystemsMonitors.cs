using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Admin;

public class SystemsMonitors : LocationWithNoStartingItems
{
    private string Monitors()
    {
        return
        "The far wall is filled with a number of monitors. Of these, the ones labelled LIIBREREE, " +
            "REEAKTURZ, and LIIF SUPORT are green, but the ones labelled PLANATEREE DEFENS, PLANATEREE KORS " +
            "KUNTROOL, KUMUUNIKAASHUNZ, and PRAJEKT KUNTROOL indicate a malfunctioning condition. ";
    }

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.E, Go<AdminCorridor>() }
        };

    protected override string ContextBasedDescription =>
        $"This is a large room filled with tables full of strange equipment. {Monitors()}";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client)
    {
        string[] verbs = ["examine", "look at"];
        
        if(!action.MatchVerb(verbs))
            return base.RespondToSimpleInteraction(action, context, client);

        if (action.MatchNoun(["screens", "monitors"]))
            return new PositiveInteractionResult(Monitors());

        if (action.MatchNoun(["equipment"]))
            return new PositiveInteractionResult(
                "The equipment here is so complicated that you couldn't even begin to figure out how to operate it. ");
        
        return base.RespondToSimpleInteraction(action, context, client);
    }
    
    public override string Name => "Systems Monitors";
}