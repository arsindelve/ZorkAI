using GameEngine.Location;

namespace Planetfall.Location;

public abstract class FloydSpecialInteractionLocation : LocationBase, IFloydSpecialInteractionLocation
{
    [UsedImplicitly] public bool InteractionHasHappened { get; set; }

    public abstract string FloydPrompt { get; }

    // Special location comments are handled by Floyd when he follows the player
    // into the location - see FloydMovementManager.HandleFollowingPlayer
}