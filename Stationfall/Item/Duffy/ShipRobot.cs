using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     Shared behavior for the three robots waiting in the Robot Pool's bins. Exactly one can be
///     requisitioned, via the selection keypad (verbs.zil ROBOT-TYPE): bin 1 is Rex, bin 2 is Helen,
///     bin 3 is Floyd. The chosen robot then trails the player around the ship.
/// </summary>
public abstract class ShipRobot : ContainerBase, ICanBeExamined, ITurnBasedActor
{
    /// <summary>
    ///     Which bin this robot occupies, and therefore the number that selects it on the keypad.
    /// </summary>
    public abstract int BinNumber { get; }

    /// <summary>
    ///     True once this robot has been requisitioned. Only one robot is ever selected.
    /// </summary>
    [UsedImplicitly]
    public bool IsSelected { get; set; }

    /// <summary>
    ///     Shown when the player looks in this robot's bin, before selection.
    /// </summary>
    public abstract string InTheBinDescription { get; }

    public abstract string ExaminationDescription { get; }

    protected override int SpaceForItems => 5;

    /// <summary>
    ///     The robots carry things in plain sight (the original's SEARCHBIT/OPENBIT, ship.zil:285).
    ///     This matters: without it, handing a robot the activation form would hide it from inventory,
    ///     examine and take, and quietly make the game unwinnable.
    /// </summary>
    public override bool IsTransparent => true;

    /// <summary>
    ///     A selected robot follows the player from room to room. Returns the line describing the move,
    ///     or empty when there's nothing to say.
    /// </summary>
    public virtual Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!IsSelected)
            return Task.FromResult(string.Empty);

        if (context.CurrentLocation == CurrentLocation)
            return Task.FromResult(string.Empty);

        return Task.FromResult(FollowThePlayer(context));
    }

    /// <summary>
    ///     Moves this robot into the player's room. Subclasses override to add their own arrival line —
    ///     or, in Rex's case, to flatten you on arrival.
    /// </summary>
    protected virtual string FollowThePlayer(IContext context)
    {
        MoveToPlayer(context);
        return $"{Name} follows you. ";
    }

    protected void MoveToPlayer(IContext context)
    {
        CurrentLocation?.RemoveItem(this);
        context.CurrentLocation.ItemPlacedHere(this);
    }

    /// <summary>
    ///     Display name — the robots go by their given names, with no article.
    /// </summary>
    public string Name => NounsForMatching[0][..1].ToUpperInvariant() + NounsForMatching[0][1..];

    public override string GenericDescription(ILocation? currentLocation)
    {
        return Name;
    }

    public override void Init()
    {
    }
}
