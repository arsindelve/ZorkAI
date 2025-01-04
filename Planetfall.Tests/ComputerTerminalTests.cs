using FluentAssertions;
using Planetfall.Item.Lawanda.Library.Computer;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class ComputerTerminalTests : EngineTestsBase
{
    [Test]
    public async Task Look()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();

        var response = await target.GetResponse("look");
        response.Should().Contain("On the table is a computer terminal");
    }

    [Test]
    public async Task TurnOn()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();

        var response = await target.GetResponse("activate terminal");
        response.Should().Contain("The screen gives off a green flash, and then some writing appears on the screen:");
        response.Should()
            .Contain(
                "    1. Histooree\n    2. Kulcur\n    3. Teknolojee\n    4. Jeeografee\n    5. Xe Prajekt\n    6. Inturlajik Gaamz");
    }

    [Test]
    public async Task AlreadyOff()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();

        var response = await target.GetResponse("deactivate terminal");
        response.Should().Contain("isn't on");
    }

    [Test]
    public async Task Off_Read()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();

        var response = await target.GetResponse("read terminal");
        response.Should().Contain("The screen is dark");
    }

    [Test]
    public async Task Off_Examine()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();

        var response = await target.GetResponse("examine terminal");
        response.Should()
            .Contain(
                "The computer terminal consists of a video display screen, a keyboard with ten keys numbered from zero through nine, and an on-off switch. The screen is dark");
    }

    [Test]
    public async Task On_Examine()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        var response = await target.GetResponse("examine terminal");
        response.Should()
            .Contain(
                "The computer terminal consists of a video display screen, a keyboard with ten keys numbered from zero through nine, and an on-off switch. The screen displays some writing:");
        response.Should()
            .Contain(
                "    1. Histooree\n    2. Kulcur\n    3. Teknolojee\n    4. Jeeografee\n    5. Xe Prajekt\n    6. Inturlajik Gaamz");
    }

    [Test]
    public async Task On_Read()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        var response = await target.GetResponse("read terminal");
        response.Should()
            .Contain(
                "    1. Histooree\n    2. Kulcur\n    3. Teknolojee\n    4. Jeeografee\n    5. Xe Prajekt\n    6. Inturlajik Gaamz");
    }

    [Test]
    public async Task AlreadyOn()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        var response = await target.GetResponse("activate terminal");
        response.Should().Contain("already on");
    }

    [Test]
    [TestCase("type 1")]
    [TestCase("key 1")]
    [TestCase("push 1")]
    [TestCase("press 1")]
    [TestCase("punch 1")]
    [TestCase("type one")]
    [TestCase("key one")]
    [TestCase("push one")]
    [TestCase("press one")]
    [TestCase("punch one")]
    public async Task TypeOneFromMainMenu(string command)
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        var response = await target.GetResponse(command);
        response.Should()
            .Contain(
                "The screen clears and a different menu appears:\n\n    0. Maan Menyuu\n    1. Raashul Orijinz\n    2. Graat Hiiaatus\n    3. Riiz uv xe Nuu Teknakrasee\n");
    }

    [TestCase("punch zero")]
    [TestCase("type 0")]
    public async Task TypeZeroFromMainMenu(string command)
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        var response = await target.GetResponse(command);
        response.Should()
            .Contain(
                "The terminal feeps, and a message briefly appears on the screen explaining that typing that character has no meaning at the moment");
    }

    [TestCase("punch 17")]
    [TestCase("type 17")]
    [TestCase("type seventeen")]
    [TestCase("press 17")]
    public async Task TypeWrongNumberFromMainMenu(string command)
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        var response = await target.GetResponse(command);
        response.Should()
            .Contain(
                "The terminal feeps, and a message briefly appears on the screen explaining that typing that character has no meaning at the moment");
    }

    [TestCase("punch hello")]
    [TestCase("type dude")]
    [Test]
    public async Task TypeWordsFromMainMenu(string command)
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        var response = await target.GetResponse(command);
        response.Should().Contain("The keyboard only has the keys 0 through 9");
    }

    [Test]
    [TestCase("one", "0", MainMenu.MainMenuText)]
    [TestCase("one", "1", HistoryMenu.MenuOne)]
    [TestCase("one", "2", HistoryMenu.MenuTwo)]
    [TestCase("one", "3", HistoryMenu.MenuThree)]
    [TestCase("one", "4", MenuState.NoEffect)]
    [TestCase("one", "5", MenuState.NoEffect)]
    [TestCase("two", "0", MainMenu.MainMenuText)]
    [TestCase("two", "1", CultureMenu.MenuOne)]
    [TestCase("two", "2", CultureMenu.MenuTwo)]
    [TestCase("two", "3", CultureMenu.MenuThree)]
    [TestCase("two", "4", MenuState.NoEffect)]
    [TestCase("three", "0", MainMenu.MainMenuText)]
    [TestCase("three", "1", TechnologyMenu.MenuOne)]
    [TestCase("three", "2", TechnologyMenu.MenuTwo)]
    [TestCase("three", "3", TechnologyMenu.MenuThree)]
    [TestCase("three", "4", TechnologyMenu.MenuFour)]
    [TestCase("three", "5", TechnologyMenu.MenuFive)]
    [TestCase("three", "6", MenuState.NoEffect)]
    [TestCase("four", "0", MainMenu.MainMenuText)]
    [TestCase("four", "1", GeographyMenu.MenuOne)]
    [TestCase("four", "2", GeographyMenu.MenuTwo)]
    [TestCase("four", "3", GeographyMenu.MenuThree)]
    [TestCase("four", "4", MenuState.NoEffect)]
    [TestCase("five", "0", MainMenu.MainMenuText)]
    [TestCase("five", "1", ProjectMenu.MenuOne)]
    [TestCase("five", "2", ProjectMenu.MenuTwo)]
    [TestCase("five", "3", ProjectMenu.MenuThree)]
    [TestCase("five", "4", MenuState.NoEffect)]
    [TestCase("six", "0", MainMenu.MainMenuText)]
    [TestCase("six", "1", GamesMenu.MenuOne)]
    [TestCase("six", "2", GamesMenu.MenuTwo)]
    [TestCase("six", "3", GamesMenu.MenuThree)]
    [TestCase("six", "4", MenuState.NoEffect)]
    public async Task GoIntoChildMenu(string firstPress, string secondPress, string expectedResponse)
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        await target.GetResponse($"type {firstPress}");
        var response = await target.GetResponse($"press {secondPress}");
        response.Should().Contain(expectedResponse);
    }

    [Test]
    [TestCase("one", "1", "one", MenuState.ReachedLowestLevel)]
    [TestCase("one", "1", "zero", HistoryMenu.MainMenu)]
    [TestCase("one", "2", "zero", HistoryMenu.MainMenu)]
    [TestCase("one", "3", "zero", HistoryMenu.MainMenu)]
    [TestCase("two", "1", "one", MenuState.ReachedLowestLevel)]
    [TestCase("two", "1", "zero", CultureMenu.MainMenu)]
    [TestCase("two", "2", "zero", CultureMenu.MainMenu)]
    [TestCase("two", "3", "zero", CultureMenu.MainMenu)]
    [TestCase("three", "1", "one", MenuState.ReachedLowestLevel)]
    [TestCase("three", "1", "zero", TechnologyMenu.MainMenu)]
    [TestCase("three", "2", "zero", TechnologyMenu.MainMenu)]
    [TestCase("three", "3", "zero", TechnologyMenu.MainMenu)]
    [TestCase("three", "4", "zero", TechnologyMenu.MainMenu)]
    [TestCase("three", "5", "zero", TechnologyMenu.MainMenu)]
    [TestCase("four", "1", "one", MenuState.ReachedLowestLevel)]
    [TestCase("four", "1", "zero", GeographyMenu.MainMenu)]
    [TestCase("four", "2", "zero", GeographyMenu.MainMenu)]
    [TestCase("four", "3", "zero", GeographyMenu.MainMenu)]
    [TestCase("five", "1", "one", MenuState.ReachedLowestLevel)]
    [TestCase("five", "1", "zero", ProjectMenu.MainMenu)]
    [TestCase("five", "2", "zero", ProjectMenu.MainMenu)]
    [TestCase("five", "3", "zero", ProjectMenu.MainMenu)]
    [TestCase("six", "1", "one", MenuState.ReachedLowestLevel)]
    [TestCase("six", "1", "zero", GamesMenu.MainMenu)]
    [TestCase("six", "2", "zero", GamesMenu.MainMenu)]
    [TestCase("six", "3", "zero", GamesMenu.MainMenu)]
    public async Task GoIntoChildMenuAndBackToSubmenu(string firstPress, string secondPress, string thirdPress,
        string expectedResponse)
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        await target.GetResponse($"type {firstPress}");
        await target.GetResponse($"type {secondPress}");
        var response = await target.GetResponse($"press {thirdPress}");
        response.Should().Contain(expectedResponse);
    }
}