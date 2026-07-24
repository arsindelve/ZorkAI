using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class HadesTests : EngineTestsBase
{
    [Test]
    public async Task InHades_RingBell_SpiritsThere_BellIsDropped()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();
        var bell = Repository.GetItem<BrassBell>();
        target.Context.Take(bell);
        target.Context.Take(Repository.GetItem<Torch>());

        var response = await target.GetResponse("ring bell");
        response.Should().Contain("The bell suddenly becomes red hot and falls to the ground.");
        target.Context.HasItem<BrassBell>().Should().BeFalse();
        Repository.GetLocation<EntranceToHades>().HasItem<BrassBell>().Should().BeTrue();
    }

    [Test]
    public async Task InHades_RingBell_SpiritsThere_BellIsRedHot()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();
        var bell = Repository.GetItem<BrassBell>();
        var torch = Repository.GetItem<Torch>();
        target.Context.Take(torch);
        target.Context.Take(bell);

        await target.GetResponse("ring bell");
        var response = await target.GetResponse("look");

        response.Should().Contain("On the ground is a red hot brass bell.");
        Repository.GetItem<BrassBell>().BellIsRedHot.Should().BeTrue();
    }

    [Test]
    public async Task InHades_RingBell_SpiritsThere_SpiritsGetStunned()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();
        var bell = Repository.GetItem<BrassBell>();
        var torch = Repository.GetItem<Torch>();
        target.Context.Take(torch);
        target.Context.Take(bell);

        var response = await target.GetResponse("ring bell");

        response.Should().Contain("stop their jeering and slowly turn to face you");
        Repository.GetItem<Spirits>().Stunned.Should().BeTrue();
    }

    [Test]
    public async Task CannotPickUpRedHotBell()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();
        var bell = Repository.GetItem<BrassBell>();
        var torch = Repository.GetItem<Torch>();
        target.Context.Take(torch);
        target.Context.Take(bell);

        await target.GetResponse("ring bell");
        var response = await target.GetResponse("take bell");

        response.Should().Contain("The bell is very hot and cannot be taken.");
        Repository.GetItem<BrassBell>().CurrentLocation.Should().Be(Repository.GetLocation<EntranceToHades>());
    }


    [Test]
    public async Task BellCoolsDown()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();
        var bell = Repository.GetItem<BrassBell>();
        var torch = Repository.GetItem<Torch>();
        target.Context.Take(torch);
        target.Context.Take(bell);

        await target.GetResponse("ring bell");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        response.Should().Contain("The bell appears to have cooled down.");
        response = await target.GetResponse("take bell");
        response.Should().Contain("Taken");
        target.Context.HasItem<BrassBell>().Should().BeTrue();
    }

    [Test]
    public async Task SpiritsResumeJeering()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();
        var bell = Repository.GetItem<BrassBell>();
        var torch = Repository.GetItem<Torch>();
        target.Context.Take(torch);
        target.Context.Take(bell);

        await target.GetResponse("ring bell");
        // Ringing the bell gives the player six turns to get the candles lit (ZIL queues I-XB for
        // 6 - 1actions.zil:1101), so the jeering resumes on the sixth turn after the bell.
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        response.Should().Contain("The tension of this ceremony is broken");
        Repository.GetItem<Spirits>().Stunned.Should().BeFalse();
    }


    /// <summary>
    ///     Puts the adventurer in Hades holding everything the exorcism needs, with the candles lit
    ///     as they are when taken from the altar.
    /// </summary>
    private static void SetupForTheCeremony(GameEngine.GameEngine<ZorkI, ZorkIContext> target)
    {
        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();
        target.Context.Take(Repository.GetItem<Torch>()); // light, so reading isn't blocked by darkness
        target.Context.Take(Repository.GetItem<BrassBell>());
        target.Context.Take(Repository.GetItem<BlackBook>());
        target.Context.Take(Repository.GetItem<Matchbook>());
        target.Context.Take(Repository.GetItem<Candles>());
        Repository.GetItem<Candles>().IsOn = true; // as they are when taken from the altar
    }

    [Test]
    public async Task ReadBook_AfterRingingBell_ButCandlesNotLit_DoesNotBanish()
    {
        // The exorcism is a two-phase ceremony (ZIL 1actions.zil:1102-1125): ringing the bell
        // stuns the spirits (XB), but reading the book only banishes them once the candles are
        // carried AND lit (XC). Ringing the bell drops the candles and puts them out, so the
        // player must re-take and re-light them. Reading the book with unlit candles must do nothing.
        var target = GetTarget();
        SetupForTheCeremony(target);

        var ring = await target.GetResponse("ring bell");
        // The bell is what puts the candles out; that is why phase two has to be redone.
        ring.Should().Contain("the candles drop to the ground and they are out");
        Repository.GetItem<Candles>().IsOn.Should().BeFalse();

        var response = await target.GetResponse("read book");

        response.Should().NotContain("Begone");
        response.Should().Contain("Commandment"); // falls through to the ordinary book text
        Repository.GetItem<Spirits>().CurrentLocation
            .Should().Be(Repository.GetLocation<EntranceToHades>()); // not banished
    }

    [Test]
    public async Task ReadBook_AfterRingingBell_WithCandlesLit_Banishes()
    {
        // The happy path: after ringing the bell, re-take and re-light the candles, then read the
        // book to banish the spirits. (WalkthroughTestOne also covers this end-to-end.)
        var target = GetTarget();
        SetupForTheCeremony(target);

        await target.GetResponse("ring bell"); // candles drop, go out
        await target.GetResponse("take candles"); // re-take them
        await target.GetResponse("light a match");
        await target.GetResponse("light candles with match"); // candles lit again
        var response = await target.GetResponse("read book");

        response.Should().Contain("Begone, fiends!");
        Repository.GetItem<Spirits>().CurrentLocation.Should().BeNull(); // banished
    }

    [Test]
    public async Task LightingTheCandlesOnTheGround_DoesNotAdvanceTheCeremony()
    {
        // Phase two requires the candles be CARRIED as well as lit (ZIL 1actions.zil:1116-1119
        // tests IN? CANDLES WINNER alongside the ONBIT flag). Lighting them where they fell must
        // not announce that the ceremony advanced - otherwise the game tells the player the
        // spirits are cowering and then silently refuses to banish them when they read the book.
        var target = GetTarget();
        SetupForTheCeremony(target);

        await target.GetResponse("ring bell"); // candles drop to the ground and go out
        await target.GetResponse("light a match");
        var lighting = await target.GetResponse("light candles with match"); // never picked back up

        Repository.GetItem<Candles>().IsOn.Should().BeTrue();
        target.Context.HasItem<Candles>().Should().BeFalse();
        lighting.Should().NotContain("The spirits cower at your unearthly power");

        var response = await target.GetResponse("read book");
        response.Should().NotContain("Begone");
        Repository.GetItem<Spirits>().CurrentLocation
            .Should().Be(Repository.GetLocation<EntranceToHades>());
    }

    [Test]
    public async Task ReadBook_AfterTheCandlesWereLitThenDropped_StillBanishes()
    {
        // Phase two latches (ZIL sets the XC global at 1actions.zil:1120 and the book's banishment
        // tests that latch, not the candles, at :1102). Once the flames have flickered and the
        // spirits have cowered, the player has a few turns to read the book; what happens to the
        // candles in the meantime is irrelevant.
        var target = GetTarget();
        SetupForTheCeremony(target);

        await target.GetResponse("ring bell");
        await target.GetResponse("take candles");
        await target.GetResponse("light a match");
        await target.GetResponse("light candles with match"); // phase two - the latch is set
        await target.GetResponse("drop candles");
        var response = await target.GetResponse("read book");

        response.Should().Contain("Begone, fiends!");
        Repository.GetItem<Spirits>().CurrentLocation.Should().BeNull();
    }

    [Test]
    public async Task TheCeremony_SurvivesACoupleOfWastedTurns()
    {
        // The original is forgiving: the bell buys six turns to get the candles lit (I-XB 6,
        // 1actions.zil:1101) and lighting them then starts a fresh three-turn window to read the
        // book (I-XC 3, :1124). A player who spends a turn or two looking around must not lose
        // the ceremony - the minimum four commands should not have to fit exactly.
        var target = GetTarget();
        SetupForTheCeremony(target);

        await target.GetResponse("ring bell");
        await target.GetResponse("examine the gate"); // wasted turn
        await target.GetResponse("examine the gate"); // another one
        await target.GetResponse("take candles");
        await target.GetResponse("light a match");
        await target.GetResponse("light candles with match");
        await target.GetResponse("examine the gate"); // dawdle again, inside the phase-two window
        var response = await target.GetResponse("read book");

        response.Should().Contain("Begone, fiends!");
        Repository.GetItem<Spirits>().CurrentLocation.Should().BeNull();
    }

    [Test]
    [TestCase("eat the spirits")]
    [TestCase("kiss the spirits")]
    [TestCase("take the spirits")]
    [TestCase("examine the spirits")]
    public async Task YouCannotDoAnythingToTheGhosts(string input)
    {
        var target = GetTarget();
        var torch = Repository.GetItem<Torch>();
        target.Context.Take(torch);
        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();

        var response = await target.GetResponse(input);
        response.Should().Contain("You seem unable to interact with these spirits");
    }
}