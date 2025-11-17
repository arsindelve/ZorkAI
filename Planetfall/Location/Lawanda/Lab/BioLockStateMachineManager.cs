using Planetfall.Command;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Location.Lawanda.Lab;

public class BioLockStateMachineManager
{
    public enum FloydLabSequenceState
    {
        NotStarted,
        FloydRushedIn_NeedToCloseDoor,
        DoorClosed_FloydFighting,
        NeedToReopenDoor,
        DoorReopened_NeedToCloseAgain,
        Completed
    }

    // State properties
    public bool FloydHasSaidLookAMiniCard { get; set; }
    public bool FloydHasSaidNeedToGetCard { get; set; }
    public int FloydWaitingForDoorOpenCount { get; set; }
    public FloydLabSequenceState LabSequenceState { get; set; } = FloydLabSequenceState.NotStarted;
    public int FloydFightingTurnCount { get; set; }

    /// <summary>
    /// Returns true if Floyd is currently in the Bio Lab fighting (not in the player's room).
    /// </summary>
    public bool IsFloydInLabFighting =>
        LabSequenceState == FloydLabSequenceState.FloydRushedIn_NeedToCloseDoor ||
        LabSequenceState == FloydLabSequenceState.DoorClosed_FloydFighting ||
        LabSequenceState == FloydLabSequenceState.NeedToReopenDoor ||
        LabSequenceState == FloydLabSequenceState.DoorReopened_NeedToCloseAgain;

    /// <summary>
    /// Handle turn-based actions for Floyd in BioLockEast
    /// </summary>
    public string HandleTurnAction(bool isFloydHereAndOn, bool computerRoomFloydHasExpressedConcern)
    {
        // During the lab sequence, Floyd is not in the room (he's in the lab fighting)
        var isFloydInLab = LabSequenceState == FloydLabSequenceState.FloydRushedIn_NeedToCloseDoor ||
                           LabSequenceState == FloydLabSequenceState.DoorClosed_FloydFighting ||
                           LabSequenceState == FloydLabSequenceState.NeedToReopenDoor ||
                           LabSequenceState == FloydLabSequenceState.DoorReopened_NeedToCloseAgain;

        if (!isFloydInLab && !isFloydHereAndOn)
            return string.Empty;

        // Lab sequence: Floyd is fighting inside the lab
        if (LabSequenceState == FloydLabSequenceState.DoorClosed_FloydFighting)
        {
            FloydFightingTurnCount++;

            if (FloydFightingTurnCount == 1)
            {
                return FloydConstants.InTheLabTwo;
            }
            else if (FloydFightingTurnCount == 2)
            {
                return FloydConstants.InTheLabThree;
            }
            else if (FloydFightingTurnCount == 3)
            {
                LabSequenceState = FloydLabSequenceState.NeedToReopenDoor;
                return FloydConstants.InTheLabFour;
            }

            // Player didn't open door after InTheLabFour - Floyd dies
            return "TODO: Floyd dies because you didn't open the door in time.";
        }

        // Player failed to close door after Floyd rushed in
        if (LabSequenceState == FloydLabSequenceState.FloydRushedIn_NeedToCloseDoor)
        {
            return "TODO: You die because you didn't close the door in time.";
        }

        // Player failed to close door after reopening it
        if (LabSequenceState == FloydLabSequenceState.DoorReopened_NeedToCloseAgain)
        {
            return "TODO: You die because you didn't close the door after reopening it.";
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
    public string HandleDoorOpening(bool isFloydReady, IContext context, ICanContainItems bioLockEast)
    {
        // If Floyd is ready to go get the card, start the sequence
        if (isFloydReady && LabSequenceState == FloydLabSequenceState.NotStarted)
        {
            LabSequenceState = FloydLabSequenceState.FloydRushedIn_NeedToCloseDoor;
            FloydWaitingForDoorOpenCount = 0; // Reset the sulking counter

            // Remove Floyd from the room - he's now in the lab fighting!
            var floyd = Repository.GetItem<Floyd>();
            bioLockEast.RemoveItem(floyd);

            return FloydConstants.InTheLabOne;
        }

        // Player needs to reopen the door to let Floyd back
        if (LabSequenceState == FloydLabSequenceState.NeedToReopenDoor)
        {
            LabSequenceState = FloydLabSequenceState.DoorReopened_NeedToCloseAgain;
            return string.Empty; // No message, just open it
        }

        // Opening at wrong time = death
        var youDie =
            "Opening the door reveals a Bio-Lab full of horrible mutations. You stare at them, frozen with horror. " +
            "Growling with hunger and delight, the mutations march into the bio-lock and devour you.";

        return new DeathProcessor().Process(youDie, context).InteractionMessage;
    }

    /// <summary>
    /// Handle door closing logic. Returns custom message or empty string to use default.
    /// </summary>
    public string HandleDoorClosing()
    {
        // Floyd just rushed in, player closes door correctly
        if (LabSequenceState == FloydLabSequenceState.FloydRushedIn_NeedToCloseDoor)
        {
            LabSequenceState = FloydLabSequenceState.DoorClosed_FloydFighting;
            FloydFightingTurnCount = 0;
            return "The door closes.\n\nAnd not a moment too soon! You hear a pounding from the door as the monsters within vent their frustration at losing their prey.";
        }

        // Player reopened door, now closes it immediately (success!)
        if (LabSequenceState == FloydLabSequenceState.DoorReopened_NeedToCloseAgain)
        {
            LabSequenceState = FloydLabSequenceState.Completed;
            return FloydConstants.AfterLab;
        }

        return string.Empty; // Use default door closing message
    }
}
