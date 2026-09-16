using FluentAssertions;
using GameEngine;
using Model.Interface;
using Moq;
using OpenAI.Chat;
using Planetfall.GlobalCommand;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Computer;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Mech;
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

    /// <summary>
    ///     Issue #580. PR #573 fixed the shape above and shipped in 2.11.0, yet "enter door" in the
    ///     Elevator Lobby still answered "You cannot go that way." on 14 of 14 production trials while
    ///     its mirror "exit door" reached the question. That asymmetry pins the shape gpt-4o really
    ///     emits: "exit the boat" is an in-prompt example that KEEPS its noun, but nothing in the
    ///     prompt shows "enter &lt;thing&gt;" as a board, so the model reads bare "enter door" as a pure
    ///     DIRECTION move - rule 1a lists "in" as a relative direction and rule 5 lists the literal
    ///     word "enter" - and a direction has no object, so it emits no &lt;noun&gt; at all. With the
    ///     object gone, MoveEngine.TryBoardTheNamedObject has nothing to look up and the lobby's
    ///     missing In exit produces the flat refusal. The other two candidate causes are ruled out by
    ///     the same report: "examine door" asks the question in this room, so the noun IS resolvable in
    ///     scope; and a direction that failed to resolve would have gone to destination navigation
    ///     ("You can't get there from here."), not to MoveEngine's refusal. The player named the door -
    ///     recover it from their own words.
    /// </summary>
    [Test]
    public async Task EnterADoor_WhenTheParserDropsTheNounEntirely_StillAsksWhichDoor()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>move</intent>
                                               <verb>enter</verb>
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
    ///     Issue #580, the same dropped noun with the direction tagged as the word "in" rather than
    ///     "enter". Both spellings are on rule 5's list and both resolve to Direction.In, so the
    ///     recovery must not depend on which one the model happened to pick.
    /// </summary>
    [Test]
    public async Task EnterADoor_WhenTheParserDropsTheNounAndTagsTheDirectionIn_StillAsksWhichDoor()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>move</intent>
                                               <verb>enter</verb>
                                               <direction>in</direction>
                                               """));
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse("enter door");

        response.Should().Contain("Do you mean");
        response.Should().Contain("upper elevator door");
        response.Should().Contain("lower elevator door");
        Context.CurrentLocation.Should().BeOfType<ElevatorLobby>();
    }

    /// <summary>
    ///     Issue #580's decisive discriminator, reproduced: the Upper Elevator has exactly ONE door in
    ///     scope, so nothing here is about disambiguation - with the object recovered, "enter door"
    ///     walks through the door the way "go through door" always did, instead of refusing.
    /// </summary>
    [Test]
    public async Task EnterADoor_WhenTheParserDropsTheNoun_StillWalksThroughTheOnlyDoorInTheRoom()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>move</intent>
                                               <verb>enter</verb>
                                               <direction>enter</direction>
                                               """));
        StartHere<UpperElevator>();
        GetItem<UpperElevatorDoor>().IsOpen = true;
        GetLocation<UpperElevator>().InLobby = true;

        var response = await target.GetResponse("enter door");

        response.Should().NotContain("cannot go that way");
        Context.CurrentLocation.Should().BeOfType<ElevatorLobby>();
    }

    /// <summary>
    ///     Issue #580, point 5: even the half that worked was model-dependent - "exit door" produced
    ///     the question on only 1 of 5 production trials. The same dropped noun is the shape behind the
    ///     trial that refused, and the same recovery fixes it, so neither half depends on which bucket
    ///     the model picks.
    /// </summary>
    [Test]
    public async Task ExitADoor_WhenTheParserDropsTheNounEntirely_StillAsksWhichDoor()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>move</intent>
                                               <verb>exit</verb>
                                               <direction>exit</direction>
                                               """));
        StartHere<ElevatorLobby>();

        var response = await target.GetResponse("exit door");

        response.Should().Contain("Do you mean");
        response.Should().Contain("upper elevator door");
        response.Should().Contain("lower elevator door");
        Context.CurrentLocation.Should().BeOfType<ElevatorLobby>();
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

    /// <summary>
    ///     Issue #550, a residual of #538 that PR #540's preposition recovery does not cover: here the
    ///     preposition survives, the multi-noun routing is healthy, and the cube handler is healthy —
    ///     only the bare adjective fails to resolve. The original gives the good bedistor the
    ///     bare-adjective handle its fused sibling has (<c>ADJECTIVE GOOD NINETY OHM</c>,
    ///     planetfall-source/compone.zil:1419), which is exactly why "take fused with pliers" works in
    ///     this port while "put good in cube" dropped the turn.
    ///     <para>
    ///         The walkthrough suite could not catch it: <c>base-mappings.json</c> pre-expanded
    ///         "put good in cube" to nounOne "good bedistor" — an intent the parser never produces — so
    ///         the bare noun was never exercised. That mapping is now faithful to the parser, which
    ///         makes walkthrough step 262 a second guard; this test is the one that pins the real
    ///         parser rather than a model of it.
    ///     </para>
    /// </summary>
    [Test]
    public async Task PutTheBedistorByItsBareAdjective_WithTheRealParser_StillFixesCourseControl()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>act</intent>
                                               <verb>put</verb>
                                               <noun>good</noun>
                                               <noun>cube</noun>
                                               <preposition>in</preposition>
                                               """));
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;
        Take<FusedBedistor>(); // pried out of the socket and still in hand, as in the prod report
        Take<GoodBedistor>();

        var response = await target.GetResponse("put good in cube");

        response.Should().Contain("The warning lights go out");
        GetItem<LargeMetalCube>().HasItem<GoodBedistor>().Should().BeTrue();
        GetLocation<CourseControl>().Fixed.Should().BeTrue();
    }

    /// <summary>
    ///     Issue #521. "ask floyd for the diary" is the original's own grammar for being handed
    ///     something back (<c>TELL/ASK &lt;actor&gt; FOR &lt;object&gt;</c> -> <c>V-ASK-FOR</c>,
    ///     planetfall-source/syntax.zil:341-342). It is the one phrasing of this mechanic that does NOT
    ///     reach Floyd as speech - it leads with the verb rather than his name, so direct-address
    ///     detection never fires - which makes the real parser's shape for it load-bearing rather than
    ///     incidental. <c>base-mappings.json</c> stubs the same command for the handler tests; this pins
    ///     the parser that has to produce it in production.
    /// </summary>
    [Test]
    public async Task AskFloydForTheDiary_WithTheRealParser_StillGetsItBack()
    {
        var target = GetTarget(ParserReturning("""
                                               <intent>act</intent>
                                               <verb>ask</verb>
                                               <noun>floyd</noun>
                                               <noun>diary</noun>
                                               <preposition>for</preposition>
                                               """));
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.ItemBeingHeld = Take<Diary>();
        Context.RemoveItem(GetItem<Diary>());
        GetItem<Diary>().CurrentLocation = floyd;

        var response = await target.GetResponse("ask floyd for the diary");

        response.Should().Contain("handing you the diary");
        Context.HasItem<Diary>().Should().BeTrue();
        floyd.ItemBeingHeld.Should().BeNull();
    }
}
