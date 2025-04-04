using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using Utilities;
using ZorkOne.Command;

namespace ZorkOne.Location;

public class MaintenanceRoom : DarkLocation, ITurnBasedActor
{
    private readonly string[] _leakNouns =
        ["leak", "pipe", "water", "water leak", "stream", "crack", "break", "burst", "hole", "rupture", "fracture"];

    public override string Name => "Maintenance Room";

    [UsedImplicitly] public int CurrentWaterLevel { get; set; }

    public bool RoomFlooded { get; private set; }

    [UsedImplicitly] public bool LeakIsFixed { get; set; }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        CurrentWaterLevel++;

        if (CurrentWaterLevel >= 13)
        {
            RoomFlooded = true;
            context.RemoveActor(this);
        }

        // If we leave the room, the water level continues to rise, but
        // we don't die, and we're not notified of it. 
        if (context.CurrentLocation != this)
            return Task.FromResult(string.Empty);

        if (CurrentWaterLevel >= 13)
        {
            context.RemoveActor(this);
            return Task.FromResult(new DeathProcessor()
                .Process(
                    "I'm afraid you have done drowned yourself.\n",
                    context).InteractionMessage);
        }

        return Task.FromResult($"The water level here is now up to your {WaterLevel.Map[CurrentWaterLevel]}.");
    }

    private InteractionResult FixTheLeak(IContext context)
    {
        // Interestingly, in the original game, we still have the gunk
        // and there is no mention of the fixed leak in the room description, 
        // nor of the water level (even if it was at our heads).
        // Other than the jammed blue button, it's like it never happened. 

        LeakIsFixed = true;
        context.RemoveActor(this);
        return new PositiveInteractionResult(
            "By some miracle of Zorkian technology, you have managed to stop the leak in the dam. ");
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is what appears to have been the maintenance room " +
               "for Flood Control Dam #3. Apparently, this room has been " +
               "ransacked recently, for most of the valuable equipment is " +
               "gone. On the wall in front of you is a group of buttons " +
               "colored blue, yellow, brown, and red. There are doorways to " +
               "the west and south. \n";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters { Location = GetLocation<DamLobby>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<DamLobby>() } }
        };
    }

    public override void Init()
    {
        StartWithItem<Wrench>();
        StartWithItem<Screwdriver>();
        StartWithItem<ToolChests>();
        StartWithItem<Tube>();
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        var intentMatch = action.Match<ViscousMaterial>(Verbs.FixVerbs, _leakNouns,
            ["with", "using"]);

        intentMatch |= action.Match(Verbs.ApplyVerbs, GetItem<ViscousMaterial>().NounsForMatching, _leakNouns,
            ["on", "to", "against", "in"]);

        if (intentMatch)
            return FixTheLeak(context);

        return await base.RespondToMultiNounInteraction(action, context);
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
                        "brown button"
                    }.SingleLineListWithOr()}?",
                    new Dictionary<string, string>
                    {
                        { "brown", "brown button" },
                        { "yellow", "yellow button" },
                        { "red", "red button" },
                        { "blue", "blue button" }
                    },
                    "press {0}"
                );
        }

        return noun switch
        {
            "blue button" or "blue" => BlueClick(context),
            "red button" or "red" => RedClick(),
            "yellow button" or "yellow" => YellowClick(),
            "brown button" or "brown" => BrownClick(),

            _ => await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory)
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
        if (CurrentWaterLevel > 0 || LeakIsFixed)
            return new PositiveInteractionResult("The blue button appears to be jammed. ");

        context.RegisterActor(this);
        return new PositiveInteractionResult(
            "There is a rumbling sound and a stream of water appears to burst from the east wall " +
            "of the room (apparently, a leak has occurred in a pipe).");
    }
}

internal static class WaterLevel
{
    internal static readonly Dictionary<int, string> Map = new()
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