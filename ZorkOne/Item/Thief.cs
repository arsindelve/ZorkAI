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
    internal ICombatEngine ThiefAttackEngine { private get; set; } = new AdventurerVersusThiefCombatEngine();

    public bool IsUnconscious { get; set; }

    public bool IsStunned { get; set; }

    public bool IsDead { get; set; }

    [UsedImplicitly] public List<IItem> TreasureStash { get; set; } = new();

    public override string CannotBeTakenDescription => IsUnconscious
        ? ""
        : "Once you got him, what would you do with him? ";

    /*

    No effect:
      The thief stabs nonchalantly with his stiletto and misses.
      You dodge as the thief comes in low.
      You parry a lightning thrust, and the thief salutes you with a grim nod.
      The thief tries to sneak past your guard, but you twist away.
      The thief stabs nonchalantly with his stiletto and misses.

    Small Wound
      The thief slowly approaches, strikes like a snake, and leaves you wounded.
      The stiletto flashes faster than you can follow, and blood wells from your leg.
      A quick thrust pinks your left arm, and blood starts to trickle down.
      The thief draws blood, raking his stiletto across your arm.
      The thief strikes at your wrist, and suddenly your grip is slippery with blood.

    Knockout
      The thief knocks you out.
      Shifting in the midst of a thrust, the thief knocks you unconscious with the haft of his stiletto.

    Stun You:
      The thief attacks, and you fall back desperately.
      The thief rams the haft of his blade into your stomach, leaving you out of breath.
      The butt of his stiletto cracks you on the skull, and you stagger back.


    Kill You:

      Finishing you off, the thief inserts his blade into your heart.
      The thief neatly flips your nasty knife out of your hands, and it drops to the floor.
      The stiletto severs your jugular.  It looks like the end.
      The thief bows formally, raises his stiletto, and with a wry grin, ends the battle and your life.
      The thief comes in from the side, feints, and inserts the blade into your ribs.

      After Knockout:
        The thief, forgetting his essentially genteel upbringing, cuts your throat.
        The thief, a pragmatist, dispatches you as a threat to his livelihood.
        The thief amuses himself by searching your pockets.


 ___________________________________________________________________________________________________________-

    
      The robber revives, briefly feigning continued unconsciousness, and, when he sees his moment, scrambles away from you.


        Almost as soon as the thief breathes his last breath, a cloud of sinister black fog envelops him, and when the fog lifts, the carcass has disappeared.
        As the thief dies, the power of his magic decreases, and his treasures reappear:
          A stiletto
          A crystal skull
          A trunk of jewels
          A torch
          A sapphire-encrusted bracelet


     */

    public override string[] NounsForMatching =>
    [
        "suspicious-looking individual", "thief", "man", "robber", "gentleman", "guy", "dude", "footpad", "crook",
        "criminal", "gent", "bandit"
    ];

    public override bool IsTransparent => true;

    public string ExaminationDescription => "The thief is a slippery character with beady eyes that flit back and " +
                                            "forth. He carries, along with an unmistakable arrogance, a large bag " +
                                            "over his shoulder and a vicious stiletto, whose blade is aimed menacingly " +
                                            "in your direction. I'd watch out if I were you. ";

    public InteractionResult OfferThisThing(IItem item, IContext context)
    {
        var sb = new StringBuilder();

        if (IsUnconscious)
            sb.AppendLine("Your proposed victim suddenly recovers consciousness. ");

        if (item is Egg egg)
        {
            egg.IsOpen = true;
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
        throw new NotImplementedException();
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
        
        result = new KillSomeoneDecisionEngine<Thief>(ThiefAttackEngine).DoYouWantToKillSomeone(action, context);
        return result ?? base.RespondToMultiNounInteraction(action, context);
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        var killInteraction = new KillSomeoneDecisionEngine<Thief>(ThiefAttackEngine).DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(action, context);
        return killInteraction ?? base.RespondToSimpleInteraction(action, context, client);
    }

    public override void Init()
    {
        StartWithItemInside<Stiletto>();
    }
}