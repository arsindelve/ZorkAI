using Model.Intent;
using OpenAI;

namespace ZorkOne.Location;

public class MaintenanceRoom : DarkLocation
{
    public override string Name => "Maintenance Room";

    protected override string ContextBasedDescription => "This is what appears to have been the maintenance room " +
                                                         "for Flood Control Dam #3. Apparently, this room has been " +
                                                         "ransacked recently, for most of the valuable equipment is " +
                                                         "gone. On the wall in front of you is a group of buttons " +
                                                         "colored blue, yellow, brown, and red. There are doorways to " +
                                                         "the west and south.";

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<DamLobby>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<DamLobby>() } }
        };

    public override void Init()
    {
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        string[] verbs = ["push", "press", "activate", "toggle"];
        var verb = action.Verb.ToLowerInvariant().Trim();
        var noun = action.Noun?.ToLowerInvariant().ToLowerInvariant().Trim();

        if (!verbs.Contains(verb))
            return base.RespondToSimpleInteraction(action, context, client);

        return noun switch
        {
            "blue button" or "blue" => BlueClick(),
            "red button" or "red" => RedClick(),
            "yellow button" or "yellow" => YellowClick(),
            "brown button" or "brown" => BrownClick(),

            _ => base.RespondToSimpleInteraction(action, context, client)
        };
    }

    private InteractionResult BrownClick()
    {
        GetItem<ControlPanel>().GreenBubbleGlowing = false;
        return new PositiveInteractionResult("Click. ");
    }

    private InteractionResult YellowClick()
    {
        GetItem<ControlPanel>().GreenBubbleGlowing = true;
        return new PositiveInteractionResult("Click. ");
    }

    private InteractionResult RedClick()
    {
        if (IsNoLongerDark)
        {
            IsNoLongerDark = false;
            return new PositiveInteractionResult("The lights within the room shut off. ");
        }

        IsNoLongerDark = true;
        return new PositiveInteractionResult("The lights within the room come on. ");
    }

    private InteractionResult BlueClick()
    {
        throw new NotImplementedException();
    }
}