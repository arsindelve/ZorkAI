using System.Text;
using GameEngine;
using GameEngine.IntentEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.ActorInteraction;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Item;

public class Thief : ContainerBase, ICanBeExamined, ITurnBasedActor, ICanBeAttacked, ICanBeGivenThings
{
    private readonly GiveSomethingToSomeoneDecisionEngine<Thief> _giveHimSomethingEngine = new();

    internal ICombatEngine ThiefAttackedEngine { private get; set; } = new AdventurerVersusThiefCombatEngine(new RandomChooser());

    internal ThiefCombatEngine ThiefAttackingEngine { private get; set; } = new();

    internal ThiefRobsYouEngine ThiefRobbingEngine { private get; set; } = new(new RandomChooser());

    /// <summary>
    /// Gets or sets a value indicating whether the thief is unconscious.
    /// </summary>
    /// <remarks>
    /// When the thief is unconscious, certain interactions, such as taking items from the thief,
    /// become possible, and the thief is unable to defend themselves in combat. This property
    /// is typically modified as a result of specific combat outcomes or other game events.
    /// </remarks>
    public bool IsUnconscious { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the thief is stunned.
    /// </summary>
    /// <remarks>
    /// When the thief is stunned, their ability to act, such as attacking or defending themselves, is temporarily impaired.
    /// This condition is usually caused by specific combat outcomes or unexpected actions from opponents. The stunned state typically wears off after a short period or specific events.
    /// </remarks>
    public bool IsStunned { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the thief is disarmed.
    /// </summary>
    /// <remarks>
    /// A disarmed thief temporarily loses the ability to use their weapon, rendering them less dangerous in combat.
    /// This state may be caused by a successful disarm attack during combat or other specific game events.
    /// Once disarmed, the thief may attempt to recover their weapon or otherwise adapt to their disadvantage.
    /// </remarks>
    public bool IsDisarmed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the thief is dead.
    /// </summary>
    /// <remarks>
    /// When the thief is dead, he no longer engage in combat, can no longer defend his location,
    /// and their belongings may become accessible to the player. This property is typically set as a
    /// result of combat or other fatal interactions with the thief.
    /// </remarks>
    public bool IsDead { get; set; }

    /// <summary>
    /// Gets or sets the number of turns the thief remains unconscious.
    /// </summary>
    /// <remarks>
    /// This property tracks the duration, in turns, that the thief stays unconscious.
    /// After reaching a certain threshold, the thief regains consciousness automatically,
    /// potentially resuming their actions and interactions. This is typically updated
    /// during turn-based gameplay mechanics.
    /// </remarks>
    [UsedImplicitly]
    public int TurnsUnconscious { get; set; }

    /// <summary>
    /// Gets or sets the collection of treasures the thief has accumulated.
    /// </summary>
    /// <remarks>
    /// The TreasureStash represents items the thief has stolen or gathered.
    /// This collection may grow as the thief successfully takes items from others or when specific interactions occur.
    /// Items in this stash may become accessible under certain conditions, such as when the thief is incapacitated or defeated.
    /// </remarks>
    [UsedImplicitly]
    public List<IItem> TreasureStash { get; set; } = new();

    /// <summary>
    /// Gets a description indicating why the thief cannot be taken.
    /// </summary>
    /// <remarks>
    /// This property provides contextual feedback to the player when attempting to take the thief.
    /// The description varies depending on the thief's state. If the thief is unconscious, no
    /// description is provided, indicating that it is possible to interact with the thief.
    /// Otherwise, a message explains the impracticality of taking the thief.
    /// </remarks>
    public override string CannotBeTakenDescription => IsUnconscious
        ? ""
        : "Once you got him, what would you do with him? ";

    public override string[] NounsForMatching =>
    [
        "suspicious-looking individual", "individual", "thief", "man", "robber", "gentleman", "guy", "dude", "footpad",
        "crook", "criminal", "gent", "bandit"
    ];

    public override bool IsTransparent => true;

    /// <summary>
    /// Gets a description of the thief when examined by a player.
    /// </summary>
    /// <remarks>
    /// The description varies depending on the thief's current state. When unconscious, the thief appears
    /// as a suspicious individual lying on the ground. Otherwise, the thief is depicted as a dangerous character
    /// with a menacing demeanor, carrying a large bag and wielding a stiletto.
    /// </remarks>
    public string ExaminationDescription => IsUnconscious
        ? "The thief is a suspicious-looking individual, lying unconscious on the ground. "
        : "The thief is a slippery character with beady eyes that flit back and " +
          "forth. He carries, along with an unmistakable arrogance, a large bag " +
          "over his shoulder and a vicious stiletto, whose blade is aimed menacingly " +
          "in your direction. I'd watch out if I were you. ";

    /// <summary>
    /// Offers an item to the thief. If the thief is unconscious, he regains consciousness.
    /// Special behavior occurs when giving a specific item (like an egg), otherwise the item is added to the thief's stash.
    /// </summary>
    /// <param name="item">The item being offered to the thief.</param>
    /// <param name="context">The context in which the interaction occurs.</param>
    /// <returns>An <see cref="InteractionResult"/> describing the result of the interaction.</returns>
    public InteractionResult OfferThisThing(IItem item, IContext context)
    {
        var sb = new StringBuilder();

        if (IsUnconscious)
        {
            IsUnconscious = false;
            sb.AppendLine("Your proposed victim suddenly recovers consciousness. ");
        }

        if (item is Egg egg)
        {
            // He opens it for you! 
            egg.IsOpen = true;
            // Giving him the egg prevents him from trying to kill you for a turn. 
            IsStunned = true;
            sb.AppendLine(
                "The thief is taken aback by your unexpected generosity, but accepts the jewel-encrusted egg " +
                "and stops to admire its beauty. ");
        }
        else
        {
            sb.AppendLine($"The thief places the {item.NounsForMatching.OrderByDescending(s => s.Length).First()} " +
                          $"in his bag and thanks you politely. ");
        }

        TreasureStash.Add(item);
        item.CurrentLocation?.RemoveItem(item);
        item.CurrentLocation = null;

        return new PositiveInteractionResult(sb.ToString());
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (IsDead)
        {
            context.RemoveActor(this);
            return Task.FromResult("");
        }

        if (context.CurrentLocation is TreasureRoom)
            return DefendingHisTreasure(context);

        return ProwlingTheDungeon(context);
    }

    private Task<string> ProwlingTheDungeon(IContext context)
    {
        // Not a location where he wanders. 
        if (context.CurrentLocation is not IThiefMayVisit)
            return Task.FromResult("");

        return ThiefRobbingEngine.StealSomething(context);
    }

    private Task<string> DefendingHisTreasure(IContext context)
    {
        if (IsUnconscious)
        {
            TurnsUnconscious++;

            if (TurnsUnconscious != 2)
                return Task.FromResult("");

            IsUnconscious = false;
            TurnsUnconscious = 0;

            return Task.FromResult(
                "The robber revives, briefly feigning continued unconsciousness, and, when he sees his moment, " +
                "scrambles away from you. ");
        }

        if (IsDisarmed)
        {
            IsDisarmed = false;
            return Task.FromResult(
                "The robber, somewhat surprised at this turn of events, nimbly retrieves his stiletto. ");
        }

        if (IsStunned)
        {
            IsStunned = false;
            return Task.FromResult("The thief slowly regains his feet. ");
        }

        return Task.FromResult(ThiefAttackingEngine.Attack(context));
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return IsUnconscious
            ? "There is a suspicious-looking individual lying unconscious on the ground. "
            : "There is a suspicious-looking individual, holding a large bag, leaning against one wall. He is armed with a deadly stiletto. ";
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        var result = _giveHimSomethingEngine.AreWeGivingSomethingToSomeone(action, this, context);

        if (result is not null)
            return result;

        result = new KillSomeoneDecisionEngine<Thief>(ThiefAttackedEngine).DoYouWantToKillSomeone(action, context);
        return result ?? base.RespondToMultiNounInteraction(action, context);
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        var killInteraction =
            new KillSomeoneDecisionEngine<Thief>(ThiefAttackedEngine).DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(
                action, context);

        return killInteraction ?? base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override void Init()
    {
        StartWithItemInside<Stiletto>();
    }
}