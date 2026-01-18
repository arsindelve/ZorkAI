using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace EscapeRoom.Location;

public class StorageCloset : DarkLocation
{
    public override string Name => "Storage Closet";

    public override string DarkDescription =>
        "It is pitch dark. You can't see a thing. You should find a light source before exploring further.";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "A small, cramped storage closet. Cardboard boxes are stacked against the walls. " +
               "The only exit is south to the reception area.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Reception>() } }
        };
    }

    public override void Init()
    {
        StartWithItem<CardboardBox>();
    }
}
