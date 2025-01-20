using System.Text;
using GameEngine;
using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.ActorInteraction;

internal class AdventurerVersusThiefCombatEngine : ICombatEngine
{
    private readonly IRandomChooser _chooser;
    private readonly List<(CombatOutcome outcome, string text)> _notStunnedOutcomes;
    private Thief? _thief;

    public AdventurerVersusThiefCombatEngine(IRandomChooser chooser) : this()
    {
        _chooser = chooser;
    }

    public AdventurerVersusThiefCombatEngine()
    {
        _chooser = new RandomChooser();

        _notStunnedOutcomes =
        [
            (CombatOutcome.Miss, "Clang! Crash! The thief parries. "),
            (CombatOutcome.Miss, "The thief is struck on the arm; blood begins to trickle down. "),
            (CombatOutcome.Miss, "A good stroke, but it's too slow; the thief dodges. "),
            (CombatOutcome.Miss, "You charge, but the thief jumps nimbly aside. "),
            (CombatOutcome.Miss, "A good slash, but it misses the thief by a mile. "),
            (CombatOutcome.Miss, "A quick stroke, but the thief is on guard. "),
            (CombatOutcome.Miss, "Slash! Your stroke connects! This could be serious! "),
            (CombatOutcome.Miss, "You parry a lightning thrust, and the thief salutes you with a grim nod. "),
            (CombatOutcome.Miss, "Your {weapon} misses the thief by an inch. "),
            (CombatOutcome.Miss,
                "The thief's weapon is knocked to the floor, leaving him unarmed. The robber, somewhat surprised at this turn of events, nimbly retrieves his stiletto. "),

            (CombatOutcome.Stun, "The thief is momentarily disoriented and can't fight back."),
            (CombatOutcome.Stun, "The force of your blow knocks the thief back, stunned. "),
            (CombatOutcome.Stun, "The thief is confused and can't fight back. The thief slowly regains his feet. "),
            (CombatOutcome.Stun, "The thief is staggered, and drops to his knees. The thief slowly regains his feet. "),

            (CombatOutcome.Fatal, "It's curtains for the thief as your {weapon} removes his head. "),

            (CombatOutcome.Knockout, "The thief drops to the floor, unconscious. "),
            (CombatOutcome.Knockout, "The thief is knocked out! "),
            (CombatOutcome.Knockout, "Your {weapon} crashes down, knocking the thief into dreamland. "),
            (CombatOutcome.Knockout, "A furious exchange, and the thief is knocked out! "),
            (CombatOutcome.Knockout, "The thief is battered into unconsciousness. "),
            (CombatOutcome.Knockout, "The haft of your {weapon} knocks out the thief. "),

            (CombatOutcome.DropWeapon, " You parry a low thrust, and your sword slips out of your hand. ")
        ];
    }

    public InteractionResult? Attack(IContext context, IWeapon? weapon)
    {
        // You can't bare-knuckle with the troll. No weapon, no fight. 
        if (weapon is null)
            return null;

        // Don't assign these in the constructor or as initializers. You'll
        // get stack overflow errors. 
        _thief = Repository.GetItem<Thief>();

        if (context is ZorkIContext { IsStunned: true } zorkContext)
        {
            zorkContext.IsStunned = false;
            return new PositiveInteractionResult(
                "You are still recovering from that last blow, so your attack is ineffective. ");
        }

        if (_thief.IsUnconscious)
            return DeathBlow(context, "The unconscious thief cannot defend himself: He dies. ");

        var attack = _chooser.Choose(_notStunnedOutcomes);
        attack.text = attack.text.Replace("{weapon}",
            ((ItemBase?)weapon)?.NounsForMatching.FirstOrDefault() ?? " weapon ");


        switch (attack.outcome)
        {
            case CombatOutcome.Miss:
                return new PositiveInteractionResult(attack.text);

            case CombatOutcome.Stun:
                _thief.IsStunned = true;
                return new PositiveInteractionResult(attack.text);

            case CombatOutcome.Fatal:
                return DeathBlow(context, attack.text);

            case CombatOutcome.Knockout:
                return Knockout(attack.text);

            case CombatOutcome.DropWeapon:
                context.Drop((IItem)weapon);
                return new PositiveInteractionResult(attack.text);
        }

        return new NoNounMatchInteractionResult();
    }

    private PositiveInteractionResult Knockout(string text)
    {
        _thief!.IsUnconscious = true;
        return new PositiveInteractionResult(text);
    }

    private InteractionResult DeathBlow(IContext context, string attackText)
    {
        _thief!.IsDead = true;

        // And he vanishes. Poof. 
        _thief.CurrentLocation = null;

        var result = $"{attackText}\nAlmost as soon as the thief breathes his last breath, a " +
                     $"cloud of sinister black fog envelops him, and when the fog lifts, " +
                     $"the carcass has disappeared. " + (context.HasItem<Sword>()
                         ? "Your sword is no longer glowing. "
                         : "");

        var sb = new StringBuilder(result);

        if (_thief.TreasureStash.Any())
        {
            sb.AppendLine("\nAs the thief dies, the power of his magic decreases, and his treasures reappear:");
            foreach (var item in _thief.TreasureStash)
            {
                if(item is Egg)
                    sb.AppendLine("\tA jewel-encrusted egg, with a golden clockwork canary");    
                else
                    sb.AppendLine($"\t{item.GenericDescription(context.CurrentLocation)}");
                
                context.Drop(item);
            }
        }

        sb.AppendLine("The chalice is now safe to take.");

        return new PositiveInteractionResult(sb.ToString());
    }
}