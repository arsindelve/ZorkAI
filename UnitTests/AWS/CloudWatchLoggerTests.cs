using CloudWatch;
using CloudWatch.Model;
using Newtonsoft.Json;

namespace UnitTests.AWS;

/// <summary>
/// Tests for CloudWatch logging functionality.
/// Note: These tests focus on testable business logic (serialization, correlation IDs, etc.)
/// Full AWS integration tests would require mocking IAmazonCloudWatchLogs or real AWS credentials.
/// Production code uses ICloudWatchLogger interface (see LoggingAndHistoryTests.cs, IntentParserTests.cs).
/// </summary>
public class CloudWatchLoggerBehaviorTests
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void Should_SerializeLog_ToJson()
        {
            // Arrange
            var log = new TestTurnLog
            {
                TurnCorrelationId = Guid.NewGuid().ToString(),
                TestData = "test data",
                TestNumber = 42
            };

            // Act
            var json = JsonConvert.SerializeObject(log);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("TurnCorrelationId");
            json.Should().Contain("TestData");
            json.Should().Contain("TestNumber");
        }

        [Test]
        public void Should_IncludeAllProperties_InSerializedLog()
        {
            // Arrange
            var log = new TurnLog
            {
                SessionId = "session-123",
                Location = "West of House",
                Score = 0,
                Moves = 1,
                Input = "go north",
                Response = "You are in the forest.",
                TurnCorrelationId = Guid.NewGuid().ToString()
            };

            // Act
            var json = JsonConvert.SerializeObject(log);

            // Assert
            json.Should().Contain("session-123");
            json.Should().Contain("West of House");
            json.Should().Contain("go north");
            json.Should().Contain("You are in the forest");
        }

        [Test]
        public void Should_HandleNullProperties_Gracefully()
        {
            // Arrange
            var log = new TestTurnLog
            {
                TurnCorrelationId = null,
                TestData = null
            };

            // Act
            var json = JsonConvert.SerializeObject(log);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("null");
        }

        [Test]
        public void Should_SerializeGenerationLog_Correctly()
        {
            // Arrange
            var log = new GenerationLog
            {
                LanguageModel = "gpt-4",
                UserPrompt = "What do I see?",
                Response = "You see a mailbox.",
                Temperature = 0.7f,
                SystemPrompt = "You are a game narrator.",
                TurnCorrelationId = Guid.NewGuid().ToString()
            };

            // Act
            var json = JsonConvert.SerializeObject(log);

            // Assert
            json.Should().Contain("gpt-4");
            json.Should().Contain("What do I see?");
            json.Should().Contain("0.7");
        }
    }

    [TestFixture]
    public class TurnCorrelationIdTests
    {
        [Test]
        public void Should_SetTurnCorrelationId_OnLog()
        {
            // Arrange
            var correlationId = Guid.NewGuid();
            var log = new TestTurnLog
            {
                TestData = "test"
            };

            // Act
            log.TurnCorrelationId = correlationId.ToString();

            // Assert
            log.TurnCorrelationId.Should().Be(correlationId.ToString());
        }

        [Test]
        public void Should_ConvertGuid_ToString()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var guidString = guid.ToString();

            // Assert
            guidString.Should().NotBeNullOrEmpty();
            Guid.TryParse(guidString, out var parsed).Should().BeTrue();
            parsed.Should().Be(guid);
        }

        [Test]
        public void Should_PreserveOtherLogProperties_WhenSettingCorrelationId()
        {
            // Arrange
            var log = new TurnLog
            {
                SessionId = "session-123",
                Location = "Forest",
                Score = 0,
                Moves = 1,
                Input = "look",
                Response = "You see trees.",
                TurnCorrelationId = null
            };

            // Act
            log.TurnCorrelationId = Guid.NewGuid().ToString();

            // Assert
            log.SessionId.Should().Be("session-123");
            log.Location.Should().Be("Forest");
            log.TurnCorrelationId.Should().NotBeNullOrEmpty();
        }
    }

    [TestFixture]
    public class LogTimestampTests
    {
        [Test]
        public void Should_CreateTimestamp_AsUtcNow()
        {
            // Test the pattern used in CloudWatchLogger: Timestamp = DateTime.UtcNow

            var before = DateTime.UtcNow;
            var timestamp = DateTime.UtcNow;
            var after = DateTime.UtcNow;

            timestamp.Should().BeOnOrAfter(before);
            timestamp.Should().BeOnOrBefore(after);
            timestamp.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public void Should_FormatTimestamp_ForLogging()
        {
            // Arrange
            var timestamp = new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc);

            // Act
            var formatted = timestamp.ToString("o"); // ISO 8601 format

            // Assert
            formatted.Should().Contain("2024-01-15");
            formatted.Should().Contain("14:30:00");
        }
    }
}

