using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class WindingPassage : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<MirrorRoomSouth>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<CaveSouth>() } }
        };
    
    protected override string ContextBasedDescription =>
        "This is a winding passage. It seems that there are only exits on the east and north. ";

    public override string Name => "Winding Passage";

    public override void Init()
    {
    }
    
    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        string? glow = this.CheckSwordNoLongerGlowing<Spirits, EntranceToHades, CaveSouth>(previousLocation, context);
        return !string.IsNullOrEmpty(glow) ? Task.FromResult(glow) : base.AfterEnterLocation(context, previousLocation, generationClient);
    }
}