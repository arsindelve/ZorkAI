using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Item.Kalamontee;

public class Padlock : ItemBase, ICanBeTakenAndDropped
{
    [UsedImplicitly] public bool Locked { get; set; } = true;

    [UsedImplicitly] public bool AttachedToDoor { get; set; } = true;

    public override string CannotBeTakenDescription => Locked ? "The padlock is locked to the door. " : "";

    public override string[] NounsForMatching => ["lock", "padlock"];

    public override string? OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        AttachedToDoor = false;
        return base.OnBeingTaken(context, previousLocation);
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a padlock here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A padlock";
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["remove", "take"], NounsForMatching))
        {
            if (Locked && AttachedToDoor)
                return new PositiveInteractionResult("The padlock is locked to the door. ");

            AttachedToDoor = false;
            context.ItemPlacedHere(this);
            return new PositiveInteractionResult("Taken. ");
        }

        if (action.Match(["unlock", "open"], NounsForMatching))
        {
            if (!Locked)
                return new PositiveInteractionResult("The padlock is already open. ");

            return new PositiveInteractionResult("You'll need to specify what you want to unlock it with. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        if (!context.HasItem<Key>())
            return new NoNounMatchInteractionResult();

        // Allow "unlock door with key" when padlock is attached to the door
        var nounsToMatch = AttachedToDoor
            ? [..NounsForMatching, "door"]
            : NounsForMatching;

        var match = action.Match<Key>(["unlock", "open"], nounsToMatch, ["with", "using"]);
        match |= action.Match<Padlock>(["use"], Repository.GetItem<Key>().NounsForMatching, ["on"]);

        if (match)
        {
            Locked = false;

            Repository.GetItem<Floyd>().CommentOnAction(FloydPrompts.PadlockUnlocked, context);

            return new PositiveInteractionResult("The padlock springs open. ");
        }

        return await base.RespondToMultiNounInteraction(action, context);
    }
}