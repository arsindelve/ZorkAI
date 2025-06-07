using Utilities;

namespace UnitTests.Utilities;

[TestFixture]
public class ExtensionMethodsTests
{
    [Test]
    public void GetRandomElement_NullList_ThrowsArgumentException()
    {
        // Arrange
        List<string> nullList = null!;

        // Act & Assert
        Action action = () => nullList!.GetRandomElement();
        action.Should().Throw<ArgumentException>()
            .WithMessage("List cannot be null or empty");
    }

    [Test]
    public void GetRandomElement_EmptyList_ThrowsArgumentException()
    {
        // Arrange
        var emptyList = new List<int>();

        // Act & Assert
        Action action = () => emptyList.GetRandomElement();
        action.Should().Throw<ArgumentException>()
            .WithMessage("List cannot be null or empty");
    }

    [Test]
    public void GetRandomElement_SingleItemList_ReturnsThatItem()
    {
        // Arrange
        var singleItemList = new List<string> { "only item" };

        // Act
        var result = singleItemList.GetRandomElement();

        // Assert
        result.Should().Be("only item");
    }

    [Test]
    public void GetRandomElement_MultipleItemList_ReturnsItemFromList()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = list.GetRandomElement();

        // Assert
        list.Should().Contain(result);
    }

    [Test]
    public void GetRandomElement_CalledMultipleTimes_ReturnsRandomElements()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var results = new HashSet<int>();
        var numberOfIterations = 50; // High enough to ensure randomness, but not too high to make test slow

        // Act
        for (var i = 0; i < numberOfIterations; i++)
        {
            results.Add(list.GetRandomElement());

            // If we've collected at least 3 different elements, we can be reasonably confident in the randomness
            if (results.Count >= 3)
                break;
        }

        // Assert
        // We expect to get at least 3 different elements after multiple calls
        // This verifies the randomness to some degree without making the test flaky
        results.Count.Should().BeGreaterOrEqualTo(3,
            "calling GetRandomElement multiple times should return different elements due to randomness");
    }

    [Test]
    public void GetRandomElement_WorksWithDifferentGenericTypes()
    {
        // Arrange
        var intList = new List<int> { 1, 2, 3 };
        var stringList = new List<string> { "a", "b", "c" };
        var doubleList = new List<double> { 1.1, 2.2, 3.3 };
        var objectList = new List<object> { "a", 1, 2.2 };

        // Act & Assert
        intList.GetRandomElement().Should().BeOneOf(1, 2, 3);
        stringList.GetRandomElement().Should().BeOneOf("a", "b", "c");
        doubleList.GetRandomElement().Should().BeOneOf(1.1, 2.2, 3.3);
        objectList.GetRandomElement().Should().BeOneOf("a", 1, 2.2);
    }
    
    [Test]
    public void ToInteger_WithNumericString_ReturnsCorrectInteger()
    {
        // Arrange
        var numericString = "42";
        
        // Act
        var result = numericString.ToInteger();
        
        // Assert
        result.Should().Be(42);
    }
    
    [Test]
    public void ToInteger_WithWordNumber_ReturnsCorrectInteger()
    {
        // Arrange
        var wordNumber = "five";
        
        // Act
        var result = wordNumber.ToInteger();
        
        // Assert
        result.Should().Be(5);
    }
    
    [Test]
    public void ToInteger_WithComplexWordNumber_ReturnsCorrectInteger()
    {
        // Arrange
        var wordNumber = "twenty three";
        
        // Act
        var result = wordNumber.ToInteger();
        
        // Assert
        result.Should().Be(23);
    }
    
    [Test]
    public void ToInteger_WithInvalidInput_ReturnsNull()
    {
        // Arrange
        var invalidInput = "not a number";
        
        // Act
        var result = invalidInput.ToInteger();
        
        // Assert
        result.Should().BeNull();
    }
    
    [Test]
    public void ToInteger_WithNull_ReturnsNull()
    {
        // Arrange
        string? nullString = null;
        
        // Act
        var result = nullString.ToInteger();
        
        // Assert
        result.Should().BeNull();
    }
    
    [Test]
    public void IndentLines_WithSingleLine_IndentsCorrectly()
    {
        // Arrange
        var singleLine = "This is a single line";
        
        // Act
        var result = singleLine.IndentLines();
        
        // Assert
        // Don't hardcode line endings, but check the contents are correct
        result.Should().StartWith("   This is a single line");
        result.Should().EndWith(Environment.NewLine);
    }
    
    [Test]
    public void IndentLines_WithMultipleLines_IndentsEachLine()
    {
        // Arrange
        var multipleLines = $"First line{Environment.NewLine}Second line{Environment.NewLine}Third line";
        
        // Act
        var result = multipleLines.IndentLines();
        
        // Assert
        // Check for the presence of each indented line without hardcoding exact line endings
        result.Should().Contain($"  First line");
        result.Should().Contain($"  Second line");
        result.Should().Contain($"  Third line");
    }
    
    [Test]
    public void IndentLines_WithEmptyString_ReturnsIndentedEmptyLine()
    {
        // Arrange
        var emptyString = "";
        
        // Act
        var result = emptyString.IndentLines();
        
        // Assert
        result.Should().Be($"   {Environment.NewLine}");
    }
    
    [Test]
    public void StripNonChars_WithMixedContent_RemovesNonAlphabeticCharacters()
    {
        // Arrange
        var mixedContent = "Hello, World! 123";
        
        // Act
        var result = mixedContent.StripNonChars();
        
        // Assert
        result.Should().Be("Hello World ");
    }
    
    [Test]
    public void StripNonChars_WithOnlyAlphabeticCharacters_ReturnsSameString()
    {
        // Arrange
        var onlyAlphabetic = "HelloWorld";
        
        // Act
        var result = onlyAlphabetic.StripNonChars();
        
        // Assert
        result.Should().Be("HelloWorld");
    }
    
    [Test]
    public void StripNonChars_WithOnlyNonAlphabeticCharacters_ReturnsEmptyString()
    {
        // Arrange
        var onlyNonAlphabetic = "123!@#";
        
        // Act
        var result = onlyNonAlphabetic.StripNonChars();
        
        // Assert
        result.Should().Be("");
    }
    
    [Test]
    public void StripNonChars_WithNull_ReturnsEmptyString()
    {
        // Arrange
        string? nullString = null;
        
        // Act
        var result = nullString.StripNonChars();
        
        // Assert
        result.Should().Be("");
    }
    
    [Test]
    public void SingleLineListWithAnd_WithMultipleItems_FormatsCorrectly()
    {
        // Arrange
        var nouns = new List<string> { "sword", "diamond", "pile of leaves" };
        
        // Act
        var result = nouns.SingleLineListWithAnd();
        
        // Assert
        result.Should().Be("a sword, a diamond and a pile of leaves");
    }
    
    [Test]
    public void SingleLineListWithAnd_WithSingleItem_ReturnsSingleItemWithArticle()
    {
        // Arrange
        var nouns = new List<string> { "sword" };
        
        // Act
        var result = nouns.SingleLineListWithAnd();
        
        // Assert
        result.Should().Be("a sword");
    }
    
    [Test]
    public void SingleLineListWithAnd_WithTwoItems_FormatsCorrectly()
    {
        // Arrange
        var nouns = new List<string> { "sword", "shield" };
        
        // Act
        var result = nouns.SingleLineListWithAnd();
        
        // Assert
        result.Should().Be("a sword and a shield");
    }
    
    [Test]
    public void SingleLineListWithAnd_WithEmptyList_ThrowsException()
    {
        // Arrange
        var nouns = new List<string>();
        
        // Act & Assert
        Action action = () => nouns.SingleLineListWithAnd();
        
        // The method tries to access elements of an empty list which would throw an exception
        action.Should().Throw<InvalidOperationException>();
    }
    
    [Test]
    public void SingleLineListWithAndNoArticle_WithMultipleItems_FormatsCorrectlyWithoutArticles()
    {
        // Arrange
        var nouns = new List<string> { "sword", "diamond", "pile of leaves" };
        
        // Act
        var result = nouns.SingleLineListWithAndNoArticle();
        
        // Assert
        result.Should().Be("sword, diamond and pile of leaves");
    }
    
    [Test]
    public void SingleLineListWithAndNoArticle_WithTwoItems_FormatsCorrectlyWithoutArticles()
    {
        // Arrange
        var nouns = new List<string> { "sword", "shield" };
        
        // Act
        var result = nouns.SingleLineListWithAndNoArticle();
        
        // Assert
        result.Should().Be("sword and shield");
    }
    
    [Test]
    public void SingleLineListWithAndNoArticle_WithSingleItem_ReturnsSingleItemWithoutArticle()
    {
        // Arrange
        var nouns = new List<string> { "sword" };
        
        // Act
        var result = nouns.SingleLineListWithAndNoArticle();
        
        // Assert
        result.Should().Be("sword");
    }
    
    [Test]
    public void SingleLineListWithAndNoArticle_WithEmptyList_ThrowsException()
    {
        // Arrange
        var nouns = new List<string>();
        
        // Act & Assert
        Action action = () => nouns.SingleLineListWithAndNoArticle();
        
        // The method would try to access elements of an empty list which would throw an exception
        action.Should().Throw<InvalidOperationException>();
    }
    
    [Test]
    public void SingleLineListWithOr_WithMultipleItems_FormatsCorrectlyWithOr()
    {
        // Arrange
        var nouns = new List<string> { "brass lantern", "green lantern", "useless lantern" };
        
        // Act
        var result = nouns.SingleLineListWithOr();
        
        // Assert
        result.Should().Be("the brass lantern, the green lantern or the useless lantern");
    }
    
    [Test]
    public void SingleLineListWithOr_WithTwoItems_FormatsCorrectlyWithOr()
    {
        // Arrange
        var nouns = new List<string> { "brass lantern", "green lantern" };
        
        // Act
        var result = nouns.SingleLineListWithOr();
        
        // Assert
        result.Should().Be("the brass lantern or the green lantern");
    }
    
    [Test]
    public void SingleLineListWithOr_WithSingleItem_ReturnsSingleItemWithArticle()
    {
        // Arrange
        var nouns = new List<string> { "brass lantern" };
        
        // Act
        var result = nouns.SingleLineListWithOr();
        
        // Assert
        result.Should().Be("the brass lantern");
    }
    
    [Test]
    public void SingleLineListWithOr_WithEmptyList_ThrowsException()
    {
        // Arrange
        var nouns = new List<string>();
        
        // Act & Assert
        Action action = () => nouns.SingleLineListWithOr();
        
        // The method would try to access elements of an empty list which would throw an exception
        action.Should().Throw<InvalidOperationException>();
    }
}