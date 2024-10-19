using GameEngine;
using GameEngine.Item.ItemProcessor;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class CaveSouth : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<WindingPassage>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<MirrorRoomSouth>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<EntranceToHades>() } }
        };

    protected override string ContextBasedDescription =>
        "This is a tiny cave with entrances west and north, and a dark, forbidding staircase leading down. ";

    public override string Name => "Cave";

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var returnValue = "";
        var litCandlesInPossession = context.HasItem<Candles>() && Repository.GetItem<Candles>().IsOn;

        if (litCandlesInPossession)
        {
            var result = new TurnLightOnOrOffProcessor().Process(
                new SimpleIntent { Noun ="candles", Verb = "turn off" }, context, Repository.GetItem<Candles>(),
                null!);
            returnValue += "\nA gust of wind blows out your candles! " + result!.InteractionMessage;
        }

        returnValue += this.CheckSwordGlowingFaintly<Spirits, EntranceToHades>(context);
        return !string.IsNullOrWhiteSpace(returnValue) ? Task.FromResult(returnValue) : base.AfterEnterLocation(context, previousLocation, generationClient);
    }
}