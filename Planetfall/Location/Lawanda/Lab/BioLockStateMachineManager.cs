using Planetfall.Command;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

public class BioLockStateMachineManager
{
    public enum FloydLabSequenceState
    {
        NotStarted,
        FloydRushedInNeedToCloseDoor,
        DoorClosedFloydFighting,
        NeedToReopenDoor,
        DoorReopenedNeedToCloseAgain,
        Completed
    }

    // State properties
    public bool FloydHasSaidLookAMiniCard { get; set; }
    public bool FloydHasSaidNeedToGetCard { get; set; }
    public int FloydWaitingForDoorOpenCount { get; set; }
    public FloydLabSequenceState LabSequenceState { get; set; } = FloydLabSequenceState.NotStarted;
    public int FloydFightingTurnCount { get; set; }
    public bool HasWaitedOneTurnInBioLockEast { get; set; }
    public int TurnsAfterFloydRushedIn { get; set; }
    public int TurnsAfterDoorReopened { get; set; }

    /// <summary>
    /// Returns true if Floyd is currently in the Bio Lab fighting (not in the player's room).
    /// </summary>
    public bool IsFloydInLabFighting =>
        LabSequenceState == FloydLabSequenceState.FloydRushedInNeedToCloseDoor ||
        LabSequenceState == FloydLabSequenceState.DoorClosedFloydFighting ||
        LabSequenceState == FloydLabSequenceState.NeedToReopenDoor ||
        LabSequenceState == FloydLabSequenceState.DoorReopenedNeedToCloseAgain;

    /// <summary>
    /// Handle turn-based actions for Floyd in BioLockEast
    /// </summary>
    public string HandleTurnAction(bool isFloydHereAndOn, bool computerRoomFloydHasExpressedConcern, IContext context, Floyd floyd)
    {
        // During the lab sequence, Floyd is not in the room (he's in the lab fighting)
        var isFloydInLab = LabSequenceState == FloydLabSequenceState.FloydRushedInNeedToCloseDoor ||
                           LabSequenceState == FloydLabSequenceState.DoorClosedFloydFighting ||
                           LabSequenceState == FloydLabSequenceState.NeedToReopenDoor ||
                           LabSequenceState == FloydLabSequenceState.DoorReopenedNeedToCloseAgain;

        if (!isFloydInLab && !isFloydHereAndOn)
            return string.Empty;

        // First turn in BioLockEast: Floyd doesn't say anything yet
        if (!HasWaitedOneTurnInBioLockEast)
        {
            HasWaitedOneTurnInBioLockEast = true;
            return string.Empty;
        }

        // Lab sequence: Floyd is fighting inside the lab
        if (LabSequenceState == FloydLabSequenceState.DoorClosedFloydFighting)
        {
            FloydFightingTurnCount++;

            if (FloydFightingTurnCount == 1)
            {
                return FloydConstants.InTheLabTwo;
            }

            if (FloydFightingTurnCount == 2)
            {
                LabSequenceState = FloydLabSequenceState.NeedToReopenDoor;
                return FloydConstants.InTheLabThree;
            }

            // Player didn't open door after InTheLabThree - Floyd dies
            return FloydConstants.FloydDies;
        }

        // Player failed to close door after Floyd rushed in
        if (LabSequenceState == FloydLabSequenceState.FloydRushedInNeedToCloseDoor)
        {
            TurnsAfterFloydRushedIn++;
            if (TurnsAfterFloydRushedIn > 1)
            {
                return new DeathProcessor().Process(FloydConstants.BiologicalNightmaresDeath, context).InteractionMessage;
            }
            return string.Empty; // Give player one turn to close the door
        }

        // Player failed to close door after reopening it
        if (LabSequenceState == FloydLabSequenceState.DoorReopenedNeedToCloseAgain)
        {
            TurnsAfterDoorReopened++;
            if (TurnsAfterDoorReopened > 1)
            {
                return new DeathProcessor().Process(FloydConstants.BiologicalNightmaresDeath, context).InteractionMessage;
            }
            return string.Empty; // Give player one turn to close the door
        }

        // Player failed to reopen door after InTheLabThree - Floyd dies
        if (LabSequenceState == FloydLabSequenceState.NeedToReopenDoor)
        {
            floyd.HasDied = true;
            return FloydConstants.FloydDies;
        }

        // State 3: Waiting for door to open (up to 5 turns) - check this first
        if (FloydHasSaidNeedToGetCard && FloydWaitingForDoorOpenCount < 5)
        {
            FloydWaitingForDoorOpenCount++;

            if (FloydWaitingForDoorOpenCount == 5)
            {
                return FloydConstants.Sulk;
            }

            return FloydConstants.OpenTheDoor;
        }

        // State 2: Floyd has expressed concern, start the "get card" sequence
        if (computerRoomFloydHasExpressedConcern && !FloydHasSaidNeedToGetCard)
        {
            FloydHasSaidNeedToGetCard = true;
            return FloydConstants.NeedToGetCard;
        }

        // State 1: Floyd hasn't expressed concern yet, just point out the card
        if (!computerRoomFloydHasExpressedConcern && !FloydHasSaidLookAMiniCard)
        {
            FloydHasSaidLookAMiniCard = true;
            return FloydConstants.LookAMiniCard;
        }

        return string.Empty;
    }

