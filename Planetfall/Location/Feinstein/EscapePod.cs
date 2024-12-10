using GameEngine.Location;
using Model.AIGeneration;
using Model.Location;
using Model.Movement;
using Planetfall.Command;
using Planetfall.Item.Feinstein;

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
        if (context.CurrentLocation.SubLocation == null)
            return "You're not in the safety web. ";

        context.CurrentLocation.SubLocation = null;
        return "You are standing again. ";
    }

    public string LocationDescription => ", in the safety web";

    public string ExaminationDescription =>
        "The safety webbing fills most of the pod. It could accomodate from one to, perhaps, twenty people. ";

    public override string[] NounsForMatching => ["webbing", "safety webbing", "web"];
}

internal class EscapePod : LocationBase, ITurnBasedActor
{
    public byte TurnsInEscapePod { get; set; }

    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        switch (input)
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
                    Location = Repository.GetLocation<DeckNine>(),
                    CanGo = _ => Repository.GetItem<BulkheadDoor>().IsOpen,
                    CustomFailureMessage = "The pod door is closed. "
                }
            },
            {
                Direction.E,
                new MovementParameters
                {
                    Location = Repository.GetLocation<DeckNine>(),
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
        $"The bulkhead leading out is {(Repository.GetItem<BulkheadDoor>().IsOpen ? "open" : "closed")}.";

    public override string Name => "Escape Pod";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        TurnsInEscapePod++;
        string action = "";

        switch (TurnsInEscapePod)
        {
            case 2:
            {
                context.RemoveActor(Repository.GetLocation<DeckNine>());
                Repository.GetItem<BulkheadDoor>().IsOpen = false;
                action =
                    "More powerful explosions buffet the ship. The lights flicker madly, and the escape-pod bulkhead clangs shut. ";
                break;
            }
            case 3:
                action = "Explosions continue to rock the ship. ";
                break;
            case 4:
                action = "You feel the pod begin to slide down its ejection tube as explosions shake the mother ship. ";
                break;
            case 5:
                // TODO: If not in web, You are thrown against the bulkhead, bruising a few limbs. The safety webbing might have offered a bit more protection.
                // TODO: If not in web, chance of: You are thrown against the bulkhead, head first. It seems that getting in the safety webbing would have been a good idea.

                action =
                    "Through the viewport of the pod you see the Feinstein dwindle as you head away. Bursts of light " +
                    "dot its hull. Suddenly, a huge explosion blows the Feinstein into tiny pieces, sending the " +
                    "escape pod tumbling away! \n\nAs the escape pod tumbles away from the former location of " +
                    "the Feinstein, its gyroscopes whine. The pod slowly stops tumbling. Lights on the control " +
                    "panel blink furiously as the autopilot searches for a reasonable destination. ";
                break;

            case 6:
                action = "The auxiliary rockets fire briefly, and a nearby planet swings into view through the port. " +
                         "It appears to be almost entirely ocean, with just a few visible islands and an unusually " +
                         "small polar ice cap. A moment later, the system's sun swings into view, and the viewport " +
                         "polarizes into a featureless black rectangle. ";
                break;

            case 7:
                action = "The main thrusters fire a long, gentle burst. A monotonic voice issues from " +
                         "the control panel. \"Approaching planet...human-habitable.\" ";
                break;

            case 11:
                action = "The pod is buffeted as it enters the planet's atmosphere. ";
                break;

            case 12:
                action = "You feel the temperature begin to rise, and the pod's climate control system roars " +
                         "as it labors to compensate. ";
                break;

            case 13:
                action = "The viewport suddenly becomes transparent again, giving you a view of endless ocean below. " +
                         "The lights on the control panel flash madly as the pod's computer searches for a " +
                         "suitable landing site. The thrusters fire long and hard, slowing the pod's descent. ";
                break;

            case 14:
                action = "The pod is now approaching the closer of a pair of islands. It appears to be surrounded " +
                         "by sheer cliffs rising from the water, and is topped by a wide plateau. The plateau " +
                         "seems to be covered by a sprawling complex of buildings. ";
                break;

            case 15:

                context.RemoveActor(this);
                if (context.CurrentLocation.SubLocation is SafetyWeb)
                {
                    action =
                        "The pod lands with a thud. Through the viewport you can see a rocky cleft and some water " +
                        "below. The pod rocks gently back and forth as if it was precariously balanced. A previously " +
                        "unseen panel slides open, revealing some emergency provisions, including a survival " +
                        "kit and a towel.";

                    ItemPlacedHere<Towel>();
                }

                // TODO: Add towel and kit 
                else
                {
                    action = "The pod, whose automated controls were unfortunately designed by computer scientists, " +
                             "lands with a good deal of force. Your body sails across the pod until it is stopped by " +
                             "one of the sharper corners of the control panel. ";

                    return Task.FromResult(new DeathProcessor().Process(action, context).InteractionMessage);
                }

                break;
        }


        return Task.FromResult("\n\n" + action);
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
//     >examine web
//     The safety webbing fills most of the pod. It could accomodate from one to, perhaps, twenty people.
//
//     >examine kit
// The survival kit is closed.
//
//     >examine towel
//     A pretty ordinary towel. Something is written in its corner.
//
//     >read towel
//     (Taking the towel first)
// "S.P.S. FEINSTEIN
// Escape Pod #42
// Don't Panic!"