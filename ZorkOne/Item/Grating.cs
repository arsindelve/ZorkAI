using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Item;

public class Grating : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForMatching => ["grating", "grate"];

    public bool IsLocked { get; set; } = true;

    public string ExaminationDescription => $"The grating is {(IsOpen ? "open" : "closed")}. ";

    public bool IsOpen { get; set; }

    public string NowOpen(ILocation currentLocation)
    {
        return currentLocation switch
        {
            GratingRoom => "The grating opens to reveal trees above you. ",
            // Note: Try this in the real game. It's a bug! 
            Clearing => "The grating opens to reveal darkness below. ",
            _ => ""
        };
    }

    public string NowClosed(ILocation currentLocation)
    {
        return "The grating is now closed";
    }

    public string AlreadyOpen => "Too late. ";

    public string AlreadyClosed => "Too late. ";

    public bool HasEverBeenOpened { get; set; }

    public string? CannotBeOpenedDescription(IContext context)
    {
        return IsLocked ? "The grating is locked. " : null;
    }

    public override string OnOpening(IContext context)
    {
        var leaves = Repository.GetItem<PileOfLeaves>();

        if (leaves.HasEverBeenPickedUp)
            return string.Empty;

        leaves.HasEverBeenPickedUp = true;
       
        Repository.GetLocation<Clearing>().Items.Add(this);
        Repository.GetLocation<GratingRoom>().ItemPlacedHere(leaves);
        return "\nA pile of leaves falls onto your head and to the ground. ";
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        if (action.Match<SkeletonKey>(["unlock"], NounsForMatching,
                ["with", "using"]))
            if (context.CurrentLocation is GratingRoom)
            {
                var message = "The grate is unlocked. ";
                Repository.GetItem<Grating>().IsLocked = false;
                return new PositiveInteractionResult(message);
            }
            else
            {
                return new PositiveInteractionResult("You can't reach the lock from here. ");
            }

        if (action.Match<SkeletonKey>(["lock"], NounsForMatching, ["with", "using"]))
            if (context.CurrentLocation is GratingRoom)
            {
                var message = "The grate is locked. ";
                Repository.GetItem<Grating>().IsLocked = true;
                return new PositiveInteractionResult(message);
            }
            else
            {
                return new PositiveInteractionResult("You can't lock it from this side. ");
            }

        return await base.RespondToMultiNounInteraction(action, context);
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["lock"], NounsForMatching))
            if (context.CurrentLocation is Clearing)
            {
                if (context.HasItem<SkeletonKey>())
                    return new PositiveInteractionResult("You can't lock it from this side. ");
            }
            else
            {
                Repository.GetItem<Grating>().IsLocked = true;
                return new PositiveInteractionResult("The grate is locked");
            }

        if (action.Match(["unlock"], NounsForMatching))
            if (context.HasItem<SkeletonKey>())
            {
                var message = "(with the skeleton key)";

                if (context.CurrentLocation is GratingRoom)
                {
                    message += "\n\nThe grate is unlocked. ";
                    Repository.GetItem<Grating>().IsLocked = false;
                }
                else
                {
                    message += "\n\nYou can't reach the lock from here. ";
                }

                return new PositiveInteractionResult(message);
            }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override string NeverPickedUpDescription(ILocation? currentLocation)
    {
        return currentLocation switch
        {
            Clearing => IsOpen
                ? "There is an open grating, descending into darkness."
                : "There is a grating securely fastened into the ground. ",
            GratingRoom => IsOpen
                ? "Above you is an open grating with sunlight pouring in. "
                : IsLocked
                    ? "Above you is a grating locked with a skull-and-crossbones lock. "
                    : "Above you is a grating. ",
            _ => throw new NotSupportedException()
        };
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return NeverPickedUpDescription(currentLocation);
    }
}