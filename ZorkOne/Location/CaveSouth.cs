using Game.Item.ItemProcessor;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class CaveSouth : DarkLocation
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

    public override string AfterEnterLocation(IContext context, ILocation previousLocation)
    {
        var returnValue = "";
        var swordInPossession = context.HasItem<Sword>();
        var litCandlesInPossession = context.HasItem<Candles>() && Repository.GetItem<Candles>().IsOn;
        
        Spirits spirits = Repository.GetItem<Spirits>();
        var entranceToHades = Repository.GetLocation<EntranceToHades>();
        var spiritsAlive = spirits.CurrentLocation == entranceToHades;

        // TODO: Make this an actor, with a random chance (50%) to blow out the candles. I think that's what Zork does, needs to check source code. 
        // TODO: If your candles get blown out and it was the only light source, "It is now completely dark" 
        // https://github.com/historicalsource/zork1/blob/7d54d16fca7a5dd7c6191c93651aad925f8c0922/1actions.zil#L2424
        if (litCandlesInPossession)
        {
            var result = new TurnLightOnOrOffProcessor().Process(
                new SimpleIntent { Noun ="candles", Verb = "turn off" }, context, Repository.GetItem<Candles>(),
                null!);
            returnValue += "\nA gust of wind blows out your candles! " + result!.InteractionMessage;
        }

        if (spiritsAlive && swordInPossession)
            returnValue += "\nYour sword is glowing with a faint blue glow.";

        if (string.IsNullOrWhiteSpace(returnValue))
            return base.AfterEnterLocation(context, previousLocation);

        return returnValue;
    }

    public override void Init()
    {
    }
}