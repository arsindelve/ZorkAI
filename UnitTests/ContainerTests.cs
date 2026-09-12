using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace UnitTests;

/// <summary>
/// Container base-class behavior, independent of any single game item: whether a closed container
/// leaks its contents into noun matching and multi-noun interactions, what
/// <see cref="OpenAndCloseContainerBase" /> reports when examined, and whether light escapes a
/// container to illuminate the room. Merged from the former ClosedContainerMultiNounTests /
/// OpenAndCloseContainerExamineTests / HasLightSourceContainerTests — three two-to-four test files
/// asking the same question from different angles: what does a container expose, and when?
/// </summary>
public class ContainerTests : EngineTestsBase
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

    private class ShinyWidget : ItemBase
    {
        public override string[] NounsForMatching => ["widget"];

        public override string GenericDescription(ILocation? currentLocation) => "a shiny widget";
    }

    /// <summary>A minimal openable, non-transparent container.</summary>
    private class TestBox : OpenAndCloseContainerBase
    {
        public override string[] NounsForMatching => ["box"];

        public override void Init()
        {
        }
    }

    /// <summary>An opaque (non-transparent) openable container that inherits the default examine.</summary>
    private class OpaqueBox : OpenAndCloseContainerBase
    {
        public override string[] NounsForMatching => ["box"];

        public override void Init()
        {
        }
    }

    /// <summary>A transparent openable container that inherits the default examine.</summary>
    private class GlassBox : OpenAndCloseContainerBase
    {
        public override string[] NounsForMatching => ["glass box"];

        public override bool IsTransparent => true;

        public override void Init()
        {
        }
    }

    private static string Examine(OpenAndCloseContainerBase container) =>
        ((ICanBeExamined)container).ExaminationDescription;

    // A closed (and non-transparent) openable container must not leak its contents to multi-noun
    // interactions. A closed container already blocks simple interactions and noun matching against
    // its contents; multi-noun interactions must behave the same way.

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

    // The default ICanBeExamined.ExaminationDescription that OpenAndCloseContainerBase provides
    // (issue #398). Minimal test doubles keep the base behavior independent of any single game item.

    [Test]
    public void OpaqueClosed_ReportsClosed_AndHidesContents()
    {
        var box = new OpaqueBox();
        box.ItemPlacedHere(new ShinyWidget());
        box.IsOpen = false;

        var text = Examine(box);

        text.Should().Contain("The box is closed");
        text.Should().NotContain("shiny widget");
    }

    [Test]
    public void OpaqueOpen_ListsContents()
    {
        var box = new OpaqueBox();
        box.ItemPlacedHere(new ShinyWidget());
        box.IsOpen = true;

        var text = Examine(box);

        text.Should().Contain("shiny widget");
        text.Should().NotContain("is closed");
    }

    [Test]
    public void OpaqueOpenEmpty_ReportsEmpty()
    {
        var box = new OpaqueBox();
        box.IsOpen = true;

        var text = Examine(box);

        text.Should().Contain("The box is empty");
    }

    // Hardening (issue #398 review): a transparent container reveals its contents even when
    // closed, matching ItemListDescription's own transparency handling. Before the fix the
    // default short-circuited to "The glass box is closed." and hid the widget.
    [Test]
    public void TransparentClosed_StillListsContents()
    {
        var box = new GlassBox();
        box.ItemPlacedHere(new ShinyWidget());
        box.IsOpen = false;

        var text = Examine(box);

        text.Should().Contain("shiny widget");
        text.Should().NotContain("is closed");
    }

    // Context.HasLightSource for light sources that live inside a container in the room (the
    // "shaft basket" puzzle scenario).

    [Test]
    public void OffLampInTransparentContainerInRoom_IsNotALightSource()
    {
        var engine = GetTarget();

        // Dark room as the current location.
        var cellar = Repository.GetLocation<Cellar>();
        engine.Context.CurrentLocation = cellar;

        // A transparent container in the room.
        var basket = Repository.GetItem<Basket>();
        cellar.ItemPlacedHere(basket);

        // A toggleable lamp inside the container, turned OFF.
        var lantern = Repository.GetItem<Lantern>();
        lantern.IsOn = false;
        basket.ItemPlacedHere(lantern);

        // An OFF lamp provides no light, even inside an open/transparent container.
        engine.Context.HasLightSource.Should().BeFalse();
    }

    [Test]
    public void OnLampInTransparentContainerInRoom_IsALightSource()
    {
        var engine = GetTarget();

        var cellar = Repository.GetLocation<Cellar>();
        engine.Context.CurrentLocation = cellar;

        var basket = Repository.GetItem<Basket>();
        cellar.ItemPlacedHere(basket);

        var lantern = Repository.GetItem<Lantern>();
        lantern.IsOn = true;
        basket.ItemPlacedHere(lantern);

        engine.Context.HasLightSource.Should().BeTrue();
    }

    [Test]
    public void ConstantLightSourceInTransparentContainerInRoom_IsALightSource()
    {
        var engine = GetTarget();

        var cellar = Repository.GetLocation<Cellar>();
        engine.Context.CurrentLocation = cellar;

        var basket = Repository.GetItem<Basket>();
        cellar.ItemPlacedHere(basket);

        // The torch is a constant light source (ICannotBeTurnedOff) - the actual shaft puzzle item.
        var torch = Repository.GetItem<Torch>();
        basket.ItemPlacedHere(torch);

        engine.Context.HasLightSource.Should().BeTrue();
    }
}
