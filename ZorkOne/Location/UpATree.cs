using GameEngine;
using GameEngine.Location;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Location;

public class UpATree : BaseLocation, IDropSpecialLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Down, new MovementParameters { Location = GetLocation<ForestPath>() } },
            {
                Direction.Up,
                new MovementParameters { CanGo = _ => false, CustomFailureMessage = "You cannot climb any higher. " }
            }
        };

    public override string Name => "Up A Tree";

    protected override string ContextBasedDescription =>
        "You are about 10 feet above the ground nestled among some large branches. The nearest branch above you is above your reach. ";

    public InteractionResult DropSpecial(IItem item, IContext context)
    {
        var egg = Repository.GetItem<Egg>();
        Repository.GetLocation<ForestPath>().ItemPlacedHere(item);

        if (item is Egg)
        {
            egg.IsDestroyed = true;
            egg.IsOpen = true;

            Repository.GetItem<Canary>().IsDestroyed = true;

            return new PositiveInteractionResult(
                "The egg falls to the ground and springs open, seriously damaged. There is a golden " +
                "clockwork canary nestled in the egg. " + Canary.DestroyedMessage);
        }

        if (item is Nest && egg.CurrentLocation is Nest)
        {
            egg.IsDestroyed = true;
            egg.IsOpen = true;
            Repository.GetLocation<ForestPath>().ItemPlacedHere(egg);
            Repository.GetItem<Canary>().IsDestroyed = true;

            return new PositiveInteractionResult(
                "The nest falls to the ground, and the egg spills out of it, seriously damaged. ");
        }

        return new PositiveInteractionResult($"The {item.NounsForMatching[0]} falls to the ground. ");
    }

    public override InteractionResult RespondToSpecificLocationInteraction(string? input, IContext context)
    {
        switch (input?.ToLowerInvariant().Trim())
        {
            case "jump":
            case "leap":
            case "jump down":
            case "jump out of tree":
            case "jump out of the tree":
            case "jump down from the tree":

                context.CurrentLocation = Repository.GetLocation<ForestPath>();
                var message =
                    "In a feat of unaccustomed daring, you manage to land on your feet without killing yourself.\n\n";
                message += Repository.GetLocation<ForestPath>().Description;
                return new PositiveInteractionResult(message);
        }

        return base.RespondToSpecificLocationInteraction(input, context);
    }

    public override void Init()
    {
        StartWithItem<Nest>();
    }
}