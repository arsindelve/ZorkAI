namespace UnitTests.GlobalCommands;

public class LookCommandTests : EngineTestsBase
{
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
}