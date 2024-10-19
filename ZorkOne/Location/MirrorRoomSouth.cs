using GameEngine;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

internal class MirrorRoomSouth : MirrorRoom
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<NarrowPassage>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<CaveSouth>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<WindingPassage>() } }
        };
    
    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        string? glow = this.CheckSwordNoLongerGlowing<Spirits, EntranceToHades, CaveSouth>(previousLocation, context);
        return !string.IsNullOrEmpty(glow) ? Task.FromResult(glow) : base.AfterEnterLocation(context, previousLocation, generationClient);
    }
    
    public override void Init()
    {
        StartWithItem<Mirror>();
    }
}