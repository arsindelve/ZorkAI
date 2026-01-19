using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Item;

namespace UnitTests.SingleNounProcessors;

[TestFixture]
public class ClothingOnAndOffProcessorTests
{
    private ClothingOnAndOffProcessor _processor = null!;
    private Mock<IGenerationClient> _client = null!;
    private Mock<IContext> _context = null!;

    [SetUp]
    public void SetUp()
    {
        _processor = new ClothingOnAndOffProcessor();
        _client = new Mock<IGenerationClient>();
        _context = new Mock<IContext>();
    }

    private static Mock<T> CreateMockClothingItem<T>(bool beingWorn = false) where T : class, IItem, IAmClothing
    {
        var mock = new Mock<T>();
        mock.SetupProperty(x => x.BeingWorn, beingWorn);
        mock.Setup(x => x.NounsForMatching).Returns(["jacket", "coat"]);
        return mock;
    }

    #region Wear/Put On Tests

    [Test]
    [TestCase("wear")]
    [TestCase("don")]
    [TestCase("put on")]
    [TestCase("dress in")]
    [TestCase("slip on")]
    public async Task Process_WearClothing_SetsBeingWornTrue(string verb)
    {
        // Arrange
        var mockClothing = CreateMockClothingItem<IClothingItem>(beingWorn: false);
        var intent = new SimpleIntent { Verb = verb, Noun = "jacket" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockClothing.Object, _client.Object);

        // Assert
        result.Should().NotBeNull();
        mockClothing.Object.BeingWorn.Should().BeTrue();
        result!.InteractionMessage.Should().Contain("wearing");
        result.InteractionMessage.Should().Contain("jacket");
    }

    [Test]
    public async Task Process_WearClothing_AlreadyWorn_StillSetsWorn()
    {
        // Arrange - clothing is already being worn
        var mockClothing = CreateMockClothingItem<IClothingItem>(beingWorn: true);
        var intent = new SimpleIntent { Verb = "wear", Noun = "jacket" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockClothing.Object, _client.Object);

        // Assert
        result.Should().NotBeNull();
        mockClothing.Object.BeingWorn.Should().BeTrue();
        result!.InteractionMessage.Should().Contain("wearing");
    }

    [Test]
    public async Task Process_WearNonClothingItem_ReturnsNull()
    {
        // Arrange - an item that is not clothing
        var mockNonClothing = new Mock<IItem>();
        mockNonClothing.Setup(x => x.NounsForMatching).Returns(["sword"]);
        var intent = new SimpleIntent { Verb = "wear", Noun = "sword" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockNonClothing.Object, _client.Object);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Process_WearNonItemTarget_ReturnsNull()
    {
        // Arrange - something that's not even an IItem
        var mockTarget = new Mock<IInteractionTarget>();
        var intent = new SimpleIntent { Verb = "wear", Noun = "nothing" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockTarget.Object, _client.Object);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Take Off/Remove Tests

    [Test]
    [TestCase("take off")]
    [TestCase("doff")]
    [TestCase("remove")]
    [TestCase("discard")]
    [TestCase("slip off")]
    public async Task Process_TakeOffClothing_SetsBeingWornFalse(string verb)
    {
        // Arrange
        var mockClothing = CreateMockClothingItem<IClothingItem>(beingWorn: true);
        var intent = new SimpleIntent { Verb = verb, Noun = "jacket" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockClothing.Object, _client.Object);

        // Assert
        result.Should().NotBeNull();
        mockClothing.Object.BeingWorn.Should().BeFalse();
        result!.InteractionMessage.Should().Contain("removed");
        result.InteractionMessage.Should().Contain("jacket");
    }

    [Test]
    public async Task Process_TakeOffClothing_NotWorn_ReturnsNotWearingMessage()
    {
        // Arrange - clothing is not being worn
        var mockClothing = CreateMockClothingItem<IClothingItem>(beingWorn: false);
        var intent = new SimpleIntent { Verb = "remove", Noun = "jacket" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockClothing.Object, _client.Object);

        // Assert
        result.Should().NotBeNull();
        mockClothing.Object.BeingWorn.Should().BeFalse();
        result!.InteractionMessage.Should().Contain("aren't wearing");
    }

    [Test]
    public async Task Process_TakeOffNonClothingItem_ReturnsNull()
    {
        // Arrange
        var mockNonClothing = new Mock<IItem>();
        mockNonClothing.Setup(x => x.NounsForMatching).Returns(["sword"]);
        var intent = new SimpleIntent { Verb = "remove", Noun = "sword" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockNonClothing.Object, _client.Object);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Unrecognized Verb Tests

    [Test]
    [TestCase("eat")]
    [TestCase("throw")]
    [TestCase("examine")]
    [TestCase("take")]
    [TestCase("drop")]
    public async Task Process_UnrecognizedVerb_ReturnsNull(string verb)
    {
        // Arrange
        var mockClothing = CreateMockClothingItem<IClothingItem>(beingWorn: false);
        var intent = new SimpleIntent { Verb = verb, Noun = "jacket" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockClothing.Object, _client.Object);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Process_NullVerb_ReturnsNull()
    {
        // Arrange
        var mockClothing = CreateMockClothingItem<IClothingItem>(beingWorn: false);
        var intent = new SimpleIntent { Verb = null!, Noun = "jacket" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockClothing.Object, _client.Object);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Process_EmptyVerb_ReturnsNull()
    {
        // Arrange
        var mockClothing = CreateMockClothingItem<IClothingItem>(beingWorn: false);
        var intent = new SimpleIntent { Verb = "", Noun = "jacket" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockClothing.Object, _client.Object);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Process_VerbWithExtraWhitespace_StillWorks()
    {
        // Arrange
        var mockClothing = CreateMockClothingItem<IClothingItem>(beingWorn: false);
        var intent = new SimpleIntent { Verb = "  wear  ", Noun = "jacket" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockClothing.Object, _client.Object);

        // Assert
        result.Should().NotBeNull();
        mockClothing.Object.BeingWorn.Should().BeTrue();
    }

    [Test]
    public async Task Process_VerbWithMixedCase_StillWorks()
    {
        // Arrange
        var mockClothing = CreateMockClothingItem<IClothingItem>(beingWorn: false);
        var intent = new SimpleIntent { Verb = "WEAR", Noun = "jacket" };

        // Act
        var result = await _processor.Process(intent, _context.Object, mockClothing.Object, _client.Object);

        // Assert
        result.Should().NotBeNull();
        mockClothing.Object.BeingWorn.Should().BeTrue();
    }

    #endregion

    // Helper interface that combines IItem and IAmClothing for mocking
    public interface IClothingItem : IItem, IAmClothing, IInteractionTarget
    {
    }
}
