using FluentAssertions;
using GameEngine;
using NUnit.Framework;
using UnitTests;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests;

[TestFixture]
public class LoudRoomTests : EngineTestsBase
{
    [Test]
    [TestCase("W")]
    [TestCase("walk w")]
    [TestCase("go west")]
    [TestCase("head 'west'")]
    [TestCase("move \"west\"")]
    public async Task CanLeave(string command)
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        string? response = await target.GetResponse(command);
        Console.WriteLine(response);

        response.Should().Contain("Round Room");
    }
    
    [Test]
    public async Task EchosLastWordsByDefault()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();

        string? response = await target.GetResponse("hi");
        Console.WriteLine(response);

        response.Should().Contain("hi hi ...");
    }
    
    [Test]
    public async Task CanStillQuit()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();

        string? response = await target.GetResponse("quit");
        Console.WriteLine(response);

        response.Should().Contain("Do you wish");
    }
    
    [Test]
    public async Task EchosLastWorkByDefault_MultiPhrase()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();

        string? response = await target.GetResponse("hi there buddy");
        Console.WriteLine(response);

        response.Should().Contain("buddy buddy ...");
    }
    
    [Test]
    [TestCase("echo")]
    [TestCase("\"echo\"")]
    [TestCase("'echo'")]
    [TestCase("say the word 'echo'")]
    [TestCase("shout echo")]
    [TestCase("yell the word \"echo\"")]
    [TestCase("scream 'echo'")]
    public async Task SimplestSolution(string input)
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();

        string? response = await target.GetResponse(input);
        Console.WriteLine(response);

        response.Should().Contain("The acoustics of the room");
        Repository.GetLocation<LoudRoom>().EchoHasBeenSpoken.Should().BeTrue();
    }
}