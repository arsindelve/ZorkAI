using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests;

public class KitchenWindowTests : EngineTestsBase
{
    [TestFixture]
    public class ThroughWindowCommands_WindowClosed : EngineTestsBase
    {
        [Test]
        public async Task LookThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("look through window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task LookThroughTheWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("look through the window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task SqueezeThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("squeeze through window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task GoThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("go through window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task JumpThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("jump through window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task CrawlThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("crawl through window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task ClimbThroughTheWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("climb through the window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task PeerThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("peer through window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task WalkThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("walk through window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task MoveThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("move through window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task EnterThroughTheWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("enter through the window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }
    }

    [TestFixture]
    public class ThroughWindowCommands_WindowOpen : EngineTestsBase
    {
        [Test]
        public async Task LookThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("look through window");

            response.Should().Contain("The window is open. If you want to enter the house, just say so.");
        }

        [Test]
        public async Task GoThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("go through window");

            response.Should().Contain("The window is open. If you want to enter the house, just say so.");
        }

        [Test]
        public async Task SqueezeThroughTheWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("squeeze through the window");

            response.Should().Contain("The window is open. If you want to enter the house, just say so.");
        }

        [Test]
        public async Task JumpThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("jump through window");

            response.Should().Contain("The window is open. If you want to enter the house, just say so.");
        }

        [Test]
        public async Task ClimbThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("climb through window");

            response.Should().Contain("The window is open. If you want to enter the house, just say so.");
        }

        [Test]
        public async Task WalkThroughTheWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("walk through the window");

            response.Should().Contain("The window is open. If you want to enter the house, just say so.");
        }

        [Test]
        public async Task EnterThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("enter through window");

            response.Should().Contain("The window is open. If you want to enter the house, just say so.");
        }

        [Test]
        public async Task CrawlThroughWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("crawl through window");

            response.Should().Contain("The window is open. If you want to enter the house, just say so.");
        }
    }

    [TestFixture]
    public class NormalWindowInteractions : EngineTestsBase
    {
        [Test]
        public async Task ExamineWindow_Closed()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("examine window");

            response.Should().Contain("The window is slightly ajar, but not enough to allow entry");
            response.Should().NotContain("permit entry");
        }

        [Test]
        public async Task ExamineWindow_Open()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("examine window");

            response.Should().Contain("The kitchen window is open");
        }

        [Test]
        public async Task OpenWindow_FirstTime()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("open window");

            response.Should().Contain("With great effort, you open the window far enough to allow entry.");
            Repository.GetItem<KitchenWindow>().IsOpen.Should().BeTrue();
        }

        [Test]
        public async Task CloseWindow()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("close window");

            response.Should().Contain("The window closes (more easily than it opened).");
            Repository.GetItem<KitchenWindow>().IsOpen.Should().BeFalse();
        }
    }

    [TestFixture]
    public class EdgeCases : EngineTestsBase
    {
        [Test]
        public async Task ThroughWindowCommand_FromDifferentLocation_DoesNotTrigger()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

            var response = await target.GetResponse("look through window");

            // Should not trigger the special BehindHouse message
            response.Should().NotContain("permit entry");
            response.Should().NotContain("just say so");
        }

        [Test]
        public async Task ThroughWindowCommand_CaseInsensitive()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("LOOK THROUGH WINDOW");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task ThroughWindowCommand_WithExtraWords()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("try to look through the small window");

            response.Should().Contain("The window is slightly ajar, but not enough to permit entry.");
        }

        [Test]
        public async Task GoIn_ActuallyEnters_WhenWindowOpen()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("go in");

            target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
        }

        [Test]
        public async Task GoWest_ActuallyEnters_WhenWindowOpen()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            var response = await target.GetResponse("west");

            target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
        }

        [Test]
        public async Task GoIn_Blocked_WhenWindowClosed()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("go in");

            response.Should().Contain("The kitchen window is closed.");
            target.Context.CurrentLocation.Should().BeOfType<BehindHouse>();
        }
    }

    /// <summary>
    /// issue #262: "enter window" / "board window" name a real door (the kitchen window), not an
    /// ISubLocation. The window is declared as the GatingItem of Behind House's passage to the
    /// Kitchen, so they resolve the noun to it and walk its direction — the window's open-check and
    /// "closed" failure message apply — instead of a generic refusal the narrator turns into a mock.
    /// </summary>
    [TestFixture]
    public class EnterWindowDefersToMovement : EngineTestsBase
    {
        [Test]
        public async Task EnterWindow_ActuallyEnters_WhenWindowOpen()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            await target.GetResponse("enter window");

            target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
        }

        [Test]
        public async Task BoardWindow_ActuallyEnters_WhenWindowOpen()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            await target.GetResponse("board window");

            target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
        }

        [Test]
        public async Task EnterWindow_Blocked_WhenWindowClosed()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("enter window");

            response.Should().Contain("The kitchen window is closed.");
            target.Context.CurrentLocation.Should().BeOfType<BehindHouse>();
        }

        [Test]
        public async Task BoardWindow_Blocked_WhenWindowClosed()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

            var response = await target.GetResponse("board window");

            response.Should().Contain("The kitchen window is closed.");
            target.Context.CurrentLocation.Should().BeOfType<BehindHouse>();
        }

        [Test]
        public async Task ExitWindow_FromKitchen_GoesToBehindHouse_WhenWindowOpen()
        {
            // Symmetric to "enter window": from the Kitchen side, "exit window" means "go out through
            // it" -> Move(Out). The Kitchen map already exposes the window passage as Direction.Out.
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
            Repository.GetItem<KitchenWindow>().IsOpen = true;

            await target.GetResponse("exit window");

            target.Context.CurrentLocation.Should().BeOfType<BehindHouse>();
        }

        [Test]
        public async Task ExitWindow_FromKitchen_Blocked_WhenWindowClosed()
        {
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

            var response = await target.GetResponse("exit window");

            response.Should().Contain("The kitchen window is closed.");
            target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
        }

        [Test]
        public async Task EnterCarriedSack_DoesNotTeleportThroughTheWindow_EvenWhenOpen()
        {
            // issue #262 review follow-up: GetItemInScope also searches inventory, so "enter sack"
            // while carrying the (openable) brown sack used to resolve the sack and hijack the window's
            // exit. Under the GatingItem model the sack is nobody's gating item, so DirectionGatedBy
            // finds no exit for it and the player can't teleport through the window with a sack.
            var target = GetTarget();
            target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();
            Repository.GetItem<KitchenWindow>().IsOpen = true; // the dangerous case
            target.Context.Take(Repository.GetItem<BrownSack>());

            var response = await target.GetResponse("enter sack");

            target.Context.CurrentLocation.Should().BeOfType<BehindHouse>();
            response.Should().Contain("You can't enter that.");
        }
    }
}
