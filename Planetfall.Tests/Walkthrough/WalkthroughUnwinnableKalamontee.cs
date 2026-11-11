namespace Planetfall.Tests.Walkthrough;

// Unwinnable walkthroughs, part two: Kalamontee.
//
// Unlike the Feinstein traps, none of these kill you. You keep playing, the parser keeps answering,
// the score never moves again - the game is simply over and does not say so. Each fixture plays the
// opening exactly as the winning walkthroughs do, takes one wrong turn, and then demonstrates the
// consequence by showing the now-permanently-closed door it leads to.
//
// Everything downstream of the rift - both offices, the shuttle, all of Lawanda, the computer, the
// Cure - hangs off two objects: the curved metal bar that fishes the steel key out of the crevice,
// and the extendable ladder the key unlocks. Lose either one and the entire second half of the game
// becomes unreachable.

/// <summary>
///     You throw the magnet into the rift. The steel key stays in the crevice forever, so the mess
///     door stays padlocked, so the ladder is never taken, so the rift is never crossed.
/// </summary>
[TestFixture]
public sealed class WalkthroughThrowTheMagnetIntoTheRift : WalkthroughTestBase
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
    [TestCase("take kit", null, "Taken")]
    [TestCase("open door", null, "The bulkhead opens and cold ocean")]
    [TestCase("out", null, "Underwater")]
    [TestCase("up", null, "Crag")]
    [TestCase("up", null, "Balcony")]
    [TestCase("up", null, "Winding Stair")]
    [TestCase("up", null, "Courtyard")]
    [TestCase("N", null, "Plain Hall")]
    [TestCase("NE", null, "Rec Corridor")]
    [TestCase("E", null, "Mess Corridor")]
    [TestCase("E", null, "Dorm Corridor")]
    [TestCase("E", null, "Corridor Junction")]
    [TestCase("S", null, "Mech Corridor North")]
    [TestCase("S", null, "Mech Corridor")]
    [TestCase("S", null, "Mech Corridor South")]
    [TestCase("SW", null, "Tool Room")]
    [TestCase("take magnet", null, "Taken")]
    [TestCase("NE", null, "Mech Corridor South")]
    [TestCase("N", null, "Mech Corridor")]
    [TestCase("N", null, "Mech Corridor North")]
    [TestCase("N", null, "Corridor Junction")]
    [TestCase("N", null, "Admin Corridor South")]
    [TestCase("N", null, "Admin Corridor", "gaping rift")]
    // The one wrong move. Nothing warns you, and there is no second magnet anywhere in the game.
    [TestCase("throw magnet into rift", null, "sails gracefully into the rift")]
    [TestCase("S", null, "Admin Corridor South")]
    // The key is still there. You can see it. That is now all you will ever be able to do with it.
    [TestCase("examine crevice", null, "Lying at the bottom of the narrow crack", "shiny steel key")]
    [TestCase("take key", null, "Either the crevice is too narrow, or your fingers are too large")]
    [TestCase("S", null, "Corridor Junction")]
    [TestCase("W", null, "Dorm Corridor")]
    [TestCase("W", null, "Mess Corridor")]
    // No key, so the padlock never opens, so Storage West and the ladder inside it stay sealed.
    [TestCase("remove padlock", null, "The padlock is locked to the door")]
    [TestCase("N", null, "The door is closed")]
    // And with no ladder there is no way over the rift, which is where the rest of the game is.
    [TestCase("E", null, "Dorm Corridor")]
    [TestCase("E", null, "Corridor Junction")]
    [TestCase("N", null, "Admin Corridor South")]
    [TestCase("N", null, "Admin Corridor")]
    [TestCase("N", null, "The rift is too wide to jump across")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     You get the key, the padlock and the ladder, and then bridge the rift with the ladder still
///     collapsed. Two-and-a-half metres of aluminium do not span eight metres of rift, and the game
///     tells you exactly what that costs: "lost forever".
/// </summary>
[TestFixture]
public sealed class WalkthroughBridgeTheRiftWithACollapsedLadder : WalkthroughTestBase
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
    [TestCase("take kit", null, "Taken")]
    [TestCase("open door", null, "The bulkhead opens and cold ocean")]
    [TestCase("out", null, "Underwater")]
    [TestCase("up", null, "Crag")]
    [TestCase("up", null, "Balcony")]
    [TestCase("up", null, "Winding Stair")]
    [TestCase("up", null, "Courtyard")]
    [TestCase("N", null, "Plain Hall")]
    [TestCase("NE", null, "Rec Corridor")]
    [TestCase("E", null, "Mess Corridor")]
    [TestCase("E", null, "Dorm Corridor")]
    [TestCase("E", null, "Corridor Junction")]
    [TestCase("S", null, "Mech Corridor North")]
    [TestCase("S", null, "Mech Corridor")]
    [TestCase("S", null, "Mech Corridor South")]
    [TestCase("SW", null, "Tool Room")]
    [TestCase("take magnet", null, "Taken")]
    [TestCase("NE", null, "Mech Corridor South")]
    [TestCase("N", null, "Mech Corridor")]
    [TestCase("N", null, "Mech Corridor North")]
    [TestCase("N", null, "Corridor Junction")]
    [TestCase("N", null, "Admin Corridor South")]
    [TestCase("put magnet on crevice", null, "clank", "steel key")]
    [TestCase("S", null, "Corridor Junction")]
    [TestCase("W", null, "Dorm Corridor")]
    [TestCase("W", null, "Mess Corridor")]
    [TestCase("unlock padlock with key", null, "springs open")]
    [TestCase("remove lock", null, "Taken")]
    [TestCase("open door", null, "Opened")]
    [TestCase("N", null, "Storage West")]
    [TestCase("drop all", null, "Dropped")]
    [TestCase("take ladder", null, "Taken")]
    [TestCase("S", null, "Mess Corridor")]
    [TestCase("E", null, "Dorm Corridor")]
    [TestCase("E", null, "Corridor Junction")]
    [TestCase("N", null, "Admin Corridor South")]
    [TestCase("N", null, "Admin Corridor")]
    // Two-and-a-half metres of ladder against eight metres of rift. The game does not stop you, and
    // it does not give the ladder back.
    [TestCase("examine ladder", null, "currently collapsed", "around two-and-a-half meters long")]
    [TestCase("place ladder across rift", null, "far too short to reach the other edge", "lost forever")]
    [TestCase("N", null, "The rift is too wide to jump across")]
    // There is exactly one ladder in the game and it is at the bottom of the rift.
    [TestCase("S", null, "Admin Corridor South")]
    [TestCase("S", null, "Corridor Junction")]
    [TestCase("W", null, "Dorm Corridor")]
    [TestCase("W", null, "Mess Corridor")]
    [TestCase("N", null, "Storage West")]
    [TestCase("look", null, "This is a small room obviously intended as a storage area")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     You bridge the rift properly, cross it - and then collapse the ladder while standing on the far
///     side. The ladder falls into the rift and takes the only way back with it. You are alive, in a
///     dead end, holding cards for doors you can no longer reach.
/// </summary>
[TestFixture]
public sealed class WalkthroughCollapseTheLadderFromTheFarSideOfTheRift : WalkthroughTestBase
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
    [TestCase("take kit", null, "Taken")]
    [TestCase("open door", null, "The bulkhead opens and cold ocean")]
    [TestCase("out", null, "Underwater")]
    [TestCase("up", null, "Crag")]
    [TestCase("up", null, "Balcony")]
    [TestCase("up", null, "Winding Stair")]
    [TestCase("up", null, "Courtyard")]
    [TestCase("N", null, "Plain Hall")]
    [TestCase("NE", null, "Rec Corridor")]
    [TestCase("E", null, "Mess Corridor")]
    [TestCase("E", null, "Dorm Corridor")]
    [TestCase("E", null, "Corridor Junction")]
    [TestCase("S", null, "Mech Corridor North")]
    [TestCase("S", null, "Mech Corridor")]
    [TestCase("S", null, "Mech Corridor South")]
    [TestCase("SW", null, "Tool Room")]
    [TestCase("take magnet", null, "Taken")]
    [TestCase("NE", null, "Mech Corridor South")]
    [TestCase("N", null, "Mech Corridor")]
    [TestCase("N", null, "Mech Corridor North")]
    [TestCase("N", null, "Corridor Junction")]
    [TestCase("N", null, "Admin Corridor South")]
    [TestCase("put magnet on crevice", null, "clank", "steel key")]
    [TestCase("S", null, "Corridor Junction")]
    [TestCase("W", null, "Dorm Corridor")]
    [TestCase("W", null, "Mess Corridor")]
    [TestCase("unlock padlock with key", null, "springs open")]
    [TestCase("remove lock", null, "Taken")]
    [TestCase("open door", null, "Opened")]
    [TestCase("N", null, "Storage West")]
    [TestCase("drop all", null, "Dropped")]
    [TestCase("take ladder", null, "Taken")]
    [TestCase("S", null, "Mess Corridor")]
    [TestCase("E", null, "Dorm Corridor")]
    [TestCase("E", null, "Corridor Junction")]
    [TestCase("N", null, "Admin Corridor South")]
    [TestCase("N", null, "Admin Corridor")]
    [TestCase("drop ladder", null, "Dropped")]
    [TestCase("extend ladder", null, "The ladder extends ")]
    [TestCase("place ladder across rift", null, "The ladder swings out ")]
    [TestCase("N", null, "Admin Corridor North", "slowly make your way across the swaying")]
    [TestCase("W", null, "Small Office")]
    [TestCase("open desk", null, "Opening the small")]
    [TestCase("take upper card", null, "Taken")]
    [TestCase("take kitchen card", null, "Taken")]
    [TestCase("E", null, "Admin Corridor North")]
    // Tidying up. The ladder is the bridge you are standing on the wrong side of.
    [TestCase("collapse ladder", null, "As the ladder shortens it plunges into the rift")]
    [TestCase("S", null, "The rift is too wide to jump across")]
    // A handful of rooms with no exit, and a pocketful of cards for slots on the other side of the rift.
    [TestCase("W", null, "Small Office")]
    [TestCase("E", null, "Admin Corridor North")]
    [TestCase("S", null, "The rift is too wide to jump across")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     You keep the curved metal bar after it has done its one job. It is a magnet, and it quietly
///     wipes one access card per turn for as long as you carry it. Nothing announces the damage - you
///     find out at the first slot you try, by which point every card you were holding is dead.
/// </summary>
[TestFixture]
public sealed class WalkthroughCarryTheMagnetWithTheAccessCards : WalkthroughTestBase
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
    [TestCase("take kit", null, "Taken")]
    [TestCase("open door", null, "The bulkhead opens and cold ocean")]
    [TestCase("out", null, "Underwater")]
    [TestCase("up", null, "Crag")]
    [TestCase("up", null, "Balcony")]
    [TestCase("up", null, "Winding Stair")]
    [TestCase("up", null, "Courtyard")]
    [TestCase("N", null, "Plain Hall")]
    [TestCase("NE", null, "Rec Corridor")]
    [TestCase("E", null, "Mess Corridor")]
    [TestCase("E", null, "Dorm Corridor")]
    [TestCase("E", null, "Corridor Junction")]
    [TestCase("S", null, "Mech Corridor North")]
    [TestCase("S", null, "Mech Corridor")]
    [TestCase("S", null, "Mech Corridor South")]
    [TestCase("SW", null, "Tool Room")]
    [TestCase("take magnet", null, "Taken")]
    [TestCase("NE", null, "Mech Corridor South")]
    [TestCase("N", null, "Mech Corridor")]
    [TestCase("N", null, "Mech Corridor North")]
    [TestCase("N", null, "Corridor Junction")]
    [TestCase("N", null, "Admin Corridor South")]
    [TestCase("put magnet on crevice", null, "clank", "steel key")]
    // The bar has now done everything it will ever usefully do. Set it down here for the moment -
    // the ladder is a two-handed job.
    [TestCase("drop magnet", null, "Dropped")]
    [TestCase("S", null, "Corridor Junction")]
    [TestCase("W", null, "Dorm Corridor")]
    [TestCase("W", null, "Mess Corridor")]
    [TestCase("unlock padlock with key", null, "springs open")]
    [TestCase("remove lock", null, "Taken")]
    [TestCase("open door", null, "Opened")]
    [TestCase("N", null, "Storage West")]
    [TestCase("drop all", null, "Dropped")]
    [TestCase("take ladder", null, "Taken")]
    [TestCase("S", null, "Mess Corridor")]
    [TestCase("E", null, "Dorm Corridor")]
    [TestCase("E", null, "Corridor Junction")]
    [TestCase("N", null, "Admin Corridor South")]
    [TestCase("N", null, "Admin Corridor")]
    [TestCase("drop ladder", null, "Dropped")]
    [TestCase("extend ladder", null, "The ladder extends ")]
    [TestCase("place ladder across rift", null, "The ladder swings out ")]
    // ...and then picks the bar back up on the way past, because it is a tool, and hoarding tools is
    // what adventurers do.
    [TestCase("S", null, "Admin Corridor South")]
    [TestCase("take magnet", null, "Taken")]
    [TestCase("N", null, "Admin Corridor")]
    [TestCase("N", null, "Admin Corridor North", "slowly make your way across the swaying")]
    [TestCase("W", null, "Small Office")]
    [TestCase("open desk", null, "Opening the small")]
    // Each of these lands in a pocket next to a magnet. The turn it is taken is the turn it dies, and
    // the game says nothing at all.
    [TestCase("take upper card", null, "Taken")]
    [TestCase("take kitchen card", null, "Taken")]
    [TestCase("E", null, "Admin Corridor North")]
    [TestCase("S", null, "Admin Corridor", "slowly make your way across the swaying")]
    [TestCase("S", null, "Admin Corridor South")]
    [TestCase("S", null, "Corridor Junction")]
    [TestCase("W", null, "Dorm Corridor")]
    [TestCase("W", null, "Mess Corridor")]
    [TestCase("S", null, "Mess Hall")]
    // First slot of the game, first news of the damage. The kitchen holds the lower elevator card,
    // which is the only way down to the shuttle and Lawanda.
    [TestCase("slide kitchen access card through slot", null, "Damejd kard", "akses deeniid")]
    [TestCase("S", null, "The door is closed")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}
