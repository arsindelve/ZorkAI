using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace EscapeRoom.Item;

public class ExitDoor : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForMatching => ["door", "exit door", "metal door", "exit", "lock"];

    public bool IsLocked { get; set; } = true;

    public bool IsOpen { get; set; }

    private bool _hasAwardedUnlockPoints;

    public string ExaminationDescription =>
        IsLocked
            ? "A heavy metal door with a prominent lock. The word 'EXIT' is painted above it. "
            : IsOpen
                ? "The exit door stands open, revealing freedom beyond! "
                : "A heavy metal door. It is unlocked. ";

    public string NowOpen(ILocation currentLocation)
    {
        return """
               The heavy door swings open, revealing bright sunlight beyond!

               *** CONGRATULATIONS! ***

               You have successfully completed the Escape Room Tutorial!
               You've learned the basic commands needed for interactive fiction:
               - LOOK and EXAMINE to observe your surroundings
               - TAKE and DROP to manage inventory
               - OPEN containers to find items
               - UNLOCK doors with keys
               - Navigate with N, S, E, W

               You are now ready for real adventures!
               """;
    }

    public override string OnOpening(IContext context)
    {
        // Trigger victory!
        if (context is EscapeRoomContext escapeContext)
        {
            escapeContext.HasEscaped = true;
            escapeContext.AddPoints(50);
        }

        return string.Empty;
    }

    private void AwardUnlockPoints(IContext context)
    {
        if (_hasAwardedUnlockPoints) return;
        _hasAwardedUnlockPoints = true;
        context.AddPoints(10);
    }

    public string NowClosed(ILocation currentLocation)
    {
        return "You close the exit door. ";
    }

    public string AlreadyOpen => "The door is already open. ";

    public string AlreadyClosed => "The door is already closed. ";

    public bool HasEverBeenOpened { get; set; }

    public string? CannotBeOpenedDescription(IContext context)
    {
        return IsLocked ? "The door is locked. " : null;
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        // Handle "unlock door with key"
        if (action.Match<BrassKey>(["unlock"], NounsForMatching, ["with", "using"]))
        {
            if (!context.HasItem<BrassKey>())
                return new PositiveInteractionResult("You don't have the key. ");

            IsLocked = false;
            AwardUnlockPoints(context);
            return new PositiveInteractionResult("The door is now unlocked. ");
        }

        // Handle "lock door with key"
        if (action.Match<BrassKey>(["lock"], NounsForMatching, ["with", "using"]))
        {
            if (!context.HasItem<BrassKey>())
                return new PositiveInteractionResult("You don't have the key. ");

            IsLocked = true;
            return new PositiveInteractionResult("The door is now locked. ");
        }

        return await base.RespondToMultiNounInteraction(action, context);
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // Handle "unlock door" when player has key
        if (action.Match(["unlock"], NounsForMatching))
        {
            if (context.HasItem<BrassKey>())
            {
                IsLocked = false;
                AwardUnlockPoints(context);
                return new PositiveInteractionResult("(with the brass key)\n\nThe door is now unlocked. ");
            }

            return new PositiveInteractionResult("You don't have the key to unlock this door. ");
        }

        // Handle "lock door" when player has key
        if (action.Match(["lock"], NounsForMatching))
        {
            if (context.HasItem<BrassKey>())
            {
                IsLocked = true;
                return new PositiveInteractionResult("(with the brass key)\n\nThe door is now locked. ");
            }

            return new PositiveInteractionResult("You don't have the key to lock this door. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override string NeverPickedUpDescription(ILocation? currentLocation)
    {
        return IsOpen
            ? "The exit door stands open. "
            : IsLocked
                ? "There is a locked metal door here. "
                : "There is a metal door here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return NeverPickedUpDescription(currentLocation);
    }
}
