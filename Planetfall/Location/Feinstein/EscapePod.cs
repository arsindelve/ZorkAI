using GameEngine.Location;
using Model.AIGeneration;
using Model.Location;
using Model.Movement;
using Planetfall.Command;
using Planetfall.Item;
using Planetfall.Item.Feinstein;

namespace Planetfall.Location.Feinstein;

internal class EscapePod : BaseLocation, ITurnBasedActor
{
    public byte TurnsInEscapePod { get; set; }

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
                if (false)
                    action =
                        "The pod lands with a thud. Through the viewport you can see a rocky cleft and some water " +
                        "below. The pod rocks gently back and forth as if it was precariously balanced. A previously " +
                        "unseen panel slides open, revealing some emergency provisions, including a survival " +
                        "kit and a towel.";
                else
                {
                    action = "The pod, whose automated controls were unfortunately designed by computer scientists, " +
                             "lands with a good deal of force. Your body sails across the pod until it is stopped by " +
                             "one of the sharper corners of the control panel. ";
                    return Task.FromResult(new DeathProcessor().Process(action, context).InteractionMessage);
                }

                break;
        }

        // TODO: Add towel and kit 

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