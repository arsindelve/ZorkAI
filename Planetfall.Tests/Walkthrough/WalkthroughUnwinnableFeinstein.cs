using JetBrains.Annotations;

namespace Planetfall.Tests.Walkthrough;

// Unwinnable walkthroughs, part one: the Feinstein.
//
// The regular walkthroughs in this folder all document the winning line. These document the losing
// ones - the sequences after which the game is over, or can no longer be won, no matter what the
// player types next. They exist so that a refactor which quietly makes a trap survivable (or makes a
// survivable turn lethal) fails a test instead of silently changing the game.
//
// Every fixture plays from move one, exactly as a player would, so the trap is reached the same way
// the player reaches it - not by dropping into a god-mode state next to it.

/// <summary>
///     You never go through the pod bulkhead. The Feinstein gives you four turns after the first
///     explosion, then tears itself apart around you.
/// </summary>
[TestFixture]
public sealed class WalkthroughNeverEnterTheEscapePod : WalkthroughTestBase
{
    [Test]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "A massive explosion rocks the ship", "The door to port slides open")]
    // The pod is standing wide open and one move away for the next two turns. This walkthrough
    // spends both of them standing still.
    [TestCase("wait", null, "More distant explosions")]
    [TestCase("wait", null, "The lights flicker madly", "the escape-pod bulkhead clangs shut")]
    // The bulkhead is shut now. Nothing typed from here can change the ending.
    [TestCase("west", null, "The escape pod bulkhead is closed")]
    [TestCase("wait", null,
        "An enormous explosion tears the walls of the ship apart",
        "If only you had made it to an escape pod",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     You wander off Deck Nine before the explosion. The emergency bulkheads seal behind you and the
///     pod is not merely closed, it is unreachable - two turns later the ship kills you where you stand.
/// </summary>
[TestFixture]
public sealed class WalkthroughStrandedAwayFromTheEscapePod : WalkthroughTestBase
{
    [Test]
    [TestCase("up", null, "Gangway")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "A massive explosion rocks the ship")]
    // No "the door to port slides open" here - that only happens where you can see it, and you are a
    // deck away. Going back down is still possible on this turn; the next one takes it away.
    [TestCase("wait", null, "A narrow bulkhead at the base of the gangway slams shut")]
    [TestCase("wait", null,
        "The ship rocks from the force of multiple explosions",
        "Too bad you weren't in the escape pod",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     You reach the pod but never strap in. The webbing is not decoration: it is the only thing that
///     keeps you off the control panel when the pod hits the planet, and the landing is fatal without it.
/// </summary>
[TestFixture]
public sealed class WalkthroughNeverSitInTheSafetyWebbing : WalkthroughTestBase
{
    [Test]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "A massive explosion rocks the ship")]
    [TestCase("port", null, "Escape Pod", "A mass of safety webbing")]
    // Every remaining turn is spent NOT sitting down.
    [TestCase("wait", null, "The pod door clangs shut")]
    [TestCase("wait", null, "You feel the pod begin to slide down its ejection")]
    // The first bill comes due here. Loose in the pod when the Feinstein goes up, you are thrown
    // against the bulkhead; the game says as plainly as it ever says anything that the webbing would
    // have helped.
    [TestCase("wait", null,
        "the escape pod tumbles away from th",
        "You are thrown against the bulkhead, bruising a few limbs",
        "The safety webbing might have offered a bit more protection")]
    [TestCase("wait", null, "The auxiliary rockets fire ")]
    [TestCase("wait", null, "The main thrusters fire a long")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "The pod is buffeted")]
    [TestCase("wait", null, "You feel the temperature")]
    [TestCase("wait", null, "The viewport suddenly becomes transparent")]
    [TestCase("wait", null, "The pod is now approaching")]
    // And the second bill. The pod lands; you do not.
    [TestCase("wait", null,
        "lands with a good deal of force",
        "stopped by one of the sharper corners of the control panel",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The same refusal to strap in, but this time the one-in-five roll goes the other way and the
///     Feinstein's death throes finish you ten turns before the landing would have.
/// </summary>
[TestFixture]
public sealed class WalkthroughThrownAgainstTheBulkheadHeadFirst : WalkthroughTestBase
{
    [UsedImplicitly]
    public void TheRollGoesBadly()
    {
        ThrownAgainstTheBulkheadIsFatal = true;
    }

    [Test]
    [TestCase("wait", "TheRollGoesBadly", "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "A massive explosion rocks the ship")]
    [TestCase("port", null, "Escape Pod")]
    [TestCase("wait", null, "The pod door clangs shut")]
    [TestCase("wait", null, "You feel the pod begin to slide down its ejection")]
    [TestCase("wait", null,
        "sending the escape pod tumbling away",
        "You are thrown against the bulkhead, head first",
        "getting in the safety webbing would have been a good idea",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     You do everything right, land safely - and then stand up. The pod is balanced on a cleft above
///     the water, and standing tips it in. From that moment there is a short, fixed number of turns in
///     which to get out; this walkthrough spends them all waiting.
/// </summary>
[TestFixture]
public sealed class WalkthroughStandUpInTheLandedPodAndDrown : WalkthroughTestBase
{
    [Test]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "A massive explosion rocks the ship")]
    [TestCase("port", null, "Escape Pod")]
    [TestCase("sit", null, "You are now safely cushioned within the web")]
    [TestCase("wait", null, "You feel the pod begin to slide down its ejection")]
    [TestCase("wait", null, "the escape pod tumbles away from th")]
    [TestCase("wait", null, "The auxiliary rockets fire ")]
    [TestCase("wait", null, "The main thrusters fire a long")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "The pod is buffeted")]
    [TestCase("wait", null, "You feel the temperature")]
    [TestCase("wait", null, "The viewport suddenly becomes transparent")]
    [TestCase("wait", null, "The pod is now approaching")]
    [TestCase("wait", null, "The pod lands with a thud", "precariously balanced")]
    // Standing up is what tips the pod off its ledge. The clock this starts cannot be stopped by
    // sitting back down - only by getting out of the pod.
    [TestCase("stand", null, "the pod shifts slightly and you feel it falling", "water rising past the viewport")]
    [TestCase("wait", null, "The pod is now completely submerged", "continuing to sink")]
    [TestCase("wait", null, "The pod creaks ominously from the increasing pressure")]
    [TestCase("wait", null,
        "The pod splits open, and water pours in",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The alien ambassador wanders past on his way somewhere, munching something that looks a great
///     deal like celery. It is not celery, and Blow'k-Bibben-Gordoan metabolism is not compatible with
///     ours. This is the shortest losing walkthrough in the game: four moves.
/// </summary>
[TestFixture]
public sealed class WalkthroughEatTheAmbassadorsCelery : WalkthroughTestBase
{
    [UsedImplicitly]
    public void TheAmbassadorWandersIn()
    {
        DeckNineEncounterRoll = 1;
    }

    [Test]
    [TestCase("wait", "TheAmbassadorWandersIn", "Time passes")]
    [TestCase("wait", null,
        "The alien ambassador from the planet Blow'k-bibben-Gordo ambles toward you",
        "He is munching on something resembling an enormous stalk of celery")]
    // He will not hand it over - taking it is a protocol violation, not a theft.
    [TestCase("take celery", null, "The ambassador seems perturbed by your lack of normal protocol")]
    // Eating it is neither.
    [TestCase("eat celery", null,
        "Blow'k-Bibben-Gordoan metabolism is not compatible with our own",
        "You die of all sorts of convulsions",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     Ensign First Class Blather catches you off your post on Deck Eight. He is a bully with a
///     clipboard and the game invites you to do something about it. Do not: he is not a combatant, he
///     is an authored instant death.
/// </summary>
[TestFixture]
public sealed class WalkthroughPickAFightWithBlather : WalkthroughTestBase
{
    [Test]
    [TestCase("up", null, "Gangway")]
    [TestCase("up", null, "Deck Eight", "Ensign Blather, his uniform immaculate, enters", "Twenty demerits")]
    [TestCase("examine blather", null, "a tall, beefy officer with a tremendous, misshapen nose")]
    // Saluting is the correct answer, and the game rewards it. This walkthrough does not salute.
    [TestCase("attack blather", null,
        "Blather removes several of your appendages and internal organs",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The same encounter, played by simply ignoring him. Blather runs out of patience on the fifth
///     turn and throws you in the brig, and the brig has one exit, which is locked. Nothing you type
///     from there matters: the Feinstein explodes on schedule with you inside it.
/// </summary>
[TestFixture]
public sealed class WalkthroughGetThrownInTheBrigAndBlowUpWithTheShip : WalkthroughTestBase
{
    [Test]
    [TestCase("up", null, "Gangway")]
    [TestCase("up", null, "Deck Eight", "Ensign Blather, his uniform immaculate, enters", "Forty if you're not back on Deck Nine in five seconds")]
    [TestCase("wait", null, "I said to return to your post, Ensign Seventh Class!")]
    [TestCase("wait", null, "I said to return to your post, Ensign Seventh Class!")]
    [TestCase("wait", null, "I said to return to your post, Ensign Seventh Class!")]
    [TestCase("wait", null,
        "Blather loses his last vestige of patience and drags you to the",
        "brig",
        "the door clangs shut behind you",
        "Graffiti cover the walls")]
    // From here the game is decided. The cell door is the only exit and it does not open.
    [TestCase("south", null, "The cell door is locked")]
    [TestCase("open cell door", null, "No way, Jose")]
    [TestCase("wait", null, "Time passes")]
    // The explosion arrives on its own schedule whether or not you can do anything about it.
    [TestCase("wait", null, "A massive explosion rocks the ship")]
    [TestCase("wait", null, "the sounds of emergency bulkheads closing")]
    [TestCase("wait", null,
        "The ship rocks from the force of multiple explosions",
        "Too bad you weren't in the escape pod",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}
