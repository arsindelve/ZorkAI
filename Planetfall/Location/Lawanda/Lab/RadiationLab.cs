using GameEngine.Location;

namespace Planetfall.Location.Lawanda.Lab;

internal class RadiationLab : LocationBase
{
    public override string Name => "Radiation Lab";

    public override void Init()
    {
        // TODO: There is a powerful portable lamp here, currently off.
        // TODO: Sitting on a long table is a small brown spool.
    }
    
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<RadiationLockEast>() }
        };
    }

    // TODO: >examine crack // The crack is too small to go through, but large enough to look through.
    // TODO: >look through crack // You see a dimly lit Bio Lab. Sinister shapes lurk about within.
    // TODO: >examine equipment // The equipment here is so complicated that you couldn't even begin to figure out how to operate it.
    // TODO: >>turn on lamp // The lamp is now producing a bright light.

    // TODO: (Five Turns) You suddenly feel sick and dizzy. 
    // TODO: (Six Turns) You feel incredibly nauseous and begin vomiting. Also, all your hair has fallen out.
    // TODO: (Seven Turns) It seems you have picked up a bad case of radiation poisoning. DEATH
    
    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This room is filled with unfathomable equipment and large canisters bearing radioactive warnings. Many of " +
            "the canisters are split open. You can see the Bio Lab through a large crack in the south wall. " +
            "Sinister-looking forms move about within the Bio Lab. Although the lights here are off, the room is " +
            "suffused with a pale blue glow. ";
    }
}