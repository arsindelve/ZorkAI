using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class Chasm : DarkLocationWithNoStartingItems, IDropSpecialLocation
{
    public override string Name => "Chasm";

    public InteractionResult DropSpecial(IItem item, IContext context)
    {
        context.Items.Remove(item);
        item.CurrentLocation = null;
        var message = $"The {item.NounsForMatching[0]} drops out of sight and into the chasm. ";
        return new PositiveInteractionResult(message);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.SW, new MovementParameters { Location = GetLocation<EastWestPassage>() } },
            { Direction.Up, new MovementParameters { Location = GetLocation<EastWestPassage>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<NorthSouthPassage>() } },
            { Direction.NE, new MovementParameters { Location = GetLocation<ReservoirSouth>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "A chasm runs southwest to northeast and the path follows it. You are on the south side of the " +
               "chasm, where a crack opens into a passage. ";
    }
}