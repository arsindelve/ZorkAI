using FluentAssertions;
using GameEngine;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class NorthSouthPassageTests
{
    [SetUp]
    public void SetUp()
    {
        Repository.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        Repository.Reset();
    }

    [Test]
    public void Name_ShouldNotIncludeTrailingWhitespace()
    {
        var location = Repository.GetLocation<NorthSouthPassage>();

        location.Name.Should().Be("North-South Passage");
        location.Name.Should().Be(location.Name.TrimEnd());
    }
}
