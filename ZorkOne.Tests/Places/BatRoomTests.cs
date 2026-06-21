using FluentAssertions;
using GameEngine;
using Model.Interface;
using Model.Location;
using Moq;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.CoalMineLocation;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class BatRoomTests : EngineTestsBase
{
    private static IEnumerable<TestCaseData> AllBatDropRooms()
    {
        yield return new TestCaseData((Func<ILocation>)(() => Repository.GetLocation<CoalMineOne>())).SetName("{m}(CoalMineOne)");
        yield return new TestCaseData((Func<ILocation>)(() => Repository.GetLocation<CoalMineTwo>())).SetName("{m}(CoalMineTwo)");
        yield return new TestCaseData((Func<ILocation>)(() => Repository.GetLocation<CoalMineThree>())).SetName("{m}(CoalMineThree)");
        yield return new TestCaseData((Func<ILocation>)(() => Repository.GetLocation<CoalMineFour>())).SetName("{m}(CoalMineFour)");
        yield return new TestCaseData((Func<ILocation>)(() => Repository.GetLocation<LadderTop>())).SetName("{m}(LadderTop)");
        yield return new TestCaseData((Func<ILocation>)(() => Repository.GetLocation<LadderBottom>())).SetName("{m}(LadderBottom)");
        yield return new TestCaseData((Func<ILocation>)(() => Repository.GetLocation<SqueakyRoom>())).SetName("{m}(SqueakyRoom)");
        yield return new TestCaseData((Func<ILocation>)(() => Repository.GetLocation<MineEntrance>())).SetName("{m}(MineEntrance)");
    }

    [TestCaseSource(nameof(AllBatDropRooms))]
    public async Task EnterFromTheEastWithoutGarlic_CarriesPlayerOffToTheChosenRoom(Func<ILocation> getExpectedRoom)
    {
        var target = GetTarget();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        target.Context.CurrentLocation = Repository.GetLocation<ShaftRoom>();

        var expectedRoom = getExpectedRoom();
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.Choose(It.IsAny<List<ILocation>>())).Returns(expectedRoom);
        Repository.GetLocation<BatRoom>().Chooser = mockChooser.Object;

        // Act - enter the Bat Room from the east (the Shaft Room)
        var response = await target.GetResponse("west");
        Console.WriteLine(response);

        // Assert
        response.Should().Contain("Fweep!");
        response.Should().Contain("The bat grabs you by the scruff of your neck and lifts you away....");
        target.Context.CurrentLocation.Should().Be(expectedRoom);
    }

    [Test]
    public async Task EnterFromTheSouthWithoutGarlic_CarriesPlayerOff()
    {
        var target = GetTarget();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        target.Context.CurrentLocation = Repository.GetLocation<SqueakyRoom>();

        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.Choose(It.IsAny<List<ILocation>>()))
            .Returns(Repository.GetLocation<MineEntrance>());
        Repository.GetLocation<BatRoom>().Chooser = mockChooser.Object;

        // Act - enter the Bat Room from the south (the Squeaky Room)
        var response = await target.GetResponse("north");
        Console.WriteLine(response);

        // Assert
        response.Should().Contain("Fweep!");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<MineEntrance>());
    }

    [Test]
    public async Task EnterWithGarlicInInventory_IsSafe()
    {
        var target = GetTarget();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        target.Context.Take(Repository.GetItem<Garlic>());
        target.Context.CurrentLocation = Repository.GetLocation<ShaftRoom>();

        var response = await target.GetResponse("west");
        Console.WriteLine(response);

        response.Should().NotContain("Fweep!");
        response.Should().Contain("deranged and holding his nose");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<BatRoom>());
    }

    [Test]
    public async Task GarlicLooseOnTheBatRoomFloor_IsSafe()
    {
        var target = GetTarget();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        Repository.GetLocation<BatRoom>().ItemPlacedHere(Repository.GetItem<Garlic>());
        target.Context.CurrentLocation = Repository.GetLocation<ShaftRoom>();

        var response = await target.GetResponse("west");
        Console.WriteLine(response);

        response.Should().NotContain("Fweep!");
        response.Should().Contain("deranged and holding his nose");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<BatRoom>());
    }

    [Test]
    public async Task TakingTheBatWithoutGarlic_CarriesPlayerOff()
    {
        var target = GetTarget();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        target.Context.CurrentLocation = Repository.GetLocation<BatRoom>();

        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.Choose(It.IsAny<List<ILocation>>()))
            .Returns(Repository.GetLocation<MineEntrance>());
        Repository.GetLocation<BatRoom>().Chooser = mockChooser.Object;

        var response = await target.GetResponse("take bat");
        Console.WriteLine(response);

        response.Should().Contain("Fweep!");
        response.Should().Contain("The bat grabs you by the scruff of your neck and lifts you away....");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<MineEntrance>());
    }

    [Test]
    public async Task TakingTheBatWithGarlic_CannotReachIt()
    {
        var target = GetTarget();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        target.Context.Take(Repository.GetItem<Garlic>());
        target.Context.CurrentLocation = Repository.GetLocation<BatRoom>();

        var response = await target.GetResponse("take bat");
        Console.WriteLine(response);

        response.Should().Contain("You can't reach him; he's on the ceiling.");
        response.Should().NotContain("Fweep!");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<BatRoom>());
    }
}
