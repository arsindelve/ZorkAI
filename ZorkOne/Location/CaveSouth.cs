using GameEngine;
using GameEngine.Item.ItemProcessor;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class CaveSouth : DarkLocationWithNoStartingItems, IThiefMayVisit
{
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

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var returnValue = "";
        var litCandlesInPossession = context.HasItem<Candles>() && Repository.GetItem<Candles>().IsOn;

        if (litCandlesInPossession)
        {
            var result = new TurnOnOrOffProcessor().Process(
                new SimpleIntent { Noun = "candles", Verb = "turn off" }, context, Repository.GetItem<Candles>(),
                null!);
            returnValue += "\nA gust of wind blows out your candles! " + result!.InteractionMessage;
        }

        returnValue += LocationHelper.CheckSwordGlowingFaintly<Spirits, EntranceToHades>(context);
        return !string.IsNullOrWhiteSpace(returnValue)
            ? Task.FromResult(returnValue)
            : base.AfterEnterLocation(context, previousLocation, generationClient);
    }
}