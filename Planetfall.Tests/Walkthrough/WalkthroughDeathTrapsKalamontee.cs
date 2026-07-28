using JetBrains.Annotations;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Shuttle;

namespace Planetfall.Tests.Walkthrough;

// Point traps, Kalamontee side.
//
// The fixtures in WalkthroughUnwinnableKalamontee.cs play from move one, because in those the
// *journey* is the trap - the wrong move is only fatal because of everything you did to get there.
// The ones here are point traps: a single room, a single wrong verb. Replaying forty-five moves of
// identical prologue in front of each would add nothing, so these start from a god-mode drop next to
// the trap and walk the last few moves in, the way WalkthroughBioLock and WalkthroughMutantChase do.

/// <summary>
///     The undertow. You land on a crag with the ocean below, and the water is not scenery - the pod's
///     bulkhead opens directly into it. Two turns in the water is one turn too many.
/// </summary>
[TestFixture]
public sealed class WalkthroughLingerInTheUndertow : WalkthroughTestBase
{
    [UsedImplicitly]
    public void OnTheCrag()
    {
        StartHere<Crag>();
    }

    [Test]
    [TestCase("look", "OnTheCrag", "Crag")]
    [TestCase("down", null, "Underwater", "Currents buffet you against the sharp rocks of an underwater cliff")]
    // The way out is up, and it is available right now. This walkthrough treads water instead.
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null,
        "A mighty undertow drags you across some underwater obstructions",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The same water, entered a second time. The first swim is survivable because you have never been
///     in it before; the game does not give you that grace twice, and going back down for something you
///     left in the pod kills you the moment you arrive.
/// </summary>
[TestFixture]
public sealed class WalkthroughSwimBackDownIntoTheUndertow : WalkthroughTestBase
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
    [TestCase("wait", null, "The pod lands with a thud")]
    // Out through the water and up the cliff - the intended route, and it works.
    [TestCase("open door", null, "The bulkhead opens and cold ocean")]
    [TestCase("out", null, "Underwater")]
    [TestCase("up", null, "Crag")]
    // The towel and the survival kit are still down there in the pod. They are not worth this.
    [TestCase("down", null,
        "A mighty undertow drags you across some underwater obstructions",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The rift is eight metres across and thirty deep, with sharp rocks at the bottom, and the room
///     description says so. The game still lets you jump.
/// </summary>
[TestFixture]
public sealed class WalkthroughJumpIntoTheRift : WalkthroughTestBase
{
    [UsedImplicitly]
    public void AtTheRift()
    {
        StartHere<AdminCorridorSouth>();
    }

    [Test]
    [TestCase("north", "AtTheRift", "Admin Corridor", "gaping rift")]
    [TestCase("examine rift", null, "at least eight meters wide and more than thirty meters deep",
        "The bottom is covered with sharp and nasty rocks")]
    // Having read that description, jump in anyway.
    [TestCase("jump into rift", null,
        "You get a brief (but much closer) view of the sharp and nasty rocks at the bottom of the rift",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The machine shop dispenses a flask of milky white liquid, and the puzzle it belongs to is about
///     pouring it into a hole in the tower. It is chemical stock, not a drink.
/// </summary>
[TestFixture]
public sealed class WalkthroughDrinkTheChemicals : WalkthroughTestBase
{
    [UsedImplicitly]
    public void InTheToolRoom()
    {
        StartHere<ToolRoom>();
    }

    [Test]
    [TestCase("take flask", "InTheToolRoom", "Taken")]
    [TestCase("east", null, "Machine Shop")]
    [TestCase("put flask under spout", null, "sitting under the spout")]
    [TestCase("press black button", null, "The fluid gradually turns milky white")]
    [TestCase("take flask", null, "Taken")]
    // It is warm, it is milky white, and you have not had anything to drink since the Feinstein.
    [TestCase("drink fluid", null,
        "Mmmmm....that tasted just like delicious poisonous chemicals!",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The shuttle's speed has no effect on how long the trip takes - only on how it ends. The signs
///     counting down 15, 10, 5 in the last stretch of tunnel are the game telling you to pull the lever
///     back. This walkthrough leaves it pushed forward the whole way.
/// </summary>
[TestFixture]
public sealed class WalkthroughCrashTheShuttleIntoTheStation : WalkthroughTestBase
{
    [UsedImplicitly]
    public void InAlfiesControlCabin()
    {
        StartHere<AlfieControlEast>();
        Take<ShuttleAccessCard>();
    }

    [Test]
    [TestCase("slide shuttle access card through slot", "InAlfiesControlCabin", "activated")]
    // Lever up means accelerate, and it stays up until it is pulled back. Nothing in the tunnel will
    // pull it back for you.
    [TestCase("push lever", null, "now in the upper position", "the shuttle car begins to move forward")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "You pass a sign which says", "Limit 45")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The tunnel levels out", "Beegin Deeseluraashun")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    [TestCase("wait", null, "The shuttle car continues to move")]
    // The last three signs are a countdown, in metres, of how much tunnel is left.
    [TestCase("wait", null, "blinking red lights", "15")]
    [TestCase("wait", null, "blinking red lights", "10")]
    [TestCase("wait", null, "blinking red lights", "5")]
    [TestCase("wait", null, "approaching a brightly lit area", "the concrete platforms of a shuttle station")]
    [TestCase("wait", null,
        "The shuttle car hurtles past the platforms and rams into the wall at the far end of the station",
        "you're in no condition to care",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     Course Control, repaired, and then unrepaired. Swapping the good bedistor into the socket is
///     worth six points and fixes the planet's decaying orbit; the socket is live the moment it works,
///     and reaching back in for the part is fatal.
/// </summary>
[TestFixture]
public sealed class WalkthroughTouchTheLiveBedistor : WalkthroughTestBase
{
    [UsedImplicitly]
    public void AtCourseControlWithTheParts()
    {
        StartHere<CourseControl>();
        Take<Pliers>();
        Take<GoodBedistor>();
    }

    [Test]
    [TestCase("open cube", "AtCourseControlWithTheParts", "The lid swings open", "fused")]
    [TestCase("take fused with pliers", null, "With a tug")]
    [TestCase("put good bedistor in cube", null, "Done. The warning lights go out and another light goes on")]
    // Six points, the orbit is saved, and the socket now has current running through it.
    [TestCase("take good bedistor", null,
        "Kerzap!! You should know better than to touch an active bedistor!",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}
