using GameEngine.Location;
using Model.AIGeneration;

namespace Planetfall.Location.Shuttle;

public abstract class ShuttleControl : LocationWithNoStartingItems, ITurnBasedActor
{
    //  It's already closed.                                                                                                                                                                               
    // The shuttle car hurtles past the platforms and rams into the wall at the far end of the station. The shuttle car is destroyed, but you're in no condition to care.                                 
    // A recorded voice says "Operator should remain in control cabin while shuttle car is between stations."                                                                                             

    [UsedImplicitly]
    public int Speed { get; set; }

    public abstract ILocation Cabin { get; }

    [UsedImplicitly]
    public bool SpeedChanged { get; set; }

    [UsedImplicitly]
    public ShuttleLeverPosition LeverPosition { get; set; } = ShuttleLeverPosition.Neutral;

    public async Task<string> Act(IContext context, IGenerationClient client)
    {
        SpeedChanged = false;

        if (LeverPosition != ShuttleLeverPosition.Neutral)
            return await ChangeSpeed();

        if (Speed != 0)
            return await Move();

        return string.Empty;
    }

    // A recorded voice says "Use other control cabin. Control activation overridden."                                                                                                                    
    // The control cabin door slides shut and the shuttle car begins to move forward! The display changes to 5.                                                                                           
    // The shuttle car continues to move. The display still reads 5.                                                                                                                                      
    // The tunnel levels out and begins to slope upward. A sign flashes by which reads "Hafwaa Mark -- Beegin Deeseluraashun."     
    // The shuttle car is approaching a brightly lit area. As you near it, you make out the concrete platforms of a shuttle station.                                                                      

    // You pass a sign, surrounded by blinking red lights, which says "15."    
    // You pass a sign, surrounded by blinking red lights, which says "10."   

    // The lever is now in the lower position.                                                                                                                                                            
    // The shuttle car glides into the station and comes to rest at the concrete platform. You hear the cabin doors slide open.    
    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a small control cabin. A control panel contains a slot, a lever, and a display. The lever can be " +
            "set at a central position, or it could be pushed up to a position labelled \"+\", or pulled down to a " +
            "position labelled \"-\". It is currently at the center setting. The display, a digital readout, currently " +
            "reads 0. Through the cabin window you can see a featureless concrete wall. ";

        // Through the cabin window you can see parallel rails running along the   
        // floor of a long tunnel, vanishing in the distance.                                                                        
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
}