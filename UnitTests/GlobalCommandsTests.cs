using ZorkOne.Location;

namespace UnitTests;

public class GlobalCommandsTests : EngineTestsBase
{
    [Test]
    public async Task Inventory_EmptyHandedDescription()
    {
        var engine = GetTarget(new IntentParser());
        engine.Context.Items.Should().BeEmpty();

        // Act
        var response = await engine.GetResponse("i");

        // Assert
        response.Should().Contain("empty-handed");
    }
    
    [Test]
    public async Task Static_Intent_Inventory_Alternate_Phrase()
    {
        var target = GetTarget(new IntentParser());

        // Act
        var result = await target.GetResponse("what do I have on me?");

        // Assert
        result.Should().Contain("empty-handed");
    }

    [Test]
    public async Task Static_Intent_AlternatePhrase()
    {
        var target = GetTarget(new IntentParser());

        // Act
        var result = await target.GetResponse("LOOK ");

        // Assert
        result.Should().Contain("You are standing in an open field");
    }
    
    [Test]
    public async Task Static_Intent_Look()
    {
        var target = GetTarget(new IntentParser());

        // Act
        var result = await target.GetResponse("Where am I? ");

        // Assert
        result.Should().Contain("You are standing in an open field");
    }
    
    [Test]
    public async Task Look_ItemIsClosed_NeverBeenOpened()
    {
        var target = GetTarget(new IntentParser());
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
    public async Task Quit_Cancel()
    {
        var engine = GetTarget(new IntentParser());

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
        var engine = GetTarget(new IntentParser());

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
        var engine = GetTarget(new IntentParser());

        // Act
        await engine.GetResponse("quit");
        var response = await engine.GetResponse("yes");

        // Assert
        response.Should().Contain("-1");
    }
    
    [Test]
    public async Task Quit_Affirmative_AlternativeResponse()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        await engine.GetResponse("I want to quit");
        var response = await engine.GetResponse("yup");

        // Assert
        response.Should().Contain("-1");
    }
    
    [Test]
    public async Task Score()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        var response = await engine.GetResponse("score");

        // Assert
        response.Should().Contain("Beginner");
    }

    [Test]
    public async Task Score_AlternateScore_AlternateVery()
    {
        var engine = GetTarget(new IntentParser());
        engine.Context.AddPoints(345);

        // Act
        var response = await engine.GetResponse("what is my score");

        // Assert
        response.Should().Contain("Wizard");
    }
    
    [Test]
    public async Task Restart_Affirmative()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        await engine.GetResponse("start over");
        var response = await engine.GetResponse("yes");

        // Assert
        response.Should().Contain("-2");
    }
    
    [Test]
    public async Task Restart_Affirmative_AlternativeResponse()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        await engine.GetResponse("restart");
        var response = await engine.GetResponse("yup");

        // Assert
        response.Should().Contain("-2");
    }
    
    [Test]
    public async Task Restart_Cancel_WithBlankInput()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        await engine.GetResponse("restart");
        await engine.GetResponse("");
        await engine.GetResponse("look");
        
        var response = await engine.GetResponse("look around");

        // Assert
        response.Should().Contain("You are standing in an open field");
    }
}