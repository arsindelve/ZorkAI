using FluentAssertions;
using GameEngine;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

public class ExplosionTests : EngineTestsBase
{

    [Test]
    public async Task Experience_IntoEscapePod()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("Wait");
        await target.GetResponse("Wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should().Contain("A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. The door to port slides open.");

        response = await target.GetResponse("west");
        response.Should().Contain("The ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing.");

        response = await target.GetResponse("wait");
        response.Should().Contain("The pod door clangs shut as heavy explosions continue to buffet the Feinstein");

        response = await target.GetResponse("wait");
        response.Should().Contain("Explosions continue to rock the ship.");

        // response = await target.GetResponse("wait");
        // response.Should().Contain("An enormous explosion tears the walls of the ship apart. If only you had made it to an escape pod...");
        // response.Should().Contain("You have died");

    }

    [Test]
    public async Task Experience_StayOnDeckNine()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("Wait");
        await target.GetResponse("Wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should().Contain("A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. The door to port slides open.");

        response = await target.GetResponse("wait");
        response.Should().Contain("More distant explosions! A narrow emergency bulkhead at the base of the gangway and a wider one along the corridor to starboard both crash shut!");

        response = await target.GetResponse("wait");
        response.Should().Contain("More powerful explosions buffet the ship. The lights flicker madly, and the escape-pod bulkhead clangs shut.");

        response = await target.GetResponse("wait");
        response.Should().Contain("Explosions continue to rock the ship.");

        response = await target.GetResponse("wait");
        response.Should().Contain("An enormous explosion tears the walls of the ship apart. If only you had made it to an escape pod...");
        response.Should().Contain("You have died");

    }

    [Test]
    public async Task Experience_On_DeckEight()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("Up");
        await target.GetResponse("Up");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should().Contain("A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls.");
        response.Should().Contain("bellows Blather, turning a deepening shade of crimson");
        response.Should().Contain("Blather, looking slightly disoriented, barks at you to resume your assigned duties");

        response = await target.GetResponse("wait");
        response.Should().Contain("You are deafened by more explosions and by the sound of emergency bulkheads slamming closed. Blather, foaming slightly at the mouth, screams at you to swab the decks.");

        response = await target.GetResponse("wait");
        response.Should().Contain("The ship rocks from the force of multiple explosions. The lights go out, and you feel a sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Experience_In_ReactorLobby()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("east");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should().Contain("A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls.");
        response.Should().Contain("bellows Blather, turning a deepening shade of crimson");
        response.Should().Contain("Blather, looking slightly disoriented, barks at you to resume your assigned duties");

        response = await target.GetResponse("wait");
        response.Should().Contain("You are deafened by more explosions and by the sound of emergency bulkheads slamming closed. Blather, foaming slightly at the mouth, screams at you to swab the decks.");

        response = await target.GetResponse("wait");
        response.Should().Contain("The ship rocks from the force of multiple explosions. The lights go out, and you feel a sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Experience_In_Gangway()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Up");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should().Contain("A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls.");

        response = await target.GetResponse("wait");
        response.Should().Contain("Another explosion. A narrow bulkhead at the base of the gangway slams shut!");

        response = await target.GetResponse("wait");
        response.Should().Contain("The ship rocks from the force of multiple explosions. The lights go out, and you feel a sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Experience_In_Brig()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Up");
        await target.GetResponse("Up");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");
        response.Should().Contain("brig");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");

        response = await target.GetResponse("wait");
        response.Should().Contain("A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls.");

        response = await target.GetResponse("wait");
        response.Should().Contain("The ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing.");

        response = await target.GetResponse("wait");
        response.Should().Contain("The ship rocks from the force of multiple explosions. The lights go out, and you feel a sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...");
        response.Should().Contain("You have died");
    }
}
