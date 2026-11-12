using GameEngine;
using GameEngine.Location;
using JetBrains.Annotations;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using Newtonsoft.Json;

namespace ZorkOne.Location;

public class CaveSouth : DarkLocationWithNoStartingItems, IThiefMayVisit, ITurnBasedActor
{
    // [JsonIgnore]: the chooser is behaviour, not state. Left in, it was written into every save made
    // after the cave was first loaded, and a save holding a chooser the serialiser cannot rebuild could
    // not be restored at all.
    [UsedImplicitly] [JsonIgnore]
    public IRandomChooser RandomChooser { get; set; } = new RandomChooser();

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
        if (context.CurrentLocation == this && context.HasItem<Candles>() && Repository.GetItem<Candles>().IsOn &&
            RandomChooser.RollDiceSuccess(2))
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
