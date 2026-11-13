using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location.CoalMineLocation;

namespace ZorkOne.Tests.Places;

/// <summary>
///     The basket on its chain. It answers <see cref="Model.Verbs.RaiseVerbs" /> and
///     <see cref="Model.Verbs.LowerVerbs" /> - the same grouping the living room rug answers when it is
///     lifted, because raising a thing is one action whatever the thing is.
/// </summary>
public class ShaftRoomTests : EngineTestsBase
{
    private GameEngine<ZorkI, ZorkIContext> AtTheTopOfTheShaft()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<ShaftRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        return target;
    }

    [TestCase("raise basket")]
    [TestCase("lift basket")]
    [TestCase("hoist basket")]
    public async Task TheBasketComesBackUp_HoweverThePlayerSaysToRaiseIt(string input)
    {
        var target = AtTheTopOfTheShaft();
        await target.GetResponse("lower basket");

        var response = await target.GetResponse(input);

        response.Should().Contain("The basket is raised to the top of the shaft");
        Repository.GetLocation<ShaftRoom>().Items.Should().Contain(Repository.GetItem<Basket>());
    }

    [Test]
    public async Task LoweringTheBasket_SendsItToTheBottom()
    {
        var target = AtTheTopOfTheShaft();

        var response = await target.GetResponse("lower basket");

        response.Should().Contain("The basket is lowered to the bottom of the shaft");
        Repository.GetLocation<DraftyRoom>().Items.Should().Contain(Repository.GetItem<Basket>());
    }
}
