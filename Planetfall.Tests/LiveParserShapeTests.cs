using FluentAssertions;
using GameEngine;
using Model.Interface;
using Moq;
using OpenAI.Chat;
using Planetfall.GlobalCommand;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Computer;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Lawanda;
using ZorkAI.OpenAI;

namespace Planetfall.Tests;

/// <summary>
///     Issue #538. Every walkthrough suite in this repo drives the engine through
///     <c>UnitTests.TestParser</c>, which resolves an intent straight out of
///     <c>UnitTests/IntentMappings/base-mappings.json</c>. That proves the HANDLERS work but says
///     nothing about whether the real parser ever reaches them — which is exactly where the bug
///     lived: in production these commands intermittently produced no engine response at all, and
///     the player got invented narrator prose instead of the action they asked for.
///     <para>
///         These tests close that blind spot for the specific shapes involved. They run the genuine
///         <see cref="ParsingHelper" /> (via <see cref="OpenAIParser" />) over the raw tagged
///         completions gpt-4o actually emits — including the degraded shapes that caused the
///         dropped turns — and assert the command still reaches its handler.
///     </para>
/// </summary>
public class LiveParserShapeTests : EngineTestsBase
{
    /// <summary>
    ///     Builds a real <see cref="IntentParser" /> whose AI layer is stubbed to return a fixed
    ///     tagged completion. Everything downstream of the model — tag extraction, intent-shape
    ///     classification, engine dispatch — is the production code path.
    /// </summary>
    private static IIntentParser ParserReturning(string taggedCompletion)
    {
        var completion = new Mock<IChatCompletionClient>();
        completion
            .Setup(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>()))
            .ReturnsAsync(taggedCompletion);

