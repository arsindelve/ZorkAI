using Utilities;

namespace UnitTests.Utilities;

[TestFixture]
public class LimitedStackTests
{
    [Test]
    public void Push_AddsItemToStack()
    {
        // Arrange
        var stack = new LimitedStack<string>();

        // Act
        stack.Push("test");

        // Assert
        stack.GetAll().Should().ContainSingle().Which.Should().Be("test");
    }

    [Test]
    public void Push_ExceedingMaxCount_RemovesOldestItem()
    {
        // Arrange
        var stack = new LimitedStack<int>();

        // Act - push 6 items (max is 5)
        for (var i = 1; i <= 6; i++) stack.Push(i);

        // Assert - the first item (1) should be removed
        stack.GetAll().Should().HaveCount(5);
        stack.GetAll().Should().BeEquivalentTo(new[] { 2, 3, 4, 5, 6 });
    }

    [Test]
    public void GetAll_EmptyStack_ReturnsEmptyList()
    {
        // Arrange
        var stack = new LimitedStack<string>();

        // Act
        var result = stack.GetAll();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetAll_ReturnsAllItems()
    {
        // Arrange
        var stack = new LimitedStack<int>();
        var items = new[] { 1, 2, 3 };

        foreach (var item in items) stack.Push(item);

        // Act
        var result = stack.GetAll();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(items);
    }

    [Test]
    public void ToString_EmptyStack_ReturnsEmptyString()
    {
        // Arrange
        var stack = new LimitedStack<string>();

        // Act
        var result = stack.ToString();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ToString_ReturnsNumberedItems()
    {
        // Arrange
        var stack = new LimitedStack<string>();
        stack.Push("first");
        stack.Push("second");
        stack.Push("third");

        // Act
        var result = stack.ToString();

        // Assert
        var expectedString = "1: third\n2: second\n3: first";
        result.Should().Be(expectedString);
    }

    [Test]
    public void Peek_EmptyStack_ReturnsDefault()
    {
        // Arrange
        var stack = new LimitedStack<string>();

        // Act
        var result = stack.Peek();

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void Peek_ReturnsLastItem_WithoutRemoving()
    {
        // Arrange
        var stack = new LimitedStack<int>();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        // Act
        var result = stack.Peek();

        // Assert
        result.Should().Be(3);
        stack.GetAll().Should().HaveCount(3); // Verify item was not removed
    }

    [Test]
    public void Stack_MaintainsInsertionOrder()
    {
        // Arrange
        var stack = new LimitedStack<int>();

        // Act
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);

        // Assert
        stack.GetAll().Should().BeInAscendingOrder();
    }

    [Test]
    public void Stack_LimitsToMaxCount()
    {
        // Arrange
        var stack = new LimitedStack<int>();

        // Act - push more than max count items
        for (var i = 1; i <= 10; i++) stack.Push(i);

        // Assert
        stack.GetAll().Should().HaveCount(5);
        stack.GetAll().Should().BeEquivalentTo([6, 7, 8, 9, 10]);
    }

    [Test]
    public void Peek_AfterPushingMaxItems_ReturnsLastItem()
    {
        // Arrange
        var stack = new LimitedStack<int>();

        // Act - push more than max count items
        for (var i = 1; i <= 6; i++) stack.Push(i);

        // Assert
        stack.Peek().Should().Be(6);
    }

    [Test]
    public void ToString_AfterPushingMaxItems_ReturnsCorrectlyNumberedItems()
    {
        // Arrange
        var stack = new LimitedStack<string>();
        stack.Push("one");
        stack.Push("two");
        stack.Push("three");
        stack.Push("four");
        stack.Push("five");
        stack.Push("six"); // This pushes "one" out

        // Act
        var result = stack.ToString();

        // Assert
        var expectedString = "1: six\n2: five\n3: four\n4: three\n5: two";
        result.Should().Be(expectedString);
    }

    [Test]
    public void GetAll_DoesNotModifyStack()
    {
        // Arrange
        var stack = new LimitedStack<int>();
        stack.Push(1);
        stack.Push(2);

        // Act
        var firstGet = stack.GetAll();
        var secondGet = stack.GetAll();

        // Assert
        firstGet.Should().BeEquivalentTo(secondGet);
        stack.Peek().Should().Be(2);
    }
}