using GameEngine.Location;
using Model.AIGeneration;
using Model.Location;
using Model.Movement;
using Planetfall.Command;

namespace Planetfall.Location.Kalamontee;

public class Underwater : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map => new();

    protected override string ContextBasedDescription =>
        "You are momentarily disoriented as you enter the turbulent waters. " +
        "Currents buffet you against the sharp rocks of an underwater cliff. A dim light filters down from above. ";

    public override string Name => "Underwater";

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        return Task.FromResult(new DeathProcessor()
            .Process("A mighty undertow drags you across some underwater obstructions. ", context)
            .InteractionMessage);
    }
}