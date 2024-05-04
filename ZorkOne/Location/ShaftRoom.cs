using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

internal class ShaftRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.W, new MovementParameters { Location = GetLocation<BatRoom>() }
            },
            {
                Direction.Down,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "You wouldn't fit and would die if you could. " }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<SmellyRoom>() }
            }
        };

    protected override string ContextBasedDescription =>
        "This is a large room, in the middle of which is a small shaft descending through the floor into darkness " +
        "below. To the west and the north are exits from this room. Constructed over the top of the shaft is a metal " +
        "framework to which a heavy iron chain is attached.\nAt the end of the chain is a basket. \n"
        + (GetItem<Basket>().Items.Any() ? GetItem<Basket>().ItemListDescription("basket") : "");

    public override string Name => "Shaft Room";

    public override string AfterEnterLocation(IContext context, ILocation previousLocation)
    {
        var swordInPossession = context.HasItem<Sword>();

        if (swordInPossession)
            return "\nYour sword is glowing with a faint blue glow.";

        return string.Empty;
    }

    public override void Init()
    {
        StartWithItem<Chain>(this);
        StartWithItem<Basket>(this);
    }
}