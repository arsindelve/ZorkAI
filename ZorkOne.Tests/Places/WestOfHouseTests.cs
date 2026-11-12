using FluentAssertions;
using GameEngine;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

public class WestOfHouseTests : EngineTestsBase
{
    // Verbs.OpenItemVerbs: every way of trying to get a shut thing open gets the same refusal.
    [TestCase("open door")]
    [TestCase("pry door")]
    [TestCase("force door")]
    [TestCase("unseal door")]
    public async Task TheBoardedFrontDoor_CannotBeOpened_HoweverItIsTried(string input)
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();

        var response = await target.GetResponse(input);

        response.Should().Contain("The door cannot be opened");
    }

    // The door by its full name, as a player types it, through the production grammar.
    [TestCase("open the front door")]
    [TestCase("pry the front door")]
    public async Task TheBoardedFrontDoor_ByItsFullName(string input)
    {
        var target = GetTarget(Walkthrough.DeterministicFirst.Parser());
        target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();

        var response = await target.GetResponse(input);

        response.Should().Contain("The door cannot be opened");
    }
}
