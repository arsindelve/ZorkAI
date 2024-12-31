using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Command;
using Utilities;

namespace Planetfall.Location.Feinstein;

internal class SafetyWeb : ItemBase, ISubLocation, ICanBeExamined
{
    public string GetIn(IContext context)
    {
        if (context.CurrentLocation.SubLocation == this)
            return "You're already in the safety web. ";

        context.CurrentLocation.SubLocation = this;
        return "You are now safely cushioned within the web. ";
    }
    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (context.CurrentLocation.SubLocation != null && action.MatchVerb(["get", "rest", "sit"]))
            return new PositiveInteractionResult(GetIn(context));

        if (context.CurrentLocation.SubLocation == null && action.MatchVerb(["leave", "exit", "get"]))
            return new PositiveInteractionResult(GetOut(context));

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public string GetOut(IContext context)
    {
        var escapePod = Repository.GetLocation<EscapePod>();

        if (context.CurrentLocation.SubLocation == null)
            return "You're not in the safety web. ";

        context.CurrentLocation.SubLocation = null;

        if (escapePod is { LandedSafely: true, TurnsAfterStanding: 0 })
        {
            escapePod.TurnsAfterStanding++;
            context.RegisterActor(escapePod);
            return "As you stand, the pod shifts slightly and you feel it falling. A moment later, " +
                   "the fall stops with a shock, and you see water rising past the viewport. ";
        }

        return "You are standing again. ";
    }

    public string LocationDescription => ", in the safety web";

    public string ExaminationDescription =>
        "The safety webbing fills most of the pod. It could accomodate from one to, perhaps, twenty people. ";

    public override string[] NounsForMatching => ["webbing", "safety webbing", "web"];
}

internal class EscapePod : LocationBase, ITurnBasedActor
{
    public bool LandedSafely { get; set; }

    public byte TurnsSinceExplosion { get; set; }

