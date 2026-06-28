using Model.AIGeneration;
using Newtonsoft.Json;
using Planetfall.Command;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Computer;

namespace Planetfall.Item.Computer;

/// <summary>
/// The giant microbe that attacks the miniaturized player on the silicon strip once the computer
/// is fixed. Mirrors the original's MICROBE object and I-MICROBE daemon (comptwo.zil:2895-2998).
///
/// It is a multi-turn escort/combat puzzle: the microbe closes in each turn and digests the player
/// if left alone, follows the player between strip rooms, blocks the strip exits, and can only be
/// dispatched by sacrificing a heated-up laser (thrown off the strip, or fed to the microbe).
/// Shooting it directly only repels it momentarily — it always regenerates.
/// </summary>
public class Microbe : ItemBase, ITurnBasedActor, ICanBeExamined
{
    [UsedImplicitly] [JsonIgnore]
    public IRandomChooser Chooser { get; set; } = new RandomChooser();

    public override string[] NounsForMatching => ["microbe", "monster", "bug", "hungry microbe"];

    /// <summary>
    /// Shown when the microbe blocks a strip exit. Shared by the strip rooms that gate their
    /// southbound exit on <see cref="IsActive" />.
    /// </summary>
    public const string BlocksExitMessage =
        "Not a chance -- unless, of course, you don't mind walking into the gullet of a hungry microbe. ";

    /// <summary>
    /// True while the microbe is present and blocking the player on the strip (NO-MICROBE = false).
    /// </summary>
    [UsedImplicitly]
    public bool IsActive { get; set; }

    /// <summary>
    /// True once the microbe has been permanently disposed of (MICROBE-DISPATCHED). Prevents respawn.
    /// </summary>
    [UsedImplicitly]
    public bool Dispatched { get; set; }

    /// <summary>
    /// How close the microbe is to digesting you (MICROBE-COUNTER). At 2, the next idle turn kills you.
    /// Reset to 0 whenever the microbe follows you into a new strip room.
    /// </summary>
    [UsedImplicitly]
    public int Counter { get; set; }

    /// <summary>
    /// Set the turn the laser strikes the microbe (MICROBE-HIT). On a hit turn the microbe lashes out
    /// rather than closing in, so the closing counter does not advance.
    /// </summary>
    [UsedImplicitly]
    public bool HitThisTurn { get; set; }

    /// <summary>
    /// True on the turn the microbe spawns, so its closing daemon doesn't also fire that same turn —
    /// the spawn text is the turn-zero beat; the microbe begins advancing the following turn.
    /// </summary>
    [UsedImplicitly]
    public bool JustSpawned { get; set; }

    public string ExaminationDescription =>
        "A hungry microbe blocks your way, its cilia waving and its pseudopods towering over you. ";

    private readonly List<string> _microbeLashesOut =
    [
        "A pseudopod extends toward you. You jump back just in time to avoid being engulfed.",
        "A slimy pseudopod brushes against your shoulder. You twist away in the nick of time.",
        "A pseudopod shoots out toward your head! Ducking quickly, you save your life.",
        "Two protoplasm-filled blobs sneak toward you from the left. You jump to the side and almost fall off the strip into the void below!"
    ];

    private readonly List<string> _monsterCloses =
    [
        "The microbe slithers closer. The cilia around its gullet glisten with mucus, giving the impression that the microbe is salivating.",
        "The microbe flows toward you. It towers above you, its cilia waving madly in your face.",
        "The monster wriggles nearer. It is now so close that you can make out details in the protoplasm beneath its translucent skin."
    ];

    private const string DigestDeath =
        "The microbe wraps several pseudopods around you and shoves you into its mucus-covered gullet. " +
        "Digestive juices begin their work. The experience is not pleasant.";

