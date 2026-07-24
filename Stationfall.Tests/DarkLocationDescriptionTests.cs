using FluentAssertions;

namespace Stationfall.Tests;

public class DarkLocationDescriptionTests
{
    // Stationfall keeps Infocom's grue joke for dark rooms, but with its OWN station-flavored wording:
    // "It is pitch black. You hope there are no grues aboard the station." — NOT Zork's "You are likely
    // to be eaten by a grue." nor Planetfall's "You might be eaten by a grue." Verbatim from the original
    // DESCRIBE-ROOM routine (stationfall/verbs.zil:2668-2676). This is the per-game divergence the
    // game-supplied DarkLocationDescription seam exists to allow; it also guards against StationfallGame
    // being (re)wired to another game's line. Asserted on the game directly because Phase 1 has no dark
    // room yet to route a "look" through — a full end-to-end test arrives with the first dark location.
    [Test]
    public void StationfallGame_SuppliesItsOwnGrueWording_NotZorksNorPlanetfalls()
    {
        var description = new StationfallGame().DarkLocationDescription;

        description.Should().Contain("You hope there are no grues aboard the station");
        description.Should().NotContain("likely to be eaten");
        description.Should().NotContain("might be eaten");
    }
}
