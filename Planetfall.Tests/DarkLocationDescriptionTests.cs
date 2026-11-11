using FluentAssertions;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Tests;

public class DarkLocationDescriptionTests : EngineTestsBase
{
    // Planetfall keeps Infocom's grue joke for dark rooms, but with its OWN wording:
    // "It is pitch black. You might be eaten by a grue." — NOT Zork's "You are likely to be eaten
    // by a grue." Verbatim from the original DESCRIBE-ROOM routine (planetfall/verbs.zil). This is
    // exactly the per-game divergence the game-supplied DarkLocationDescription seam exists to allow.
    [Test]
    public async Task DarkRoom_UsesPlanetfallsOwnGrueWording_NotZorks()
    {
        var target = GetTarget();
        StartHere<TransportationSupply>(); // a forever-dark room; the player has no light source here

        var response = await target.GetResponse("look");

        response.Should().Contain("You might be eaten by a grue");
        response.Should().NotContain("likely to be eaten");
    }
}
