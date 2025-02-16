using GameEngine.Location;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using ZorkOne.Command;

namespace ZorkOne.Location;

public class SandyCave : DarkLocationWithNoStartingItems
{
    [UsedImplicitly] public int DigCount { get; set; }

    public override string Name => "Sandy Cave";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.SW, Go<SandyBeach>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a sand-filled cave whose exit is to the southwest. ";
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        if (action.Match(
                ["dig"],
                ["ground", "sand", "dirt"],
                ["shovel"],
                ["with"]))
            switch (++DigCount)
            {
                case 1:
                    return new PositiveInteractionResult("You seem to be digging a hole here. ");
                case 2:
                    return new PositiveInteractionResult("The hole is getting deeper, but that's about it. ");
                case 3:
                    return new PositiveInteractionResult("You are surrounded by a wall of sand on all sides. ");
                case 4:
                {
                    ItemPlacedHere(GetItem<Scarab>());
                    return new PositiveInteractionResult("You can see a scarab here in the sand. ");
                }
                default:
                    return new DeathProcessor().Process("The hole collapses, smothering you. ", context);
            }

        return await base.RespondToMultiNounInteraction(action, context);
    }
}