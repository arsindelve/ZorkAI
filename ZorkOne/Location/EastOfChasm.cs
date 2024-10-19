using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class EastOfChasm : LocationWithNoStartingItems
{
    public override string Name => "East of Chasm";

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        string? glow = this.CheckSwordNoLongerGlowing<Troll, TrollRoom, Cellar>(previousLocation, context);
        return !string.IsNullOrEmpty(glow) ? Task.FromResult(glow) : base.AfterEnterLocation(context, previousLocation, generationClient);
    }
    
    protected override string ContextBasedDescription =>
        "You are on the east edge of a chasm, the bottom of which cannot be seen. A narrow passage goes north, " +
        "and the path you are on continues to the east. ";

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<Cellar>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Gallery>() } }
        };
}