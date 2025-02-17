using GameEngine;
using GameEngine.Item;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Painting : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["painting"];

    public override int Size => 4;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A painting by a neglected artist is here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return
            "Fortunately, there is still one chance for you to be a vandal, for on the far wall is a painting of unparalleled beauty. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 4;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 6;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A painting";
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        if (action.Match(["cut", "chop", "slash", "destroy"], NounsForMatching,
                Repository.GetItem<Sword>().NounsForMatching, ["with", "using"]))
        {
            if (!context.HasItem<Sword>())
            {
                // It's here, but not in your inventory, so you can't use it.
                if (context.CurrentLocation.HasItem<Sword>())
                    return new PositiveInteractionResult("You need to pick up the sword first. ");

                return new NoNounMatchInteractionResult();
            }

            Repository.DestroyItem(this);
            return new PositiveInteractionResult(
                "Your skillful swordsmanship slices the painting into innumerable slivers which blow away. ");
        }

        return await base.RespondToMultiNounInteraction(action, context);
    }
}