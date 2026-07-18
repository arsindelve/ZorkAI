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
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        response.Should().Contain("The tension of this ceremony is broken");
        Repository.GetItem<Spirits>().Stunned.Should().BeFalse();
    }


    [Test]
    public async Task ReadBook_AfterRingingBell_ButCandlesNotLit_DoesNotBanish()
    {
        // The exorcism is a two-phase ceremony (ZIL 1actions.zil:1102-1125): ringing the bell
        // stuns the spirits (XB), but reading the book only banishes them once the candles are
        // carried AND lit (XC). Ringing the bell drops the candles and puts them out, so the
        // player must re-take and re-light them. Reading the book with unlit candles must do nothing.
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();
        target.Context.Take(Repository.GetItem<Torch>()); // light so reading isn't blocked by darkness
        target.Context.Take(Repository.GetItem<BrassBell>());
        target.Context.Take(Repository.GetItem<BlackBook>());
        target.Context.Take(Repository.GetItem<Candles>());

        await target.GetResponse("ring bell"); // spirits stunned; candles drop to the ground, unlit
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

        target.Context.CurrentLocation = Repository.GetLocation<EntranceToHades>();
        target.Context.Take(Repository.GetItem<Torch>());
        target.Context.Take(Repository.GetItem<BrassBell>());
        target.Context.Take(Repository.GetItem<BlackBook>());
        target.Context.Take(Repository.GetItem<Candles>());
        target.Context.Take(Repository.GetItem<Matchbook>());

        await target.GetResponse("ring bell"); // candles drop, go out
        await target.GetResponse("take candles"); // re-take them
        await target.GetResponse("light a match");
        await target.GetResponse("light candles with match"); // candles lit again
        var response = await target.GetResponse("read book");

        response.Should().Contain("Begone, fiends!");
        Repository.GetItem<Spirits>().CurrentLocation.Should().BeNull(); // banished
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