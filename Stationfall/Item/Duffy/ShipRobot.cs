using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     Shared behavior for the three robots waiting in the Robot Pool's bins. Exactly one can be
///     requisitioned, via the selection keypad (verbs.zil ROBOT-TYPE): bin 1 is Rex, bin 2 is Helen,
///     bin 3 is Floyd. The chosen robot then trails the player around the ship.
/// </summary>
public abstract class ShipRobot : ContainerBase, ICanBeExamined, ITurnBasedActor, ICanBeTalkedTo
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

    /// <summary>
    ///     True once the player has laid eyes on this robot properly — the original's TOUCHBIT. Only
    ///     the companion does anything with it, but it belongs here because it is set by simply being
    ///     in the room with them.
    /// </summary>
    [UsedImplicitly]
    public bool HasBeenSeen { get; set; }

    /// <summary>
    ///     Whether any of the three has been requisitioned yet.
    /// </summary>
    protected static bool AnyRobotPicked =>
        Repository.GetItem<Rex>().IsSelected || Repository.GetItem<Helen>().IsSelected ||
        Repository.GetItem<Floyd>().IsSelected;

    /// <summary>
    ///     A robot in its bin is behind the pool's barrier: you can look, and that is all. Once one has
    ///     been requisitioned the others stay out of reach for good.
    /// </summary>
    protected bool IsWithinReach => IsSelected;

    /// <summary>
    ///     What this robot says to a greeting. Each of the three has its own voice, and that is most of
    ///     what distinguishes them before you have to choose.
    /// </summary>
    protected abstract string GreetingResponse { get; }

    /// <summary>
    ///     What it says when asked to come along.
    /// </summary>
    protected abstract string FollowResponse { get; }

    /// <summary>
    ///     What it says to anything else. The catch-all is characterful rather than a shrug: it is the
    ///     clearest signal of what each robot is and isn't good for.
    /// </summary>
    protected abstract string CatchAllResponse { get; }

    /// <summary>
    ///     Addressing a robot by name. Recognized openings get the original's canned lines; everything
    ///     else falls to the robot's own catch-all rather than to the narrator, because these three are
    ///     characters with fixed registers, not improvisers.
    /// </summary>
    public Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client)
    {
        var said = text.ToLowerInvariant();

        if (said.Contains("hello") || said.Contains("hi ") || said.Trim() is "hi" or "hey" ||
            said.Contains("greetings"))
            return Task.FromResult(GreetingResponse);

        if (said.Contains("follow") || said.Contains("come with") || said.Contains("come along") ||
            said.Contains("walk"))
            return Task.FromResult(FollowResponse);

        return Task.FromResult(CatchAllResponse);
    }

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
    public override string Name => NounsForMatching[0][..1].ToUpperInvariant() + NounsForMatching[0][1..];

    public override string GenericDescription(ILocation? currentLocation)
    {
        return Name;
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // NB: no early return on a noun miss. ContainerBase offers the action to whatever this robot is
        // carrying before considering itself, and short-circuiting here would hide those items from
        // every verb - the same trap the Thermos fell into.
        if (action.MatchNounAndAdjective(NounsForMatching))
        {
            // You cannot requisition a robot by asking for it; that is what the equipment is for, and
            // saying so is what points the player at the keypad.
            if (action.MatchVerb(["pick", "choose", "select", "requisition", "hire"]))
                return new PositiveInteractionResult(PickResponse());

            // Only physical contact is blocked. The original gates on TOUCHING?, not on visibility -
            // and it matters: the passed-over robot's reaction is meant to be seen, so refusing to let
            // the player look at them would hide the one thing choosing has consequences for.
            var isPhysical = action.MatchVerb([
                "take", "get", "grab", "touch", "push", "pull", "move", "kick", "shake", "hit", "kiss",
                "attack", "kill", "turn on", "turn off", "open", "close", "hug", "tickle"
            ]);

            if (isPhysical && !IsWithinReach && AnyRobotPicked)
                return new PositiveInteractionResult($"You can't reach {Name} from here. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    private string PickResponse()
    {
        if (IsSelected)
            return $"You already picked {Name}. ";

        return AnyRobotPicked
            ? $"You've already made your choice, and it wasn't {Name}. "
            : "Use the automated robot selection equipment. ";
    }

    public override void Init()
    {
    }
}
