using FluentAssertions;
using GameEngine;
using GameEngine.Item;
using Model.Item;
using Model.Location;
using NUnit.Framework;

namespace UnitTests;

/// <summary>
/// Exercises the default <see cref="ICanBeExamined.ExaminationDescription" /> that
/// <see cref="OpenAndCloseContainerBase" /> provides (issue #398). Uses minimal test doubles so
/// the base behavior is verified independently of any single game item.
/// </summary>
public class OpenAndCloseContainerExamineTests
{
    private class ShinyWidget : ItemBase
    {
        public override string[] NounsForMatching => ["widget"];

        public override string GenericDescription(ILocation? currentLocation) => "a shiny widget";
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
}
