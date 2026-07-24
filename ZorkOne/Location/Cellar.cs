using GameEngine;
using GameEngine.IntentEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class Cellar : DarkLocation
{
    public override string Name => "Cellar";

    public override string[] NounsForMatching => ["basement"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // The trap door gates the passage up to the living room. Declaring it as the GatingItem lets
        // "enter/exit trap door" resolve to this exit (DoorReroute), no In alias needed. (issue #262)
        var trapDoorPassage = new MovementParameters
        {
            GatingItem = GetItem<TrapDoor>(),
            CanGo = _ => GetItem<TrapDoor>().IsOpen,
            CustomFailureMessage = "The trap door is closed. ",
            Location = GetLocation<LivingRoom>()
        };

        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.S, new MovementParameters { Location = GetLocation<EastOfChasm>() }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<TrollRoom>() }
            },
            {
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "You try to ascend the ramp, but it is impossible, and you slide back down."
                }
            },
            { Direction.Up, trapDoorPassage }
        };
    }

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        // Issue #386: the ramp is the (unclimbable) West exit, but the parser maps the verb "climb"
        // (and "go up") to Direction.Up - the trap door - so "climb ramp" / "go up ramp" returned "The
        // trap door is closed." for a completely different feature. The original SLIDE-FUNCTION
        // (zork1/1actions.zil:3086) routes any climb of the slide/ramp/chute in the Cellar to the West
        // exit, so reroute those here to Direction.W, giving the same result as "go west".
        //
        // Substring Contains (rather than exact-phrase matching like Kitchen's window handling) is
        // intentional: it tolerates the ZIL synonyms/adjectives ("steep metal ramp", "twisting slide")
        // without enumerating them, and is safe because the Cellar has no other noun containing these.
        var normalized = input?.ToLowerInvariant().Trim().Replace("the ", "") ?? "";
        var mentionsRamp = normalized.Contains("ramp") || normalized.Contains("slide") ||
                           normalized.Contains("chute");
        var isClimbOrGoUp = normalized.StartsWith("climb") || normalized.StartsWith("ascend") ||
                            normalized.StartsWith("go up") || normalized.StartsWith("up ");
        if (mentionsRamp && isClimbOrGoUp)
        {
            var (resultObject, resultMessage) =
                await new MoveEngine().Process(new MoveIntent { Direction = Direction.W }, context, client);
            return resultObject ?? new PositiveInteractionResult(resultMessage);
        }

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in a dark and damp cellar with a narrow passageway " +
               "leading north, and a crawlway to the south. On the west is the " +
               "bottom of a steep metal ramp which is unclimbable. ";
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        var result = base.BeforeEnterLocation(context, previousLocation);

        if (Repository.GetItem<TrapDoor>().IsOpen && VisitCount == 1)
        {
            Repository.GetItem<TrapDoor>().IsOpen = false;
            result += "The trap door crashes shut, and you hear someone barring it." + Environment.NewLine +
                      Environment.NewLine;
        }

        return result;
    }

    public override void Init()
    {
        StartWithItem<TrapDoor>();
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var glow = LocationHelper.CheckSwordGlowingFaintly<Troll, TrollRoom>(context);
        return !string.IsNullOrEmpty(glow)
            ? Task.FromResult(glow)
            : base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(25);
        base.OnFirstTimeEnterLocation(context);
    }
}