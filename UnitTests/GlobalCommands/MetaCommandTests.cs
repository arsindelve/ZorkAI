using GameEngine;

namespace UnitTests.GlobalCommands;

/// <summary>
/// The meta/informational global commands — score, diagnose, look, inventory — plus the two that
/// prompt for confirmation (quit, restart). They share one behavioral rule worth keeping in a single
/// file: per issue #354 none of them consumes a turn, so the "IsFreeAction_DoesNotAdvanceMoves" cases
/// below are a set and should be read together. Merged from the former ScoreCommandsTests /
/// DiagnoseCommandTests / RestartCommandsTests / QuitCommandTests / LookCommandTests /
/// InventoryCommandTests, each of which was one to five tests over the same GetTargetWithRealParser()
/// boilerplate.
/// </summary>
public class MetaCommandTests : EngineTestsBase
{
    [Test]
    public async Task Score()
    {
        var engine = GetTargetWithRealParser();

        // Act
        var response = await engine.GetResponse("score");

        // Assert
        response.Should().Contain("Beginner");
    }

    [Test]
    public async Task Score_AlternateScore_AlternateVery()
    {
        var engine = GetTargetWithRealParser();
        engine.Context.AddPoints(345);

        // Act
        var response = await engine.GetResponse("what is my score");

        // Assert
        response.Should().Contain("Wizard");
    }

    [Test]
    public async Task Score_IsFreeAction_DoesNotAdvanceMoves()
    {
        // Issue #354: "score" is a meta/informational verb and must not consume a turn.
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("score");

        // Assert
        engine.Moves.Should().Be(0);
    }

    [Test]
    public async Task Diagnose_IsFreeAction_DoesNotAdvanceMoves()
    {
        // Issue #354 follow-up: "diagnose" is a meta/informational verb (wound/death status), same
        // shape as score/look/inventory, and must not consume a turn either.
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("diagnose");

        // Assert
        engine.Moves.Should().Be(0);
    }

    [Test]
    public async Task Static_Intent_Look()
    {
        var target = GetTargetWithRealParser();

        // Act
        var result = await target.GetResponse("Where am I? ");

        // Assert
        result.Should().Contain("You are standing in an open field");
    }

    [Test]
    public async Task Static_Intent_Look_AlternatePhrase()
    {
        var target = GetTargetWithRealParser();

        // Act
        var result = await target.GetResponse("LOOK ");

        // Assert
        result.Should().Contain("You are standing in an open field");
    }

    [Test]
    public async Task Look_ItemIsClosed_NeverBeenOpened()
    {
        var target = GetTargetWithRealParser();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        var result = await target.GetResponse("look");

        // Assert
        result.Should().Contain("On the table is an elongated brown sack, smelling of hot peppers");
    }

    [Test]
    public async Task Look_ItemIsClosed_AlreadyOpened()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("open sack");
        await target.GetResponse("close sack");
        var result = await target.GetResponse("look");

        // Assert
        result.Should().Contain("There is a brown sack here.");
    }

    [Test]
    public async Task Look_ItemIsTakenAndDropped()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("take sack");
        await target.GetResponse("drop sack");
        var result = await target.GetResponse("look");

        // Assert
        result.Should().Contain("There is a brown sack here.");
    }

    [Test]
    public async Task Look_IsFreeAction_DoesNotAdvanceMoves()
    {
        // Issue #354: "look" is a meta/informational verb and must not consume a turn.
        var target = GetTarget();

        // Act
        await target.GetResponse("look");

        // Assert
        target.Moves.Should().Be(0);
    }

    [Test]
    public async Task Inventory_EmptyHandedDescription()
    {
        var engine = GetTargetWithRealParser();
        engine.Context.Items.Should().BeEmpty();

        // Act
        var response = await engine.GetResponse("i");

        // Assert
        response.Should().Contain("empty-handed");
    }

    [Test]
    public async Task Inventory_WithLeaflet()
    {
        var engine = GetTarget();
        engine.Context.Items.Should().BeEmpty();

        // Act
        await engine.GetResponse("open mailbox");
        await engine.GetResponse("take leaflet");
        var response = await engine.GetResponse("i");

        // Assert
        response.Should().Contain("leaflet");
    }

    [Test]
    public async Task Static_Intent_Inventory_Alternate_Phrase()
    {
        var target = GetTargetWithRealParser();

        // Act
        var result = await target.GetResponse("what do I have on me?");

        // Assert
        result.Should().Contain("empty-handed");
    }

    [Test]
    public async Task Inventory_IsFreeAction_DoesNotAdvanceMoves()
    {
        // Issue #354: "inventory" is a meta/informational verb and must not consume a turn.
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("i");

        // Assert
        engine.Moves.Should().Be(0);
    }

    [Test]
    public async Task Quit_Cancel()
    {
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("quit");
        await engine.GetResponse("nevermind");
        await engine.GetResponse("look");

        var response = await engine.GetResponse("look around");

        // Assert
        response.Should().Contain("You are standing in an open field");
    }

    [Test]
    public async Task Quit_Cancel_WithBlankInput()
    {
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("quit");
        await engine.GetResponse("");
        await engine.GetResponse("look");

        var response = await engine.GetResponse("look around");

        // Assert
        response.Should().Contain("You are standing in an open field");
    }

    [Test]
    public async Task Quit_Affirmative()
    {
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("quit");
        var response = await engine.GetResponse("yes");

        // Assert
        response.Should().Contain("-1");
    }

    [Test]
    public async Task Quit_Affirmative_AlternativeResponse()
    {
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("I want to quit");
        var response = await engine.GetResponse("yup");

        // Assert
        response.Should().Contain("-1");
    }

    [Test]
    public async Task Restart_Affirmative()
    {
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("start over");
        var response = await engine.GetResponse("yes");

        // Assert
        response.Should().Contain("-2");
    }

    [Test]
    public async Task Restart_Affirmative_AlternativeResponse()
    {
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("restart");
        var response = await engine.GetResponse("yup");

        // Assert
        response.Should().Contain("-2");
    }

    [Test]
    public async Task Restart_Cancel_WithBlankInput()
    {
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("restart");
        await engine.GetResponse("");
        await engine.GetResponse("look");

        var response = await engine.GetResponse("look around");

        // Assert
        response.Should().Contain("You are standing in an open field");
    }
}
