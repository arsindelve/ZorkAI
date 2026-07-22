using GameEngine.Location;
using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Computer;
using Utilities;

namespace Planetfall.Location.Lawanda;

internal class MiniaturizationBooth : LocationBase, ICardActivatedDevice
{
    public override string Name => "Miniaturization Booth";

    public override string[] NounsForMatching => ["shrink booth", "miniaturizer"];

    public bool IsEnabled { get; set; }

    // Issue #399: sliding the miniaturization card activates the booth for only 30 turns in the original
    // (<QUEUE I-TURNOFF-MINI 30>, globals.zil:1424). CardActivationTimer counts this down and clears
    // IsEnabled; without it the booth stayed activated forever.
    [UsedImplicitly] public int ActivationTurnsRemaining { get; set; }

    // Shown when the window lapses while the player is in the booth (I-TURNOFF-MINI,
    // comptwo.zil:2390-2394); silent otherwise.
    public string DeactivationAnnouncement => "A recorded voice says \"Miniaturization booth de-activated.\" ";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        return Task.FromResult(CardActivationTimer.Tick(this, context));
    }

    public override void Init()
    {
        StartWithItem<MiniaturizationSlot>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<ComputerRoom>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a small room barely large enough for one person. Mounted on the wall is a small slot, " +
            "and next to it a keyboard with numeric keys. The exit is to the north. ";
    }

    // The keyboard's scenery nouns (issue #315). Single source of truth: both the examinable scenery
    // and the keypress noun-guard (issue #433) match against this so they can't drift apart.
    private static readonly string[] KeyboardNouns =
        ["keyboard", "numeric keyboard", "keypad", "keys", "numeric keys"];

    // The keyboard is described in the room prose but is not a discrete game object (it's scenery the
    // booth handles via the type/press verbs above). Without this, "examine keyboard" fell through to
    // the narrator, which falsely told the player the keyboard wasn't here (issue #315).
    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(KeyboardNouns,
            "It's a simple numeric keypad, its ten keys numbered zero through nine. ",
            "The keyboard is mounted firmly to the wall. ")
    ];

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // Issue #433: gate on the noun, not the verb alone. Without this, "push slot" / "press wall"
        // (any type/press/push/key verb on any noun) hit the keyboard logic instead of the actual noun,
        // shadowing the booth's own slot. Only operate the keyboard when the noun is the keyboard itself
        // or a numeric key (e.g. "type 384"); anything else falls through to the noun/narrator.
        if (action.MatchVerb(Verbs.TypeVerbs.Union(["press", "push", "key"]).ToArray())
            && (action.MatchNoun(KeyboardNouns) || action.Noun.ToInteger().HasValue))
        {
            if (!IsEnabled)
                return new PositiveInteractionResult("A recording says \"Internal computer repair booth not activated.\"");

            var keyPress = action.Noun.ToInteger();

            if (keyPress.HasValue)
            {
                if (keyPress.Value == 384)
                {
                    // Teleport to Station 384. Using the booth consumes the activation and cancels its
                    // expiry countdown (mirrors the original disabling I-TURNOFF-MINI once used).
                    CardActivationTimer.Cancel(this, context);
                    context.CurrentLocation = Repository.GetLocation<Station384>();

                    var message = "You notice the walls of the booth sliding away in all directions, followed by a momentary queasiness in the pit of your stomach...\n\n" +
                        await new LookProcessor().Process(string.Empty, context, client, Runtime.Unknown);
                    return new PositiveInteractionResult(message);
                }

                // Wrong sector - player dies
                return new DeathProcessor().Process(
                    "Ooops! You seem to have transported yourself into an active sector of the computer. You are fried by powerful electric currents.",
                    context);
            }

            return new PositiveInteractionResult("The keyboard only has numeric keys. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}