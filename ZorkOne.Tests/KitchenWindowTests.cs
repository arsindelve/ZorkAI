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
}
