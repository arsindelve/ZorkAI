using FluentAssertions;
using GameEngine;
using Stationfall.Item.Duffy;

namespace Stationfall.Tests;

/// <summary>
///     The three robots as characters rather than as a keypad choice: what they say, what they let you
///     do, and how the companion behaves while he is campaigning for the job.
/// </summary>
[TestFixture]
public class DuffyRobotTests : EngineTestsBase
{
    private GameEngine<StationfallGame, StationfallContext> _target = null!;

    [SetUp]
    public void SetUp()
    {
        _target = GetTarget();
    }

    private async Task GoToRobotPool()
    {
        await _target.GetResponse("east");
        await _target.GetResponse("north");
    }

    private async Task SelectFloyd()
    {
        await GoToRobotPool();
        await _target.GetResponse("put authorization in slot");
        await _target.GetResponse("type 3");
    }

    [TestFixture]
    public class TheirVoices : DuffyRobotTests
    {
        [Test]
        public async Task EachRobotGreetsYouInItsOwnRegister()
        {
            await GoToRobotPool();

            (await _target.GetResponse("floyd, hello")).Should().Contain("bounces");
            (await _target.GetResponse("rex, hello")).Should().Contain("Hey");
            (await _target.GetResponse("helen, hello")).Should().Contain("Likewise");
        }

        [Test]
        public async Task RexWillNotFollowSomeoneHeIsNotAssignedTo()
        {
            await GoToRobotPool();

            (await _target.GetResponse("rex, follow me")).Should().Contain("Ain't been assigned");
        }

        [Test]
        public async Task HelenIsInterestedInExactlyOneSubject()
        {
            await GoToRobotPool();

            (await _target.GetResponse("helen, what do you think of the ship")).Should()
                .Contain("sorting forms");
        }
    }

    [TestFixture]
    public class ChoosingOne : DuffyRobotTests
    {
        [Test]
        public async Task AskingForARobotDirectly_PointsYouAtTheEquipment()
        {
            await GoToRobotPool();

            (await _target.GetResponse("pick floyd")).Should().Contain("automated robot selection");
        }

        [Test]
        public async Task OnceYouHaveChosen_TheOthersAreOutOfReach()
        {
            await SelectFloyd();

            (await _target.GetResponse("take rex")).Should().Contain("can't reach");
        }

        [Test]
        public async Task EachRobotCanStillBeExaminedByName()
        {
            // Floyd gave up the bare noun "robot" to stop the conversation handler hijacking commands
            // that merely contain the word (see TheRequiredFormCommand_IsNotHijackedIntoAConversation).
            // Naming any of them must still work.
            await GoToRobotPool();

            (await _target.GetResponse("examine floyd")).Should().Contain("Floyd");
            (await _target.GetResponse("examine rex")).Should().Contain("Rex");
            (await _target.GetResponse("examine helen")).Should().Contain("Helen");
        }

        [Test]
        public async Task TheRequiredFormCommand_IsNotHijackedIntoAConversation()
        {
            await GoToRobotPool();

            var response = await _target.GetResponse("put robot use authorization form in slot");

            response.Should().Contain("Authorization approved",
                "the command names a robot noun, but it is a command, not a conversation");
        }
    }

    [TestFixture]
    public class TheCompanion : DuffyRobotTests
    {
        [Test]
        public async Task IsRecognisedTheFirstTimeYouSeeHim()
        {
            var response = await _target.GetResponse("east");
            response.Should().NotContain("Resida");

            response = await _target.GetResponse("north");

            response.Should().Contain("Resida", "the reunion is the point of the room");
            Repository.GetItem<Floyd>().HasBeenSeen.Should().BeTrue();
        }

        [Test]
        public async Task IsOnlyRecognisedOnce()
        {
            await GoToRobotPool();

            (await _target.GetResponse("wait")).Should().NotContain("Resida");
        }

        [Test]
        public async Task CampaignsForTheJobWhileNobodyHasBeenChosen()
        {
            await GoToRobotPool();

            (await _target.GetResponse("wait")).Should().Contain("pick Floyd");
        }

        [Test]
        public async Task LooksDejected_IfYouChooseSomeoneElse()
        {
            await GoToRobotPool();
            await _target.GetResponse("put authorization in slot");
            await _target.GetResponse("type 1");

            (await _target.GetResponse("examine floyd")).Should().Contain("looks at his feet");
        }

        [Test]
        public async Task StopsCampaigningOnceYouHaveLeftTheRoom()
        {
            await GoToRobotPool();
            await _target.GetResponse("wait");

            await _target.GetResponse("south");

            // He is still in his bin; he must not go on begging from an empty room.
            (await _target.GetResponse("wait")).Should().NotContain("pick Floyd");
        }
    }
}
