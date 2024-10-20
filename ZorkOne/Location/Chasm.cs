using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class Chasm : DarkLocationWithNoStartingItems, IDropSpecialLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.SW, new MovementParameters { Location = GetLocation<EastWestPassage>() } },
            { Direction.Up, new MovementParameters { Location = GetLocation<EastWestPassage>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<NorthSouthPassage>() } },
            { Direction.NE, new MovementParameters { Location = GetLocation<ReservoirSouth>() } }
        };

    protected override string ContextBasedDescription =>
        "A chasm runs southwest to northeast and the path follows it. You are on the south side of the " +
        "chasm, where a crack opens into a passage. ";

    public override string Name => "Chasm";

    public InteractionResult DropSpecial(IItem item, IContext context)
    {
        context.Items.Remove(item);
        item.CurrentLocation = null;
        string message = $"The {item.NounsForMatching[0]} drops out of sight and into the chasm. ";
        return new PositiveInteractionResult(message);
    }
}