using FluentAssertions;
using GameEngine;
using Model.AIParsing;
using Moq;
using NUnit.Framework;
using UnitTests;
using ZorkOne.GlobalCommand;

namespace ZorkOne.Tests;

[TestFixture]
public class GlobalCommandTests : EngineTestsBase
{
    [Test]
    public async Task Xyzzy()
    {
        var target = GetTarget(new IntentParser(Mock.Of<IAIParser>(), new ZorkOneGlobalCommandFactory()));
        var response = await target.GetResponse("xyzzy");
        response.Should().Contain("fool");
    }

    [Test]
    public async Task Plugh()
    {
        var target = GetTarget(new IntentParser(Mock.Of<IAIParser>(), new ZorkOneGlobalCommandFactory()));
        var response = await target.GetResponse("Plugh");
        response.Should().Contain("fool");
    }

    [Test]
    public async Task Ulysses()
    {
        var target = GetTarget(new IntentParser(Mock.Of<IAIParser>(), new ZorkOneGlobalCommandFactory()));
        var response = await target.GetResponse("Ulysses");
        response.Should().Contain("sailor");
    }

    [Test]
    public async Task Odysseus()
    {
        var target = GetTarget(new IntentParser(Mock.Of<IAIParser>(), new ZorkOneGlobalCommandFactory()));
        var response = await target.GetResponse("Odysseus");
        response.Should().Contain("sailor");
    }
}