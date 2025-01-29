using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

internal class EastWestPassage : DarkLocationWithNoStartingItems, IThiefMayVisit
{
    public override string Name => "East-West Passage";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, new MovementParameters { Location = GetLocation<TrollRoom>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<Chasm>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<Chasm>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a narrow east-west passageway. There is a narrow stairway leading down at the north end of the room. ";
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var swordInPossession = context.HasItem<Sword>();
        var trollIsAlive = Repository.GetItem<Troll>().CurrentLocation == Repository.GetLocation<TrollRoom>();

        if (trollIsAlive && swordInPossession && previousLocation is Cellar)
            return Task.FromResult("\nYour sword is glowing with a faint blue glow.");

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(5);
        base.OnFirstTimeEnterLocation(context);
    }
}