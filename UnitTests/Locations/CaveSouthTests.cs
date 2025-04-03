using FluentAssertions;
using GameEngine;
using Model.Intent;
using Model.Interface;
using Moq;
using NUnit.Framework;
using ZorkOne;
using ZorkOne.Item;
using ZorkOne.Location;

namespace UnitTests.Locations;

public class CaveSouthTests
{
    [Test]
    public async Task Act_WithLitCandles_And_PositiveRoll_BlowsOutCandles()
    {
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(2)).Returns(true); // 50% chance returns true

        var context = new ZorkIContext();
        var candles = Repository.GetItem<Candles>();
        candles.IsOn = true;
        context.Items.Add(candles);

        var caveSouth = new CaveSouth(mockChooser.Object);

        var result = await caveSouth.Act(context, null!);

        result.Should().Contain("A gust of wind blows out your candles!");
        mockChooser.Verify(c => c.RollDice(2), Times.Once);
    }

    [Test]
    public async Task Act_WithLitCandles_And_NegativeRoll_DoesNotBlowOutCandles()
    {
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(2)).Returns(false); // 50% chance returns false

        var context = new ZorkIContext();
        var candles = Repository.GetItem<Candles>();
        candles.IsOn = true;
        context.Items.Add(candles);

        var caveSouth = new CaveSouth(mockChooser.Object);

        var result = await caveSouth.Act(context, null!);

        result.Should().Be(string.Empty);
        mockChooser.Verify(c => c.RollDice(2), Times.Once);
    }

    [Test]
    public async Task Act_WithoutLitCandles_ReturnsEmptyString()
    {
        var mockChooser = new Mock<IRandomChooser>();
        var context = new ZorkIContext();
        var caveSouth = new CaveSouth(mockChooser.Object);

        var result = await caveSouth.Act(context, null!);

        result.Should().Be(string.Empty);
        mockChooser.Verify(c => c.RollDice(It.IsAny<int>()), Times.Never);
    }
}