        return new IntentParser(new OpenAIParser(null, completion.Object), new PlanetfallGlobalCommandFactory());
    }

    [Test]
    public async Task SetDialToTheCode_WhenTheParserOmitsThePreposition_StillOpensTheConferenceRoomDoor()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>act</intent>
                                               <verb>set</verb>
                                               <noun>dial</noun>
                                               <noun>419</noun>
                                               """));
        StartHere<RecArea>();
        GetItem<ConferenceRoomDoor>().UnlockCode = "419";

        var response = await target.GetResponse("set dial to 419");

        response.Should().Contain("The door swings open");
        GetItem<ConferenceRoomDoor>().IsOpen.Should().BeTrue();
    }

    [Test]
    public async Task SetLaserDial_WhenTheParserOmitsThePreposition_StillMovesTheSetting()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>act</intent>
                                               <verb>set</verb>
                                               <noun>laser</noun>
                                               <noun>1</noun>
                                               """));
        StartHere<StripNearRelay>();
        Take<Laser>();

        var response = await target.GetResponse("set laser to 1");

        response.Should().Contain("The dial is now set to 1");
        GetItem<Laser>().Setting.Should().Be(1);
    }

    [Test]
    public async Task SlideCardThroughSlot_WhenTheParserOmitsThePreposition_StillActivatesTheBooth()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>act</intent>
                                               <verb>slide</verb>
                                               <noun>teleportation card</noun>
                                               <noun>slot</noun>
                                               """));
        StartHere<BoothTwo>();
        Take<TeleportationAccessCard>();

        var response = await target.GetResponse("slide teleportation card through slot");

        response.Should().Contain("Redee");
    }

    /// <summary>
    ///     Issue #538. "press 3" is the only single-noun phrasing in the report that reached no
    ///     handler, and its object is a bare numeral — which rule 3 of the system prompt does not
    ///     describe as a noun phrase, so gpt-4o sometimes leaves it untagged entirely. That collapses
    ///     the turn to a NullIntent and the booth never teleports.
    /// </summary>
    [Test]
    public async Task PressADigitButton_WhenTheParserOmitsTheNoun_StillTeleports()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>act</intent>
                                               <verb>press</verb>
                                               """));
        var booth = StartHere<BoothTwo>();
        booth.IsEnabled = true;

        var response = await target.GetResponse("press 3");

        response.Should().Contain("strange feeling in the pit of your stomach");
        Context.CurrentLocation.Name.Should().Be("Booth 3");
    }

    /// <summary>
    ///     Issue #538, the other shape a bare numeral provokes: rather than dropping the digit, the
    ///     model expands it against the room description into "button 3". The booths listed
    ///     "3 button" but never that word order, so the command missed every match and fell through to
    ///     the narrator.
    /// </summary>
    [Test]
    public async Task PressADigitButton_WhenTheParserExpandsItToButtonN_StillTeleports()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>act</intent>
                                               <verb>press</verb>
                                               <noun>button 3</noun>
                                               """));
        var booth = StartHere<BoothTwo>();
        booth.IsEnabled = true;

        var response = await target.GetResponse("press 3");

        response.Should().Contain("strange feeling in the pit of your stomach");
        Context.CurrentLocation.Name.Should().Be("Booth 3");
    }

    /// <summary>
    ///     Issue #551. The deterministic TestParser maps "enter door" straight to an
    ///     EnterSubLocationIntent, so the #532/#534 disambiguation tests pass while a real player in
    ///     the Elevator Lobby got "You cannot go that way." — gpt-4o buckets bare "enter &lt;noun&gt;"
    ///     as a MOVE whose direction is "enter", so MoveEngine answered for a room with no In exit and
    ///     EnterSubLocationEngine (and its "Do you mean...?") was never reached. Only the longer
    ///     "go through door" phrasing ever got the question.
    /// </summary>
    [Test]
    public async Task EnterADoor_WhenTheParserBucketsItAsAMove_StillAsksWhichDoor()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>move</intent>
                                               <verb>enter</verb>
                                               <noun>door</noun>
                                               <direction>enter</direction>
                                               """));
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse("enter door");

        response.Should().Contain("Do you mean");
        response.Should().Contain("upper elevator door");
        response.Should().Contain("lower elevator door");
        Context.CurrentLocation.Should().BeOfType<ElevatorLobby>();
    }

    /// <summary>
    ///     And the other half of that same mis-bucketed shape: once the noun does name one door, the
    ///     command has to actually walk through it rather than refuse. Bare "enter blue door" is the
    ///     phrasing PR #534's table verified by hand in production.
    /// </summary>
    [Test]
    public async Task EnterANamedDoor_WhenTheParserBucketsItAsAMove_StillWalksThroughIt()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>move</intent>
                                               <verb>enter</verb>
                                               <noun>blue door</noun>
                                               <direction>enter</direction>
                                               """));
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = true;

        await target.GetResponse("enter blue door");

        Context.CurrentLocation.Should().BeOfType<UpperElevator>();
    }

    /// <summary>
    ///     The mirror image of the same defect, and the same room proves it: rule 5 lists "exit"
    ///     alongside "enter", so "exit door" is bucketed as a move in Direction.Out. The lobby has no
    ///     Out exit either, so ExitSubLocationEngine - which has carried the symmetric #532
    ///     disambiguation since it was written - was just as unreachable. "exit door" is already in
    ///     base-mappings.json, so the TestParser suite could never see this.
    /// </summary>
    [Test]
    public async Task ExitADoor_WhenTheParserBucketsItAsAMove_StillAsksWhichDoor()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>move</intent>
                                               <verb>exit</verb>
                                               <noun>door</noun>
                                               <direction>exit</direction>
                                               """));
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse("exit door");

        response.Should().Contain("Do you mean");
        response.Should().Contain("upper elevator door");
        response.Should().Contain("lower elevator door");
        Context.CurrentLocation.Should().BeOfType<ElevatorLobby>();
    }

    /// <summary>
    ///     From a given room a door gates exactly one passage, so "exit blue door" traverses it the
    ///     same way "enter blue door" does - the map says which way (issue #262, DoorReroute).
    /// </summary>
    [Test]
    public async Task ExitANamedDoor_WhenTheParserBucketsItAsAMove_StillWalksThroughIt()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>move</intent>
                                               <verb>exit</verb>
                                               <noun>blue door</noun>
                                               <direction>exit</direction>
                                               """));
        StartHere<ElevatorLobby>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = true;

        await target.GetResponse("exit blue door");

        Context.CurrentLocation.Should().BeOfType<UpperElevator>();
    }

    [Test]
    public async Task TakeWithATool_WhenTheParserBucketsItAsATake_StillRemovesTheFusedBedistor()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>take</intent>
                                               <verb>take</verb>
                                               <noun>fused bedistor</noun>
                                               <noun>pliers</noun>
                                               <preposition>with</preposition>
                                               """));
        StartHere<CourseControl>();
        Take<Pliers>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("take fused bedistor with pliers");

        response.Should().Contain("you manage to remove the fused bedistor");
        Context.HasItem<FusedBedistor>().Should().BeTrue();
    }
}
