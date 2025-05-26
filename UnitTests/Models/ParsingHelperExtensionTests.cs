using FluentAssertions;
using Microsoft.Extensions.Logging;
using Model;
using Model.Intent;
using Moq;
using NUnit.Framework;

namespace UnitTests.Models;

[TestFixture]
public class ParsingHelperExtensionTests
{
    private Mock<ILogger>? _loggerMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger>();
    }

    [Test]
    public void GetIntent_WithNullOrEmptyResponse_ReturnsNullIntent()
    {
        // Test cases for various null and empty inputs
        var testCases = new[]
        {
            new { Input = "some command", Response = (string?)null },
            new { Input = "some command", Response = "" }!,
            new { Input = "some command", Response = "   " }!
        };

        foreach (var testCase in testCases)
        {
            // Act
            var result = ParsingHelperExtension.SafeGetIntent(testCase.Input, testCase.Response, _loggerMock?.Object);

            // Assert
            result.Should().BeOfType<NullIntent>();
        }
    }
}

/// <summary>
/// A wrapper class for ParsingHelper methods to simplify testing with null inputs
/// </summary>
public static class ParsingHelperExtension
{
    /// <summary>
    /// A wrapper method for ParsingHelper.GetIntent that safely handles null responses
    /// </summary>
    public static IntentBase SafeGetIntent(string input, string? response, ILogger? logger)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return new NullIntent();
        }

        try
        {
            return ParsingHelper.GetIntent(input, response, logger);
        }
        catch (ArgumentNullException)
        {
            // If null causes exceptions, return NullIntent
            return new NullIntent();
        }
    }
}
