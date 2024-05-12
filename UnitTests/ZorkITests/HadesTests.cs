namespace UnitTests.ZorkITests;

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
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        response.Should().Contain("The tension of this ceremony is broken");
        Repository.GetItem<Spirits>().Stunned.Should().BeFalse();
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