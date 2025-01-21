using GameEngine;
using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.ActorInteraction;

internal class AdventurerVersusTrollCombatEngine() : ICombatEngine
{
    private readonly IRandomChooser _chooser = new RandomChooser();
    private readonly List<(CombatOutcome outcome, string text)> _notStunnedOutcomes =
    [
        (CombatOutcome.Miss, "A quick stroke, but the troll is on guard. "),
        (CombatOutcome.Fatal, "The troll takes a fatal blow and slumps to the floor dead."),
        (CombatOutcome.Knockout, "The haft of your {weapon} knocks out the troll. "),
        (CombatOutcome.Miss, "You charge, but the troll jumps nimbly aside."),
        (CombatOutcome.Miss, "A good stroke, but it's too slow; the troll dodges."),
        (CombatOutcome.Miss, "Your {weapon} misses the troll by an inch."),
        (CombatOutcome.Knockout, "The troll is knocked out! "),
        (CombatOutcome.Miss, "The troll is confused and can't fight back. The troll slowly regains his feet."),
        (CombatOutcome.Miss, "Clang! Crash! The troll parries."),
        (CombatOutcome.Miss, "A good slash, but it misses the troll by a mile."),
        (CombatOutcome.Knockout, "The troll is battered into unconsciousness. "),
        (CombatOutcome.Fatal, "The fatal blow strikes the troll square in the heart: He dies. "),
        (CombatOutcome.Knockout, "Your {weapon} crashes down, knocking the troll into dreamland. "),
        (CombatOutcome.Fatal, "It's curtains for the troll as your {weapon} removes his head. "),
        (CombatOutcome.Stun, "The force of your blow knocks the troll back, stunned."),
        (CombatOutcome.Stun, "The troll is momentarily disoriented and can't fight back. ")
    ];

    private IItem? _axe;
    private Troll? _troll;
    private TrollRoom? _trollRoom;

    /// <summary>
    ///     Constructor for unit testing
    /// </summary>
    /// <param name="chooser"></param>
    public AdventurerVersusTrollCombatEngine(IRandomChooser chooser) : this()
    {
        _chooser = chooser;
    }
    
    public InteractionResult? Attack(IContext context, IWeapon? weapon)
    {
        // You can't bare-knuckle with the troll. No weapon, no fight. 
        if (weapon is null)
            return null;

        // Don't assign these in the constructor or as initializers. You'll
        // get stack overflow errors. 
        _troll = Repository.GetItem<Troll>();
        _trollRoom = Repository.GetLocation<TrollRoom>();
        _axe = Repository.GetItem<BloodyAxe>();

        if (context is ZorkIContext { IsStunned: true } zorkContext)
        {
            zorkContext.IsStunned = false;
            return new PositiveInteractionResult(
                "You are still recovering from that last blow, so your attack is ineffective. ");
        }

        if (_troll.IsUnconscious)
            return DeathBlow(context, "The unconscious troll cannot defend himself: He dies. ");

        if (!_troll.HasItem<BloodyAxe>())
            return DeathBlow(context, "The unarmed troll cannot defend himself: He dies.");

        var attack = _chooser.Choose(_notStunnedOutcomes);
        attack.text = attack.text.Replace("{weapon}",
            ((ItemBase?)weapon)?.NounsForMatching.FirstOrDefault() ?? " weapon ");

        switch (attack.outcome)
        {
            case CombatOutcome.Miss:
                return new PositiveInteractionResult(attack.text);

            case CombatOutcome.Stun:
                _troll.IsStunned = true;
                return new PositiveInteractionResult(attack.text);

            case CombatOutcome.Fatal:
                return DeathBlow(context, attack.text);

            case CombatOutcome.Knockout:
                return Knockout(attack.text);
        }

        return new NoNounMatchInteractionResult();
    }

    private PositiveInteractionResult DeathBlow(IContext context, string attackText)
    {
        _troll!.IsDead = true;

        // Dead troll drops the axe. 
        if (_troll.HasItem<BloodyAxe>())
            _trollRoom!.ItemPlacedHere(_axe!);

        // And he vanishes. Poof. 
        _troll.CurrentLocation = null;

        return new PositiveInteractionResult($"{attackText}\nAlmost as soon as the troll breathes his last breath, a " +
                                             $"cloud of sinister black fog envelops him, and when the fog lifts, " +
                                             $"the carcass has disappeared. " + (context.HasItem<Sword>()
                                                 ? "Your sword is no longer glowing."
                                                 : ""));
    }

    private PositiveInteractionResult Knockout(string text)
    {
        _troll!.IsUnconscious = true;

        // Unconscious troll drops the axe
        _trollRoom!.ItemPlacedHere(_axe!);

        return new PositiveInteractionResult(text);
    }
}