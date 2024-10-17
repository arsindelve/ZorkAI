using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using ZorkOne.Command;

namespace ZorkOne.Location;

public class MaintenanceRoom : DarkLocation, ITurnBasedActor
{
    public override string Name => "Maintenance Room";

    public int CurrentWaterLevel { get; set; }

    protected override string ContextBasedDescription => "This is what appears to have been the maintenance room " +
                                                         "for Flood Control Dam #3. Apparently, this room has been " +
                                                         "ransacked recently, for most of the valuable equipment is " +
                                                         "gone. On the wall in front of you is a group of buttons " +
                                                         "colored blue, yellow, brown, and red. There are doorways to " +
                                                         "the west and south. \n";

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<DamLobby>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<DamLobby>() } }
        };

    public bool RoomFlooded { get; set; }

    public bool LeakIsFixed { get; set; }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        CurrentWaterLevel++;

        if (CurrentWaterLevel >= 13)
        {
            RoomFlooded = true;
            context.RemoveActor(this);
        }

        // If we leave the room, the water level continues to rise, but
        // we cannot die, and we're not notified of it. 
        if (context.CurrentLocation != this)
            return Task.FromResult(string.Empty);

        if (CurrentWaterLevel >= 13)
        {
            context.RemoveActor(this);
            context.RemoveActor(Repository.GetItem<Troll>());
            return Task.FromResult(new DeathProcessor()
                .Process(
                    "I'm afraid you have done drowned yourself.\n",
                    context).InteractionMessage);
        }

        return Task.FromResult($"The water level here is now up to your {WaterLevel.Map[CurrentWaterLevel]}.");
    }

    public override void Init()
    {
        StartWithItem<Wrench>(this);
        StartWithItem<Screwdriver>(this);
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        string[] verbs = ["push", "press", "activate", "toggle"];
        var verb = action.Verb.ToLowerInvariant().Trim();
        var noun = action.Noun?.ToLowerInvariant().ToLowerInvariant().Trim();

        if (!verbs.Contains(verb))
            return base.RespondToSimpleInteraction(action, context, client);

        // If they said "blue button", simplify and replace "button" with "blue"
        if (action.MatchNoun(["button"]) && !string.IsNullOrEmpty(action.Adjective))
            noun = action.Adjective.ToLowerInvariant();
        
        return noun switch
        {
            "blue button" or "blue" => BlueClick(context),
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

    private InteractionResult BlueClick(IContext context)
    {
        if (LeakIsFixed)
            return new PositiveInteractionResult("The blue button appears to be jammed. ");

        context.RegisterActor(this);
        return new PositiveInteractionResult(
            "There is a rumbling sound and a stream of water appears to burst from the east wall of the room (apparently, a leak has occurred in a pipe).");
    }
}

internal static class WaterLevel
{
    internal static Dictionary<int, string> Map = new()
    {
        { 1, "ankle" },
        { 2, "shin" },
        { 3, "shin" },
        { 4, "knees" },
        { 5, "knees" },
        { 6, "hips" },
        { 7, "hips" },
        { 8, "waist" },
        { 9, "waist" },
        { 10, "chest" },
        { 11, "chest" },
        { 12, "neck" },
        { 13, "neck" }
    };
}