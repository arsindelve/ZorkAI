using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Item.Kalamontee;

public class Padlock : ItemBase, ICanBeTakenAndDropped
{
    // ReSharper disable once MemberCanBePrivate.Global
    public bool Locked { get; set; } = true;

    // ReSharper disable once MemberCanBePrivate.Global
    public bool AttachedToDoor { get; set; } = true;

    public override string CannotBeTakenDescription => Locked ? "The padlock is locked to the door. " : "";

    public override string[] NounsForMatching => ["lock", "padlock"];

    public override string? OnBeingTaken(IContext context)
    {
        AttachedToDoor = false;
        return base.OnBeingTaken(context);
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a padlock here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A padlock";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client)
    {
        if (action.Match(["remove"], NounsForMatching))
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
        
        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (!context.HasItem<Key>())
            return new NoNounMatchInteractionResult();

        var match = action.Match<Key>(["unlock", "open"], NounsForMatching, ["with", "using"]);
        match |= action.Match<Padlock>(["use"], Repository.GetItem<Key>().NounsForMatching, ["on"]);

        if (match)
        {
            Locked = false;
            return new PositiveInteractionResult("The padlock springs open. ");
        }

        return base.RespondToMultiNounInteraction(action, context);
    }
}