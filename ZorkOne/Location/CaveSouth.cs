using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class CaveSouth : DarkLocationWithNoStartingItems, IThiefMayVisit, ITurnBasedActor
{
    private readonly IRandomChooser _randomChooser;

    public CaveSouth() : this(new RandomChooser())
    {
    }

    // Constructor with injected randomChooser for unit testing only
    public CaveSouth(IRandomChooser randomChooser)
    {
        _randomChooser = randomChooser;
    }

    public override string Name => "Cave";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, new MovementParameters { Location = GetLocation<WindingPassage>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<MirrorRoomSouth>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<EntranceToHades>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a tiny cave with entrances west and north, and a dark, forbidding staircase leading down. ";
    }

    public override async Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var returnValue = "";
        
        context.RegisterActor(this);

        returnValue += LocationHelper.CheckSwordGlowingFaintly<Spirits, EntranceToHades>(context);
        return !string.IsNullOrWhiteSpace(returnValue)
            ? returnValue
            : await base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public async Task<string> Act(IContext context, IGenerationClient client)
    {
        if (context.CurrentLocation == this && context.HasItem<Candles>() && Repository.GetItem<Candles>().IsOn && _randomChooser.RollDice(2))
        {
            var result = await new TurnOnOrOffProcessor().Process(
                new SimpleIntent { Noun = "candles", Verb = "turn off" }, context, Repository.GetItem<Candles>(),
                client);
            return "\nA gust of wind blows out your candles! " + result?.InteractionMessage;
        }

        return string.Empty;
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }
}