    /// <summary>
    /// Handle door opening logic. Called from OnOpening after door is opened.
    /// </summary>
    public string HandleDoorOpening(IContext context)
    {
        var bioLockEast = Repository.GetLocation<BioLockEast>();
        var floyd = Repository.GetItem<Floyd>();

        var isFloydReady = FloydHasSaidNeedToGetCard && floyd.IsHereAndIsOn(context);

        // If Floyd is ready to go get the card, start the sequence
        if (isFloydReady && LabSequenceState == FloydLabSequenceState.NotStarted)
        {
            LabSequenceState = FloydLabSequenceState.FloydRushedInNeedToCloseDoor;
            FloydWaitingForDoorOpenCount = 0; // Reset the sulking counter
            TurnsAfterFloydRushedIn = 0; // Reset the turn counter

            // Remove Floyd from the room - he's now in the lab fighting!
            bioLockEast.RemoveItem(floyd);

            return FloydConstants.InTheLabOne;
        }

        // Player needs to reopen the door to let Floyd back (after 3 knocks)
        if (LabSequenceState == FloydLabSequenceState.NeedToReopenDoor)
        {
            LabSequenceState = FloydLabSequenceState.DoorReopenedNeedToCloseAgain;
            TurnsAfterDoorReopened = 0; // Reset the turn counter
            return FloydConstants.FloydReturnsWithCard;
        }

        // Player opened door while Floyd is still fighting (before he knocked) - immediate death
        if (LabSequenceState == FloydLabSequenceState.DoorClosedFloydFighting)
        {
            return new DeathProcessor().Process(FloydConstants.BiologicalNightmaresDeath, context).InteractionMessage;
        }

        // Opening at wrong time (before Floyd has even rushed in) = immediate death
        var youDie =
            "Opening the door reveals a Bio-Lab full of horrible mutations. You stare at them, frozen with horror. " +
            "Growling with hunger and delight, the mutations march into the bio-lock and devour you.";

        return new DeathProcessor().Process(youDie, context).InteractionMessage;
    }

    /// <summary>
    /// Handle door closing logic. Returns custom message or empty string to use default.
    /// </summary>
    public string HandleDoorClosing(IContext context)
    {
        var floyd = Repository.GetItem<Floyd>();
        var bioLockEast = Repository.GetLocation<BioLockEast>();

        // Floyd just rushed in, player closes door correctly
        if (LabSequenceState == FloydLabSequenceState.FloydRushedInNeedToCloseDoor)
        {
            LabSequenceState = FloydLabSequenceState.DoorClosedFloydFighting;
            FloydFightingTurnCount = 0;
            return "The door closes.\n\nAnd not a moment too soon! You hear a pounding from the door as the monsters within vent their frustration at losing their prey.";
        }

        // Player reopened door, now closes it immediately (success!)
        if (LabSequenceState == FloydLabSequenceState.DoorReopenedNeedToCloseAgain)
        {
            LabSequenceState = FloydLabSequenceState.Completed;
            EndSequence(context, floyd, bioLockEast);

            return FloydConstants.AfterLab;
        }

        return string.Empty; // Use default door closing message
    }

    private static void EndSequence(IContext context, Floyd floyd, BioLockEast bioLockEast)
    {
        floyd.HasDied = true;

        // Unregister actors - the sequence is complete
        context.RemoveActor(floyd);
        context.RemoveActor(bioLockEast);

        // Drop the miniaturization access card in BioLockEast
        var miniCard = Repository.GetItem<MiniaturizationAccessCard>();
        bioLockEast.ItemPlacedHere(miniCard);

        context.AddPoints(2);
    }
}
