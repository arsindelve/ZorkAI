using GameEngine.Location;
using Model.AIGeneration;

namespace Planetfall.Location.Shuttle;

/// <summary>
/// Represents the base class for a shuttle control system within a location.
/// Implements turn-based actor behavior and responds to player interactions.
/// </summary>
public abstract class ShuttleControl : LocationWithNoStartingItems, ITurnBasedActor
{
    private static readonly string[] AccelerateVerbs = ["push"];
    private static readonly string[] DecelerateVerbs = ["pull"];
    private static readonly string[] LeverNouns = ["lever", "controls", "control lever"];

    [UsedImplicitly] public int Speed { get; set; }

    public int TurnsSinceActivated { get; set; }

    [UsedImplicitly] public virtual int TunnelPosition { get; set; }

    [UsedImplicitly] public bool Activated { get; set; }

    [UsedImplicitly] public bool SpeedChanged { get; set; }

    [UsedImplicitly] public ShuttleLeverPosition LeverPosition { get; set; } = ShuttleLeverPosition.Neutral;

    public async Task<string> Act(IContext context, IGenerationClient client)
    {
        SpeedChanged = false;

        if (LeverPosition != ShuttleLeverPosition.Neutral)
            return await ChangeSpeed();

        if (Speed != 0)
            return await Move();

        return string.Empty;
    }

    public override Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(DecelerateVerbs, LeverNouns))
            return Task.FromResult<InteractionResult>(
                new PositiveInteractionResult(AdjustLever(ShuttleLeverDirection.Pull)));

        if (action.Match(AccelerateVerbs, LeverNouns))
            return Task.FromResult<InteractionResult>(
                new PositiveInteractionResult(AdjustLever(ShuttleLeverDirection.Push)));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }


    // The shuttle car glides into the station and comes to rest at the concrete platform. You hear the cabin doors slide open.    


    // At 5 or 10, 15 MPH
    // The shuttle car rumbles through the station and smashes into the wall at the far end. You are thrown forward into the control panel. Both you
    // and the shuttle car produce unhealthy crunching sounds as the cabin doors creak slowly open.

    // >= 20 
    // The shuttle car hurtles past the platforms and rams into the wall at the far end of the station. The shuttle car is destroyed, but you're in no condition to care.                                 


    // A recorded voice says "Use other control cabin. Control activation overridden."                                                                                                                    
    // The control cabin door slides shut and the shuttle car begins to move forward! The display changes to 5.                                                                                           
    // The shuttle car continues to move. The display still reads 5.                                                                                                                                      
    // The tunnel levels out and begins to slope upward. A sign flashes by which reads "Hafwaa Mark -- Beegin Deeseluraashun."     
    // The shuttle car is approaching a brightly lit area. As you near it, you make out the concrete platforms of a shuttle station.                                                                      

    // You pass a sign, surrounded by blinking red lights, which says "15."    
    // You pass a sign, surrounded by blinking red lights, which says "10."   


    // It's already closed.      


    // A recorded voice says "Operator should remain in control cabin while shuttle car is between stations."    


    private string AdjustLever(ShuttleLeverDirection direction)
    {
        // The lever is now in the lower position.     
        // The lever immediately pops back to the central position.

        if (!Activated)
            return "A recorded voice says \"Shuttle controls are not currently activated.\" ";

        // Every time we move the lever, it resets the clock. 
        TurnsSinceActivated = 0;

        switch (direction)
        {
            case ShuttleLeverDirection.Pull:
                return "";

            case ShuttleLeverDirection.Push:
                switch (LeverPosition)
                {
                    case ShuttleLeverPosition.Acceleration:
                        return "The lever is already in the upper position. ";
                    case ShuttleLeverPosition.Neutral:
                        return "The lever is now in the upper position. ";
                    case ShuttleLeverPosition.Deceleration:
                        return "The lever is now in the central position. ";
                }

                break;
        }

        return "";
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a small control cabin. A control panel contains a slot, a lever, and a display. The lever can be " +
            "set at a central position, or it could be pushed up to a position labelled \"+\", or pulled down to a " +
            "position labelled \"-\". It is currently at the center setting. The display, a digital readout, currently " +
            $"reads {Speed}. Through the cabin window you can see {OutTheWindow()} ";
    }

    private string OutTheWindow()
    {
        switch (TunnelPosition)
        {
            case 0:
                return "parallel rails running along the floor of a long tunnel, vanishing in the distance. ";
            case 200:
                return "a featureless concrete wall. ";
            default:
                return "";
        }
    }

    private Task<string> ChangeSpeed()
    {
        if (LeverPosition == ShuttleLeverPosition.Acceleration)
        {
            Speed += 5;
            SpeedChanged = true;
        }

        if (LeverPosition == ShuttleLeverPosition.Deceleration)
        {
            Speed -= 5;
            SpeedChanged = true;
        }

        return Task.FromResult(string.Empty);
    }

    private Task<string> Move()
    {
        if (SpeedChanged)
        {
            if (Speed == 0)
            {
                LeverPosition = ShuttleLeverPosition.Neutral;
                return Task.FromResult(
                    "The shuttle car comes to a stop and the lever pops back to the central position. ");
            }

            return Task.FromResult(
                $"The shuttle car continues to move. The display blinks, and now reads {Speed}. ");
        }

        return Task.FromResult($"The shuttle car continues to move. The display still reads {Speed}. ");
    }

    internal InteractionResult Activate()
    {
        if (TunnelPosition == 200)
            return new PositiveInteractionResult(
                "A recorded voice says \"Use other control cabin. Control activation overridden.\"");

        var result = Activated
            ? "\"A recorded voice says \"Shuttle controls are already activated.\""
            : "A recording of a deep male voice says \"Shuttle controls activated.\"";

        Activated = true;
        TurnsSinceActivated = 0;
        return new PositiveInteractionResult(result);
    }
}