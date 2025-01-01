using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Command;

namespace Planetfall.Location.Kalamontee;

public class Underwater : LocationWithNoStartingItems, ITurnBasedActor
{
    // ReSharper disable once MemberCanBePrivate.Global
    public bool HaveBeenUnderwater { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public byte TurnsUnderWater { get; set; }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.Up, Go<Crag>() }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "You are momentarily disoriented as you enter the turbulent waters. " +
        "Currents buffet you against the sharp rocks of an underwater cliff. A dim light filters down from above. ";

    public override string Name => "Underwater";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (HaveBeenUnderwater || TurnsUnderWater > 1)
        {
            context.RemoveActor(this);
            return Task.FromResult(new DeathProcessor()
                .Process("\n\nA mighty undertow drags you across some underwater obstructions. ", context)
                .InteractionMessage);
        }

        TurnsUnderWater++;
        return Task.FromResult(string.Empty);
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        context.RegisterActor(this);
        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
        HaveBeenUnderwater = true;
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }
}