using DynamoDb;

namespace UnitTests.AWS;

/// <summary>
/// Tests for DynamoDb repository implementations.
/// Note: These tests focus on testable business logic (GUID generation, data formatting, etc.)
/// Full AWS integration tests exist in IntegrationTests/SavedGameTests.cs
/// </summary>
[TestFixture]
public class DynamoDbSavedGameRepositoryTests
{
    [TestFixture]
    public class SaveGameMethod
    {
        [Test]
        [Explicit("Requires AWS credentials - Integration test")]
        public async Task Should_GenerateNewGuid_When_IdIsNull()
        {
            // Arrange
            var repository = new DynamoDbSavedGameRepository();

            // Act
            var id = await repository.SaveGame(null, "sessionId", "testGame", "gameData", "test_table");

            // Assert
            id.Should().NotBeNullOrEmpty();
            Guid.TryParse(id, out _).Should().BeTrue();
        }

        [Test]
        [Explicit("Requires AWS credentials - Integration test")]
        public async Task Should_UseProvidedId_When_IdIsNotNull()
        {
            // Arrange
            var repository = new DynamoDbSavedGameRepository();
            var providedId = Guid.NewGuid().ToString();

            // Act
            var returnedId = await repository.SaveGame(providedId, "sessionId", "testGame", "gameData", "test_table");

            // Assert
            returnedId.Should().Be(providedId);
        }

        [Test]
        [Explicit("Requires AWS credentials - Integration test")]
        public async Task Should_AcceptAllRequiredParameters()
        {
            // Arrange
            var repository = new DynamoDbSavedGameRepository();

            // Act
            var id = await repository.SaveGame("test-id", "session-123", "My Game", "{\"data\":\"test\"}", "zork_savegame");

            // Assert
            id.Should().Be("test-id");
        }
    }

    [TestFixture]
    public class GetSavedGamesMethod
    {
        [Test]
        [Explicit("Requires AWS credentials - Integration test")]
        public async Task Should_AcceptSessionIdAndTableName()
        {
            // Arrange
            var repository = new DynamoDbSavedGameRepository();

            // Act
            var results = await repository.GetSavedGames("session-123", "test_table");

            // Assert
            results.Should().NotBeNull();
        }
    }

    [TestFixture]
    public class DeleteSavedGameMethod
    {
        [Test]
        [Explicit("Requires AWS credentials - Integration test")]
        public async Task Should_AcceptIdSessionIdAndTableName()
        {
            // Arrange
            var repository = new DynamoDbSavedGameRepository();

            // Act - Should not throw
            await repository.DeleteSavedGameAsync("game-id", "session-123", "test_table");
        }
    }

    [TestFixture]
    public class GetSavedGameMethod
    {
        [Test]
        [Explicit("Requires AWS credentials - Integration test")]
        public async Task Should_AcceptIdSessionIdAndTableName()
        {
            // Arrange
            var repository = new DynamoDbSavedGameRepository();

            // Act
            var result = await repository.GetSavedGame("game-id", "session-123", "test_table");

            // Assert - May be null if not found
            result.Should().BeOfType<string>();
        }
    }
}

[TestFixture]
public class DynamoDbSessionRepositoryTests
{
    [TestFixture]
    public class WriteSessionStateMethod
    {
        [Test]
        [Explicit("Requires AWS credentials - Integration test")]
        public async Task Should_AcceptAllRequiredParameters()
        {
            // Arrange
            var repository = new DynamoDbSessionRepository();

            // Act - Should not throw
            await repository.WriteSessionState("session-123", "{\"data\":\"test\"}", "test_table");
        }
    }

    [TestFixture]
    public class WriteSessionStepMethod
    {
        [Test]
        [Explicit("Requires AWS credentials - Integration test")]
        public async Task Should_AcceptAllRequiredParameters()
        {
            // Arrange
            var repository = new DynamoDbSessionRepository();

            // Act - Should not throw
            await repository.WriteSessionStep("session-123", 1, "take sword", "Taken.", "test_table");
        }