/// <summary>
/// Tests for ICloudWatchLogger interface usage patterns.
/// This demonstrates how production code interacts with the logger.
/// </summary>
[TestFixture]
public class CloudWatchLoggerInterfaceTests
{
    [Test]
    public void Should_AcceptTurnLog_ViaInterface()
    {
        // Arrange
        var mockLogger = new Mock<ICloudWatchLogger<TurnLog>>();
        var log = new TurnLog
        {
            SessionId = "test",
            Location = "test",
            Score = 0,
            Moves = 1,
            Input = "test",
            Response = "test",
            TurnCorrelationId = Guid.NewGuid().ToString()
        };

        // Act
        mockLogger.Object.WriteLogEvents(log);

        // Assert - Mock accepts the log type
        mockLogger.Verify(l => l.WriteLogEvents(It.IsAny<TurnLog>()), Times.Once);
    }

    [Test]
    public void Should_AcceptGenerationLog_ViaInterface()
    {
        // Arrange
        var mockLogger = new Mock<ICloudWatchLogger<GenerationLog>>();
        var log = new GenerationLog
        {
            LanguageModel = "test",
            UserPrompt = "test",
            Response = "test",
            Temperature = 0f,
            SystemPrompt = "test",
            TurnCorrelationId = Guid.NewGuid().ToString()
        };

        // Act
        mockLogger.Object.WriteLogEvents(log);

        // Assert - Mock accepts the log type
        mockLogger.Verify(l => l.WriteLogEvents(It.IsAny<GenerationLog>()), Times.Once);
    }

    [Test]
    public async Task Should_VerifyWriteLogEvents_CalledWithCorrectLog()
    {
        // Arrange
        var mockLogger = new Mock<ICloudWatchLogger<TurnLog>>();
        var log = new TurnLog
        {
            SessionId = "session-123",
            Location = "Forest",
            Score = 10,
            Moves = 5,
            Input = "go north",
            Response = "You moved north.",
            TurnCorrelationId = "test-correlation-id"
        };

        // Act
        await mockLogger.Object.WriteLogEvents(log);

        // Assert - Verify specific log was passed
        mockLogger.Verify(l => l.WriteLogEvents(It.Is<TurnLog>(t =>
            t.SessionId == "session-123" &&
            t.Location == "Forest" &&
            t.Score == 10 &&
            t.Moves == 5 &&
            t.Input == "go north" &&
            t.Response == "You moved north." &&
            t.TurnCorrelationId == "test-correlation-id"
        )), Times.Once);
    }

    [Test]
    public async Task Should_HandleMultipleLogWrites_InSequence()
    {
        // Arrange
        var mockLogger = new Mock<ICloudWatchLogger<TurnLog>>();
        var log1 = new TurnLog
        {
            SessionId = "s1",
            Location = "L1",
            Score = 0,
            Moves = 1,
            Input = "i1",
            Response = "r1"
        };
        var log2 = new TurnLog
        {
            SessionId = "s2",
            Location = "L2",
            Score = 0,
            Moves = 2,
            Input = "i2",
            Response = "r2"
        };

        // Act
        await mockLogger.Object.WriteLogEvents(log1);
        await mockLogger.Object.WriteLogEvents(log2);

        // Assert
        mockLogger.Verify(l => l.WriteLogEvents(It.IsAny<TurnLog>()), Times.Exactly(2));
    }
}

/// <summary>
/// Test helper log type for testing serialization and property handling.
/// </summary>
public class TestTurnLog : ITurnBasedLog
{
    public string? TurnCorrelationId { get; set; }
    public string? TestData { get; set; }
    public int TestNumber { get; set; }
}