    private const string LungeDeath =
        "The microbe, whipped into a rabid frenzy by the waves of heat from the pulsing laser, literally " +
        "lunges at it. You jump back and, losing your balance, fall over the edge of the strip. The microbe, " +
        "writhing madly, hurls itself after its prey. You and the microbe both plunge into the void below.";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!IsActive)
        {
            context.RemoveActor(this);
            return Task.FromResult(string.Empty);
        }

        // Skip the closing daemon on the spawn turn — the spawn text already played this turn.
        if (JustSpawned)
        {
            JustSpawned = false;
            return Task.FromResult(string.Empty);
        }

        var laser = Repository.GetItem<Laser>();
        var holdingLaser = laser.CurrentLocation == context;
        // The microbe is registered as an actor before the laser, so this reads the warmth at the
        // START of the turn — the current shot's increment lands later in Laser.Act. The thresholds
        // below (>7, >13) are therefore "warmth coming into this turn."
        var warmth = laser.WarmthLevel;

        if (HitThisTurn)
        {
            HitThisTurn = false;
            var message = Chooser.Choose(_microbeLashesOut);

            // At blistering heat, holding the laser when it lashes out is fatal — it lunges at the
            // pulsing weapon and drags you both off the strip (I-MICROBE, WARMTH-FLAG > 13).
            if (warmth > 13 && holdingLaser)
                return Task.FromResult(Die(LungeDeath, context));

            // Warm-but-not-deadly: a pseudopod grabs for the laser and you snatch it away.
            if (warmth > 7 && holdingLaser)
                message +=
                    " Another pseudopod, perhaps attracted by the warmth of the laser, tries to envelop " +
                    "the weapon. You snatch it away from the monster's grasp.";

            return Task.FromResult(message);
        }

        // Not struck this turn — it closes in. A third idle turn (counter already at 2) is fatal.
        if (Counter >= 2)
            return Task.FromResult(Die(DigestDeath, context));

        Counter++;
        return Task.FromResult(Chooser.Choose(_monsterCloses));
    }

    private string Die(string message, IContext context)
    {
        IsActive = false;
        context.RemoveActor(this);
        return new DeathProcessor().Process(message, context).InteractionMessage;
    }

    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.ExamineVerbs, NounsForMatching))
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(ExaminationDescription));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        // "give/throw laser to/at microbe" (either noun ordering). Match the laser's full synonym set
        // (e.g. "laazur") rather than the literal "laser", since the AI parser may pass a synonym.
        var laserNouns = Repository.GetItem<Laser>().NounsForMatching;
        var givingToMicrobe =
            action.MatchVerb(["give", "throw", "feed"]) &&
            ((action.MatchNounOne(laserNouns) && action.MatchNounTwo(NounsForMatching)) ||
             (action.MatchNounOne(NounsForMatching) && action.MatchNounTwo(laserNouns)));

        if (givingToMicrobe && IsActive)
            return Task.FromResult<InteractionResult?>(GiveLaser(context));

        return base.RespondToMultiNounInteraction(action, context);
    }

    private InteractionResult GiveLaser(IContext context)
    {
        var laser = Repository.GetItem<Laser>();

        // The microbe only bothers with the laser once it's been heated up (WARMTH-FLAG > 7).
        if (laser.WarmthLevel <= 7)
            return new PositiveInteractionResult(
                "The microbe ignores the laser, but does attempt to digest your arm. ");

        // It devours the laser either way; only a truly hot laser (WARMTH-FLAG > 10) does it in.
        var hotEnoughToKill = laser.WarmthLevel > 10;
        MicrobeFightHelper.RemoveLaserFromGame(laser, context);

        if (!hotEnoughToKill)
            return new PositiveInteractionResult(
                "The microbe greedily devours the laser, and turns toward you. ");

        MicrobeFightHelper.Dispatch(this, context);
        return new PositiveInteractionResult(
            "The microbe gobbles up the laser and turns toward you. A moment later, it begins writhing " +
            "in pain. Apparently, eating the hot laser was a bit too much for it. With a bellow of agony, " +
            "it rolls off the edge of the strip. (Whew!) ");
    }

    /// <summary>
    /// Spawns the microbe into the given strip room and registers its daemon. Idempotent: does nothing
    /// if it has already spawned or been dispatched.
    /// </summary>
    public string? SpawnOnStrip(IContext context, ILocation here)
    {
        if (IsActive || Dispatched)
            return null;

        IsActive = true;
        JustSpawned = true;
        Counter = 0;
        here.ItemPlacedHere(this);
        context.RegisterActor(this);

        return
            "Suddenly, with a loud plop, a giant elephant-sized monster lands on the strip just in front " +
            "of you. It is amorphously shaped, its skin a slimy translucent red membrane. While most of " +
            "your brain screams with panic about the disgusting monster that now blocks your exit, some " +
            "small section in the back of your mind calmly realizes that this is merely some tiny microbe " +
            "which has somehow violated the sterile environment of the computer interior.\n\n" +
            "As you stand frozen with fear, the microbe slithers toward you, extending slimy pseudopods " +
            "thick with waving cilia. It looks pretty hungry, and seems intent on having you for lunch. ";
    }

    /// <summary>
    /// Moves the already-present microbe northward into the room the player just entered, resetting
    /// the closing counter (STRIP-NEAR-RELAY-F M-ENTER: "The microbe ... follows you northward.").
    /// Only ever invoked for the northbound chase, so the hardcoded "northward" is always accurate.
    /// </summary>
    public string? FollowInto(ILocation here)
    {
        if (!IsActive)
            return null;

        here.ItemPlacedHere(this);
        Counter = 0;
        return "The microbe, writhing angrily, follows you northward. ";
    }
}
