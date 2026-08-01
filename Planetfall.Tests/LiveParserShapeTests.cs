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
