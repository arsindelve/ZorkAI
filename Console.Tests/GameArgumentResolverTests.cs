using ZorkConsole;

namespace ZorkConsole.Tests;

[TestFixture]
public class GameArgumentResolverTests
{
    [TestFixture]
    public class WhenNoGameIsSupplied
    {
        [Test]
        public void EmptyArgs_IsNotValid()
        {
            var result = GameArgumentResolver.Resolve([]);

            result.IsValid.Should().BeFalse();
            result.GameName.Should().BeNull();
        }

        [Test]
        public void NullArgs_IsNotValid()
        {
            var result = GameArgumentResolver.Resolve(null);

            result.IsValid.Should().BeFalse();
        }

        [TestCase("")]
        [TestCase("   ")]
        public void BlankFirstArg_IsNotValid(string blank)
        {
            var result = GameArgumentResolver.Resolve([blank]);

            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void Feedback_TellsUserAGameIsRequired_AndListsTheSupportedGames()
        {
            var result = GameArgumentResolver.Resolve([]);

            result.Feedback.Should().NotBeNullOrWhiteSpace();
            result.Feedback.Should().Contain("game");
            result.Feedback.Should().Contain("ZorkOne");
            result.Feedback.Should().Contain("Planetfall");
            result.Feedback.Should().Contain("Stationfall");
            result.Feedback.Should().Contain("EscapeRoom");
        }
    }

    [TestFixture]
    public class WhenAnUnknownGameIsSupplied
    {
        [Test]
        public void IsNotValid()
        {
            var result = GameArgumentResolver.Resolve(["Frobozz"]);

            result.IsValid.Should().BeFalse();
            result.GameName.Should().BeNull();
        }

        [Test]
        public void Feedback_EchoesTheUnknownGame_AndListsTheSupportedGames()
        {
            var result = GameArgumentResolver.Resolve(["Frobozz"]);

            result.Feedback.Should().Contain("Frobozz");
            result.Feedback.Should().Contain("Planetfall");
        }
    }

    [TestFixture]
    public class WhenAValidGameIsSupplied
    {
        [TestCase("ZorkOne")]
        [TestCase("Planetfall")]
        [TestCase("Stationfall")]
        [TestCase("EscapeRoom")]
        public void IsValid_AndReturnsTheCanonicalName(string game)
        {
            var result = GameArgumentResolver.Resolve([game]);

            result.IsValid.Should().BeTrue();
            result.GameName.Should().Be(game);
            result.Feedback.Should().BeNull();
        }

        [TestCase("planetfall", "Planetfall")]
        [TestCase("stationfall", "Stationfall")]
        [TestCase("ZORKONE", "ZorkOne")]
        [TestCase("  escaperoom  ", "EscapeRoom")]
        public void MatchIsCaseInsensitiveAndTrimmed_ReturningTheCanonicalName(string supplied, string canonical)
        {
            var result = GameArgumentResolver.Resolve([supplied]);

            result.IsValid.Should().BeTrue();
            result.GameName.Should().Be(canonical);
        }

        [Test]
        public void ExtraArgumentsAreIgnored()
        {
            var result = GameArgumentResolver.Resolve(["Planetfall", "somethingElse"]);

            result.IsValid.Should().BeTrue();
            result.GameName.Should().Be("Planetfall");
        }
    }
}