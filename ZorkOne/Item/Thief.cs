using System.Text;
using GameEngine.IntentEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.ActorInteraction;

namespace ZorkOne.Item;

public class Thief : ContainerBase, ICanBeExamined, ITurnBasedActor, ICanBeAttacked, ICanBeGivenThings
{
    private readonly GiveSomethingToSomeoneDecisionEngine<Thief> _giveHimSomethingEngine = new();
    internal ICombatEngine ThiefAttackedEngine { private get; set; } = new AdventurerVersusThiefCombatEngine();
    internal ThiefCombatEngine ThiefAttackingEngine { private get; set; } = new();

    public bool IsUnconscious { get; set; }

    public bool IsStunned { get; set; }

    public bool IsDisarmed { get; set; }

    public bool IsDead { get; set; }

    [UsedImplicitly] public int TurnsUnconscious { get; set; }

    [UsedImplicitly] public List<IItem> TreasureStash { get; set; } = new();

    public override string CannotBeTakenDescription => IsUnconscious
        ? ""
        : "Once you got him, what would you do with him? ";

    public override string[] NounsForMatching =>
    [
        "suspicious-looking individual", "individual", "thief", "man", "robber", "gentleman", "guy", "dude", "footpad",
        "crook", "criminal", "gent", "bandit"
    ];

    public override bool IsTransparent => true;

    public string ExaminationDescription => IsUnconscious
        ? "The thief is a suspicious-looking individual, lying unconscious on the ground. "
        : "The thief is a slippery character with beady eyes that flit back and " +
          "forth. He carries, along with an unmistakable arrogance, a large bag " +
          "over his shoulder and a vicious stiletto, whose blade is aimed menacingly " +
          "in your direction. I'd watch out if I were you. ";

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
                "The thief is taken aback by your unexpected generosity, but accepts the jewel-encrusted egg and stops to admire its beauty. ");
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
        if (IsUnconscious)
        {
            TurnsUnconscious++;

            if (TurnsUnconscious != 2)
                return Task.FromResult("");

            IsUnconscious = false;
            TurnsUnconscious = 0;

            return Task.FromResult(
                "The robber revives, briefly feigning continued unconsciousness, and, when he sees his moment, scrambles away from you. ");
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

        //return Task.FromResult(ThiefAttackingEngine.Attack(context));
        
        return Task.FromResult(string.Empty);
        
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
        IGenerationClient client)
    {
        var killInteraction =
            new KillSomeoneDecisionEngine<Thief>(ThiefAttackedEngine).DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(
                action, context);

        return killInteraction ?? base.RespondToSimpleInteraction(action, context, client);
    }

    public override void Init()
    {
        StartWithItemInside<Stiletto>();
    }
}