    public byte TurnsAfterStanding { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public ILocation WhereDoesTheDoorLead { get; set; } = Repository.GetLocation<DeckNine>();

    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        switch (input?.ToLowerInvariant().StripNonChars())
        {
            case "sit":
            case "sit down":
            case "get in":
                string inMessage = Repository.GetItem<SafetyWeb>().GetIn(context);
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult(inMessage));

            case "stand":
            case "stand up":
                string outMessage = Repository.GetItem<SafetyWeb>().GetOut(context);
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult(outMessage));
        }

        return base.RespondToSpecificLocationInteraction(input, context, client);
    }

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.Out,
                new MovementParameters
                {
                    Location = WhereDoesTheDoorLead,
                    CanGo = _ => Repository.GetItem<BulkheadDoor>().IsOpen,
                    CustomFailureMessage = "The pod door is closed. "
                }
            },
            {
                Direction.E,
                new MovementParameters
                {
                    Location = WhereDoesTheDoorLead,
                    CanGo = _ => Repository.GetItem<BulkheadDoor>().IsOpen,
                    CustomFailureMessage = "The pod door is closed. "
                }
            }
        };

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(3);
        context.RegisterActor(this);
        base.OnFirstTimeEnterLocation(context);
    }

    protected override string ContextBasedDescription =>
        $"This is one of the Feinstein's primary escape pods, for use in extreme emergencies. A mass of safety " +
        $"webbing, large enough to hold several dozen people, fills half the pod. The controls are entirely automated. " +
        $"The bulkhead leading out is {(Repository.GetItem<BulkheadDoor>().IsOpen ? "open" : "closed")}. ";

    public override string Name => "Escape Pod";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        var action = TurnsAfterStanding == 0 ? HandleBeingInSpaceAndLanding(context) : YerSinking(context);
        return Task.FromResult("\n\n" + action);
    }

    private string YerSinking(IContext context)
    {
        string action = "";

        // Starts to sink when you stand / exit the safety webbing. You used to not be able 
        // to open the door and leave without standing, but they seem to have changed that in the game. 
        // You can open the door and leaving without the pod really sinking. Weird. 
        if (TurnsAfterStanding > 0)
        {
            TurnsAfterStanding++;
            action = GetItem<BulkheadDoor>().IsOpen ? SinkingWithTheDoorOpen(context) : SinkingWithTheDoorClosed(context);
        }

        return action;
    }

    private string SinkingWithTheDoorOpen(IContext context)
    {
        return TurnsAfterStanding switch
        {
            1 => "As you stand, the pod shifts slightly and you feel it falling. A moment later, the fall stops with a shock, and you see water rising past the viewport. ",
            2 => "",
            3 => "The pod is now completely submerged, and you feel it smash against underwater rocks. Bubbles streaming upward past the window indicate that the pod is continuing to sink. ",
            4 => "",
            _ => Die("Between the swirling waters and the increasing pressure, it's curtains for you. Perhaps you should have left the pod a bit sooner. ", context)
        };
    }

    private string SinkingWithTheDoorClosed(IContext context)
    {
        return TurnsAfterStanding switch
        {
            1 => "As you stand, the pod shifts slightly and you feel it falling. A moment later, the fall stops with a shock, and you see water rising past the viewport. ",
            2 => "",
            3 => "The pod is now completely submerged, and you feel it smash against underwater rocks. Bubbles streaming upward past the window indicate that the pod is continuing to sink. ",
            4 => "The pod creaks ominously from the increasing pressure. ",
            _ => Die("The pod splits open, and water pours in. ", context)
        };
    }

    private string Die(string deathText, IContext context)
    {
        context.RemoveActor(this);
        return new DeathProcessor().Process(deathText, context).InteractionMessage;
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation, IGenerationClient generationClient)
    {
        // Now safely in the pod, swap the explosion actor for the escape pod actor. 
        context.RemoveActor<ExplosionCoordinator>();
        context.RegisterActor(this);
        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    private string HandleBeingInSpaceAndLanding(IContext context)
    {
        TurnsSinceExplosion++;

        if (context.CurrentLocation is not EscapePod)
            return string.Empty;

        string action = "";

        switch (TurnsSinceExplosion)
        {
            case 1:
                action = "The ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing. ";
                break;

            case 2:
                Repository.GetItem<BulkheadDoor>().IsOpen = false;
                action = "The pod door clangs shut as heavy explosions continue to buffet the Feinstein. ";
                break;

            case 3:
                context.SystemPromptAddendum =
                    "The Feinstein has exploded from an unknown accident, and the player is now hurtling through space " +
                    "in a fully automated escape pod, looking for a place to land. ";
                
                action = "You feel the pod begin to slide down its ejection tube as explosions shake the mother ship. ";
                break;

            case 4:
                // TODO: If not in web, You are thrown against the bulkhead, bruising a few limbs. The safety webbing might have offered a bit more protection.
                // TODO: If not in web, chance of: You are thrown against the bulkhead, head first. It seems that getting in the safety webbing would have been a good idea.

                action =
                    "Through the viewport of the pod you see the Feinstein dwindle as you head away. Bursts of light " +
                    "dot its hull. Suddenly, a huge explosion blows the Feinstein into tiny pieces, sending the " +
                    "escape pod tumbling away! \n\nAs the escape pod tumbles away from the former location of " +
                    "the Feinstein, its gyroscopes whine. The pod slowly stops tumbling. Lights on the control " +
                    "panel blink furiously as the autopilot searches for a reasonable destination. ";
                break;

            case 5:
                action = "The auxiliary rockets fire briefly, and a nearby planet swings into view through the port. " +
                         "It appears to be almost entirely ocean, with just a few visible islands and an unusually " +
                         "small polar ice cap. A moment later, the system's sun swings into view, and the viewport " +
                         "polarizes into a featureless black rectangle. ";
                break;

            case 6:
                action = "The main thrusters fire a long, gentle burst. A monotonic voice issues from " +
                         "the control panel. \"Approaching planet...human-habitable.\" ";
                break;

            case 10:
                action = "The pod is buffeted as it enters the planet's atmosphere. ";
                break;

            case 11:
                action = "You feel the temperature begin to rise, and the pod's climate control system roars " +
                         "as it labors to compensate. ";
                break;

            case 12:
                action = "The viewport suddenly becomes transparent again, giving you a view of endless ocean below. " +
                         "The lights on the control panel flash madly as the pod's computer searches for a " +
                         "suitable landing site. The thrusters fire long and hard, slowing the pod's descent. ";
                break;

            case 13:
                action = "The pod is now approaching the closer of a pair of islands. It appears to be surrounded " +
                         "by sheer cliffs rising from the water, and is topped by a wide plateau. The plateau " +
                         "seems to be covered by a sprawling complex of buildings. ";
                break;

            case 14:

                context.RemoveActor(this);
                if (context.CurrentLocation.SubLocation is SafetyWeb)
                {
                    action =
                        "The pod lands with a thud. Through the viewport you can see a rocky cleft and some water " +
                        "below. The pod rocks gently back and forth as if it was precariously balanced. A previously " +
                        "unseen panel slides open, revealing some emergency provisions, including a survival " +
                        "kit and a towel.";

                    ItemPlacedHere<Towel>();
                    ItemPlacedHere<SurvivalKit>();

                    WhereDoesTheDoorLead = Repository.GetLocation<Underwater>();
                    LandedSafely = true;
                }

                else
                {
                    string death = "The pod, whose automated controls were unfortunately designed by computer scientists, " +
                             "lands with a good deal of force. Your body sails across the pod until it is stopped by " +
                             "one of the sharper corners of the control panel. ";

                    action = new DeathProcessor().Process(death, context).InteractionMessage;
                }

                break;
        }

        return action;
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
    }

    public override void Init()
    {
        StartWithItem<BulkheadDoor>();
    }
}

// NEXT: 
//
// Escape Pod, in the safety web
// This is one of the Feinstein's primary escape pods, for use in extreme emergencies. A mass of safety webbing, large enough to hold several dozen people, fills half the pod. The controls are entirely automated. The bulkhead leading out is closed.
//     There is a towel here. (outside the safety web)
// There is a survival kit here. (outside the safety web)
//
//     >examine kit
// The survival kit is closed.
