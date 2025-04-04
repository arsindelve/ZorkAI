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
    public async Task Act_WithLitCandles_InLocation_And_PositiveRoll_BlowsOutCandles()
    {
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(2)).Returns(true); // Always blow out candles

        var context = new ZorkIContext();
        var candles = Repository.GetItem<Candles>();
        candles.IsOn = true;
        context.Items.Add(candles);
        
        var caveSouth = new CaveSouth();
        caveSouth.RandomChooser = mockChooser.Object;
        context.CurrentLocation = caveSouth; // Set current location to CaveSouth

        var result = await caveSouth.Act(context, null!);

        result.Should().Contain("A gust of wind blows out your candles!");
        mockChooser.Verify(c => c.RollDice(2), Times.Once);
    }

    [Test]
    public async Task Act_WithLitCandles_InLocation_And_NegativeRoll_DoesNotBlowOutCandles()
    {
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(2)).Returns(false); // Never blow out candles

        var context = new ZorkIContext();
        var candles = Repository.GetItem<Candles>();
        candles.IsOn = true;
        context.Items.Add(candles);
        
        var caveSouth = new CaveSouth();
        caveSouth.RandomChooser = mockChooser.Object;
        context.CurrentLocation = caveSouth; // Set current location to CaveSouth

        var result = await caveSouth.Act(context, null!);

        result.Should().Be(string.Empty);
        mockChooser.Verify(c => c.RollDice(2), Times.Once);
    }

    [Test]
    public async Task Act_WithLitCandles_NotInLocation_DoesNotBlowOutCandles()
    {
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(2)).Returns(true); // Would blow out candles if in location

        var context = new ZorkIContext();
        var candles = Repository.GetItem<Candles>();
        candles.IsOn = true;
        context.Items.Add(candles);
        
        var caveSouth = new CaveSouth();
        caveSouth.RandomChooser = mockChooser.Object;
        context.CurrentLocation = new WestOfHouse(); // Set current location to somewhere else

        var result = await caveSouth.Act(context, null!);

        result.Should().Be(string.Empty);
        mockChooser.Verify(c => c.RollDice(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task Act_WithoutLitCandles_ReturnsEmptyString()
    {
        var mockChooser = new Mock<IRandomChooser>();
        var context = new ZorkIContext();
        var caveSouth = new CaveSouth();
        caveSouth.RandomChooser = mockChooser.Object;
        context.CurrentLocation = caveSouth; // Set current location to CaveSouth

        var result = await caveSouth.Act(context, null!);

        result.Should().Be(string.Empty);
        mockChooser.Verify(c => c.RollDice(It.IsAny<int>()), Times.Never);
    }
    
    [Test]
    public async Task AfterEnterLocation_RegistersActorAndReturnsExpectedString()
    {
        var mockChooser = new Mock<IRandomChooser>();
        var context = new ZorkIContext();
        var caveSouth = new CaveSouth();
        caveSouth.RandomChooser = mockChooser.Object;
        
        var result = await caveSouth.AfterEnterLocation(context, null!, null!);
        
        context.Actors.Should().Contain(caveSouth);
    }
    
    [Test]
    public void OnLeaveLocation_RemovesActorFromContext()
    {
        var mockChooser = new Mock<IRandomChooser>();
        var context = new ZorkIContext();
        var caveSouth = new CaveSouth();
        caveSouth.RandomChooser = mockChooser.Object;
        context.RegisterActor(caveSouth);
        
        caveSouth.OnLeaveLocation(context, new WestOfHouse(), null!);
        
        context.Actors.Should().NotContain(caveSouth);
    }
}
