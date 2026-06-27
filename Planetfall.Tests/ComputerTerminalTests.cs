using FluentAssertions;
using Planetfall;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
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
                "1. Histooree");
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
    public async Task Off_TypeKey_DoesNotNavigateMenu_AndScreenIsDark()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();

        var terminal = GetItem<ComputerTerminal>();
        terminal.IsOn.Should().BeFalse();

        var response = await target.GetResponse("type 1");

        // The keypress must not navigate the menu while the terminal is off.
        terminal.MenuState.CurrentItem.Should().BeOfType<MainMenu>();
        response.Should().Contain("screen is dark");
    }

    [Test]
    public async Task Off_TypeKeyMultiNoun_DoesNotNavigateMenu_AndScreenIsDark()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();

        var terminal = GetItem<ComputerTerminal>();
        terminal.IsOn.Should().BeFalse();

        var response = await target.GetResponse("type 1 on the keyboard");

        terminal.MenuState.CurrentItem.Should().BeOfType<MainMenu>();
        response.Should().Contain("screen is dark");
    }

    [Test]
    public async Task Off_TypeNonNumericKey_DoesNotNavigateMenu_AndScreenIsDark()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();

        var terminal = GetItem<ComputerTerminal>();
        terminal.IsOn.Should().BeFalse();

        // Even garbage input reports a dark screen: the IsOn guard sits *before* the
        // keyPress.HasValue check, so an off terminal can't even complain about a bad keystroke.
        var response = await target.GetResponse("type dude");

        terminal.MenuState.CurrentItem.Should().BeOfType<MainMenu>();
        response.Should().Contain("screen is dark");
        response.Should().NotContain("keys 0 through 9");
    }

    [Test]
    public async Task Off_TypeKey_FloydDoesNotComment()
    {
        var target = GetTarget();
        var libraryLobby = GetLocation<LibraryLobby>();
        target.Context.CurrentLocation = libraryLobby;

        // Floyd present and active, but the terminal is off.
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = libraryLobby;
        libraryLobby.ItemPlacedHere(floyd);

        await target.GetResponse("type 1");

        // Floyd must not comment on "first use" while the screen is dark, and the menu must not move.
        GetItem<ComputerTerminal>().MenuState.CurrentItem.Should().BeOfType<MainMenu>();
        var pfContext = (PlanetfallContext)target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
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
                "1. Histooree");
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
                "1. Histooree");
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
                "The screen clears and a different menu appears:\n\n    0. Maan Menyuu");
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

    [Test]
    [TestCase("type 1 on the keyboard")]
    [TestCase("type one on the keyboard")]
    [TestCase("key 4 on the terminal")]
    [TestCase("type 2 on the screen")]
    public async Task TypeNumberOnTerminal_MultiNoun_NavigatesIntoSubmenu(string command)
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        var response = await target.GetResponse(command);
        response.Should()
            .Contain(
                "The screen clears and a different menu appears:\n\n    0. Maan Menyuu");
    }

    [Test]
    public async Task KeyFourOnTerminal_MultiNoun_OpensGeographyMenu()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        var response = await target.GetResponse("key 4 on the terminal");
        response.Should().Contain(GeographyMenu.MainMenu);
    }

    [Test]
    public async Task PressZeroOnTerminal_MultiNoun_FromMainMenu_HasNoEffect()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        var response = await target.GetResponse("press 0 on the terminal");
        response.Should().Contain(MenuState.NoEffect);
    }

    [Test]
    public async Task PressZeroOnTerminal_MultiNoun_FromSubmenu_GoesUp()
    {
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        await target.GetResponse("type 1 on the keyboard");
        var response = await target.GetResponse("press 0 on the terminal");
        response.Should().Contain(MainMenu.MainMenuText);
    }

    [Test]
    public async Task TypeOnTerminal_MultiNoun_FloydCommentsWhenPresent()
    {
        var target = GetTarget();
        var libraryLobby = GetLocation<LibraryLobby>();
        target.Context.CurrentLocation = libraryLobby;
        GetItem<ComputerTerminal>().IsOn = true;

        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = libraryLobby;
        libraryLobby.ItemPlacedHere(floyd);

        await target.GetResponse("type 1 on the keyboard");

        var pfContext = (PlanetfallContext)target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().NotBeNull();
        pfContext.PendingFloydActionCommentPrompt.Should().Contain("library computer");
    }

    [Test]
    public async Task TypeOnComputer_FloydCommentsWhenPresent()
    {
        var target = GetTarget();
        var libraryLobby = GetLocation<LibraryLobby>();
        target.Context.CurrentLocation = libraryLobby;
        GetItem<ComputerTerminal>().IsOn = true;

        // Set up Floyd as present and active
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = libraryLobby;
        libraryLobby.ItemPlacedHere(floyd);

        await target.GetResponse("type 1");

        // Verify Floyd's pending comment was set
        var pfContext = (PlanetfallContext)target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().NotBeNull();
        pfContext.PendingFloydActionCommentPrompt.Should().Contain("library computer");
    }

    [Test]
    public async Task TypeOnComputer_FloydDoesNotComment_WhenNotPresent()
    {
        var target = GetTarget();
        var libraryLobby = GetLocation<LibraryLobby>();
        target.Context.CurrentLocation = libraryLobby;
        GetItem<ComputerTerminal>().IsOn = true;

        // Floyd is NOT in the room
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<Library>(); // Different location

        await target.GetResponse("type 1");

        // Verify Floyd's pending comment was NOT set
        var pfContext = (PlanetfallContext)target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    [Test]
    public async Task TypeOnComputer_FloydOnlyCommentsOnce()
    {
        var target = GetTarget();
        var libraryLobby = GetLocation<LibraryLobby>();
        target.Context.CurrentLocation = libraryLobby;
        GetItem<ComputerTerminal>().IsOn = true;

        // Set up Floyd as present and active
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = libraryLobby;
        libraryLobby.ItemPlacedHere(floyd);

        // First keypress - should set pending comment
        await target.GetResponse("type 1");
        var pfContext = (PlanetfallContext)target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().NotBeNull();

        // Clear the pending comment (simulating it being processed)
        pfContext.PendingFloydActionCommentPrompt = null;

        // Second keypress - should NOT set pending comment (prompt already used)
        await target.GetResponse("type 2");
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }

    [Test]
    public async Task PressZero_FromLeafNode_AfterSaveAndRestoreRoundTrip_ReturnsSubmenuText()
    {
        // Prove issue #323: after an HTTP round-trip (save→restore), pressing 0 from a leaf node
        // must navigate back to the parent submenu. Before the fix, MenuItem.Parent was internal
        // so Newtonsoft.Json skipped it; the deserialized CurrentItem had Parent=null and GoUp()
        // returned NoEffect instead of the submenu text.
        var target = GetTarget();
        StartHere<LibraryLobby>();
        GetItem<ComputerTerminal>().IsOn = true;

        await target.GetResponse("type one"); // → History submenu
        await target.GetResponse("type 1");   // → leaf: "Raashul Orijinz" text entry

        var saved = target.SaveGame();

        // GetTarget() calls Repository.Reset(), then RestoreGame re-populates from the JSON,
        // mirroring the real HTTP round-trip that loses the Parent back-reference.
        var restored = GetTarget();
        restored.RestoreGame(saved);

        // Pin the serialization contract: Path must survive the round-trip intact.
        // History is representative: all six submenus share the same path mechanism,
        // so one serialization round-trip test covers them all.
        GetItem<ComputerTerminal>().MenuState.Path.Should().Equal(new List<int> { 1, 1 },
            "Path [submenu=1, entry=1] must survive JSON serialization");

        var response = await restored.GetResponse("press 0");

        response.Should().Contain(HistoryMenu.MainMenu,
            "pressing 0 from a leaf should navigate up to the parent submenu, not feep");
        response.Should().NotContain(MenuState.NoEffect);
    }

    [Test]
    public void MenuState_Should_InvalidateCurrentItemCache_When_PathIsDirectlyReassigned()
    {
        // Prove that assigning Path directly (as the JSON deserializer does) invalidates
        // the cached CurrentItem so the new path is reflected on next access.
        var state = new MenuState();
        state.GoDown(1); // navigate into History submenu; warms the cache
        var before = state.CurrentItem;

        state.Path = [2]; // direct reassignment — simulates JSON deserializer

        state.CurrentItem.Should().NotBeSameAs(before,
            "reassigning Path must invalidate the cache so CurrentItem reflects the new path");
    }
}