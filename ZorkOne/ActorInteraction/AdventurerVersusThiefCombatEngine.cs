using System.Text;
using GameEngine;
using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.ActorInteraction;

/// <summary>
/// The AdventurerVersusThiefCombatEngine class handles combat interactions specifically
/// between an adventurer and the thief in the game. This includes determining the combat
/// outcomes such as misses, stuns, disarms, knockouts, and fatalities based on weapon usage
/// and random choices.
/// </summary>
internal class AdventurerVersusThiefCombatEngine(IRandomChooser chooser) : ICombatEngine
{
    private readonly List<(CombatOutcome outcome, string text)> _notStunnedOutcomes =
    [
        (CombatOutcome.DropOwnWeapon, " You parry a low thrust, and your {weapon} slips out of your hand. "),

        (CombatOutcome.Miss, "Clang! Crash! The thief parries. "),
        (CombatOutcome.Miss, "The thief is struck on the arm; blood begins to trickle down. "),
        (CombatOutcome.Miss, "A good stroke, but it's too slow; the thief dodges. "),
        (CombatOutcome.Miss, "You charge, but the thief jumps nimbly aside. "),
        (CombatOutcome.Miss, "A good slash, but it misses the thief by a mile. "),
        (CombatOutcome.Miss, "A quick stroke, but the thief is on guard. "),
        (CombatOutcome.Miss, "Slash! Your stroke connects! This could be serious! "),
        (CombatOutcome.Miss, "You parry a lightning thrust, and the thief salutes you with a grim nod. "),
        (CombatOutcome.Miss, "Your {weapon} misses the thief by an inch. "),

        (CombatOutcome.Stun, "The thief is momentarily disoriented and can't fight back. "),
        (CombatOutcome.Stun, "The force of your blow knocks the thief back, stunned. "),
        (CombatOutcome.Stun, "The thief is confused and can't fight back."),
        (CombatOutcome.Stun, "The thief is staggered, and drops to his knees. "),

        (CombatOutcome.Fatal, "It's curtains for the thief as your {weapon} removes his head. "),

        (CombatOutcome.Knockout, "The thief drops to the floor, unconscious. "),
        (CombatOutcome.Knockout, "The thief is knocked out! "),
        (CombatOutcome.Knockout, "Your {weapon} crashes down, knocking the thief into dreamland. "),
        (CombatOutcome.Knockout, "A furious exchange, and the thief is knocked out! "),
        (CombatOutcome.Knockout, "The thief is battered into unconsciousness. "),
        (CombatOutcome.Knockout, "The haft of your {weapon} knocks out the thief. "),

        (CombatOutcome.Disarm, "The thief is disarmed by a subtle feint past his guard. "),
        (CombatOutcome.Disarm, "The thief's weapon is knocked to the floor, leaving him unarmed. ")
    ];

    private Thief? _thief;

    /// <summary>
    /// Executes an attack on the thief using the specified weapon and context. Determines the outcome of the attack
    /// based on various factors, such as whether the thief is stunned or unconscious, or the type of attack chosen.
    /// Returns the interaction result of the attack.
    /// </summary>
    /// <param name="context">The context in which the interaction occurs, providing environment and state information.</param>
    /// <param name="weapon">The weapon used to execute the attack. If null, there is no attack.</param>
    /// <returns>
    /// An <see cref="InteractionResult"/> representing the outcome of the attack. Returns null if no weapon is used.
    /// Possible outcomes include misses, disarming, stunning, knockouts, fatal attacks, or dropping the user's weapon.
    /// </returns>
    public InteractionResult? Attack(IContext context, IWeapon? weapon)
    {
        // You can't bare-knuckle with the thief. No weapon, no fight. 
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

        var attack = chooser.Choose(_notStunnedOutcomes);
        attack.text = attack.text.Replace("{weapon}",
            ((ItemBase?)weapon)?.NounsForMatching.FirstOrDefault() ?? " weapon ");

        switch (attack.outcome)
        {
            case CombatOutcome.Miss:
                return new PositiveInteractionResult(attack.text);

            case CombatOutcome.Stun:
                _thief.IsStunned = true;
                return new PositiveInteractionResult(attack.text);

            case CombatOutcome.Disarm:
                _thief.IsDisarmed = true;
                return new PositiveInteractionResult(attack.text);
            
            case CombatOutcome.Fatal:
                return DeathBlow(context, attack.text);

            case CombatOutcome.Knockout:
                return Knockout(attack.text);

            case CombatOutcome.DropOwnWeapon:
                context.Drop((IItem)weapon);
                return new PositiveInteractionResult(attack.text);
        }

        return new NoNounMatchInteractionResult();
    }

    private PositiveInteractionResult Knockout(string text)
    {
        if (_thief is null)
            throw new ArgumentNullException();

        _thief.IsUnconscious = true;
        return new PositiveInteractionResult(text);
    }

    private InteractionResult DeathBlow(IContext context, string attackText)
    {
        if (_thief is null)
            throw new ArgumentNullException();

        _thief.IsDead = true;
        _thief.IsUnconscious = false;
        _thief.TreasureStash.Add(Repository.GetItem<Stiletto>());

        context.RemoveActor(_thief);

        // And he vanishes. Poof. 
        _thief.CurrentLocation?.Items.Remove(_thief);
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
                if (item is Egg)
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