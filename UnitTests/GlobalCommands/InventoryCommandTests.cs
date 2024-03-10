namespace UnitTests.GlobalCommands;

public class InventoryCommandTests : EngineTestsBase
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
}