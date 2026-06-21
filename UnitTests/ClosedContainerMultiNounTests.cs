using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;
using Model.Interface;
using Model.Item;

namespace UnitTests;

/// <summary>
/// Verifies that a closed (and non-transparent) openable container does not leak its
/// contents to multi-noun interactions. A closed container already blocks simple
/// interactions and noun matching against its contents; multi-noun interactions must
/// behave the same way.
/// </summary>
public class ClosedContainerMultiNounTests : EngineTestsBase
{
    /// <summary>An item inside the container that always reacts to a multi-noun interaction.</summary>
    private class WidgetInsideContainer : ItemBase
    {
        public override string[] NounsForMatching => ["widget"];

        public override Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
        {
            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("You poke the widget. "));
        }
    }

    /// <summary>A minimal openable, non-transparent container holding the widget.</summary>
    private class TestBox : OpenAndCloseContainerBase
    {
        public override string[] NounsForMatching => ["box"];

        public override void Init()
        {
        }
    }

    [Test]
    public async Task ClosedContainer_DoesNotRespondToMultiNounInteractionOnItemInside()
    {
        Repository.Reset();
        var context = GetTarget().Context;

        var box = new TestBox();
        var widget = new WidgetInsideContainer();
        box.ItemPlacedHere(widget);
        box.IsOpen = false;

        var intent = new MultiNounIntent
        {
            Verb = "poke",
            NounOne = "widget",
            NounTwo = "stick",
            Preposition = "with",
            OriginalInput = "poke widget with stick"
        };

        var result = await box.RespondToMultiNounInteraction(intent, context);

        // The box is closed and not transparent, so the widget inside is inaccessible.
        result?.InteractionHappened.Should().NotBe(true);
    }

    [Test]
    public async Task OpenContainer_StillRespondsToMultiNounInteractionOnItemInside()
    {
        Repository.Reset();
        var context = GetTarget().Context;

        var box = new TestBox();
        var widget = new WidgetInsideContainer();
        box.ItemPlacedHere(widget);
        box.IsOpen = true;

        var intent = new MultiNounIntent
        {
            Verb = "poke",
            NounOne = "widget",
            NounTwo = "stick",
            Preposition = "with",
            OriginalInput = "poke widget with stick"
        };

        var result = await box.RespondToMultiNounInteraction(intent, context);

        result?.InteractionHappened.Should().BeTrue();
    }
}