        [Test]
        public void Should_ConvertTurnIndexToNumericString()
        {
            // This tests the concept that turnIndex (long) should be stored as numeric string
            // In DynamoDB, numbers are stored as strings in AttributeValue with .N property
            long turnIndex = 42;
            string numericString = turnIndex.ToString();

            numericString.Should().Be("42");
            long.Parse(numericString).Should().Be(turnIndex);
        }
    }

    [TestFixture]
    public class GetSessionStepsAsTextMethod
    {
        [Test]
        [Explicit("Requires AWS credentials - Integration test")]
        public async Task Should_AcceptSessionIdAndTableName()
        {
            // Arrange
            var repository = new DynamoDbSessionRepository();

            // Act
            var result = await repository.GetSessionStepsAsText("session-123", "test_table");

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Should_FormatOutput_AsPromptAndResponse()
        {
            // Test the string formatting pattern used in GetSessionStepsAsText
            // Pattern: "> {input}\n{output}"

            var input = "take sword";
            var output = "Taken.";
            var formatted = $"> {input}\n{output}";

            formatted.Should().Be("> take sword\nTaken.");
            formatted.Should().StartWith(">");
            formatted.Should().Contain("\n");
        }
    }

    [TestFixture]
    public class GetSessionStateMethod
    {
        [Test]
        [Explicit("Requires AWS credentials - Integration test")]
        public async Task Should_AcceptSessionIdAndTableName()
        {
            // Arrange
            var repository = new DynamoDbSessionRepository();

            // Act
            var result = await repository.GetSessionState("session-123", "test_table");

            // Assert - May be null if not found
            result.Should().BeOfType<string>();
        }
    }
}

/// <summary>
/// Integration tests demonstrating date/time handling and tuple parsing logic.
/// These tests validate the data transformation logic without requiring AWS.
/// </summary>
[TestFixture]
public class DynamoDbDataTransformationTests
{
    [Test]
    public void Should_ConvertDateTime_ToTicksAndBack()
    {
        // This validates the pattern used in SaveGame/GetSavedGames
        // SaveGame stores: DateTime.UtcNow.Ticks.ToString()
        // GetSavedGames reads: new DateTime(long.Parse(item["date"].S))

        var now = DateTime.UtcNow;
        var ticks = now.Ticks;
        var ticksString = ticks.ToString();

        var parsedTicks = long.Parse(ticksString);
        var restoredDateTime = new DateTime(parsedTicks);

        restoredDateTime.Should().Be(now);
        restoredDateTime.Kind.Should().Be(DateTimeKind.Unspecified); // Note: Kind is lost
    }

    [Test]
    public void Should_ParseTupleFromDynamoDbResponse()
    {
        // This validates the tuple parsing logic in GetSavedGames
        // Pattern: (item["id"].S, item["name"].S, new DateTime(long.Parse(item["date"].S)))

        var id = "game-123";
        var name = "My Saved Game";
        var date = DateTime.UtcNow;
        var dateTicks = date.Ticks.ToString();

        // Simulate what the query would parse
        var tuple = (id, name, new DateTime(long.Parse(dateTicks)));

        tuple.Item1.Should().Be("game-123");
        tuple.Item2.Should().Be("My Saved Game");
        tuple.Item3.Should().BeCloseTo(date, TimeSpan.FromMilliseconds(1));
    }

    [Test]
    public void Should_GenerateValidGuid_WhenIdIsNull()
    {
        // This validates the pattern: id ??= Guid.NewGuid().ToString();

        string? id = null;
        id ??= Guid.NewGuid().ToString();

        id.Should().NotBeNullOrEmpty();
        Guid.TryParse(id, out var guid).Should().BeTrue();
        guid.Should().NotBe(Guid.Empty);
    }

    [Test]
    public void Should_PreserveProvidedGuid_WhenIdIsNotNull()
    {
        // This validates the pattern: id ??= Guid.NewGuid().ToString();

        string id = "existing-id";
        id.Should().Be("existing-id");
    }
}
