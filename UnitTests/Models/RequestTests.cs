using FluentAssertions;
using Model.AIGeneration.Requests;
using NUnit.Framework;

namespace UnitTests.Models;

[TestFixture]
public class RequestTests
{
    [Test]
    public void AskedForCurrentTimeRequest_DefaultConstructor_SetsUserMessageCorrectly()
    {
        // Act
        var request = new AskedForCurrentTimeRequest();
        
        // Assert
        request.UserMessage.Should().Contain("The player asked what time it is");
        request.UserMessage.Should().Contain("that has no meaning in this game");
        request.UserMessage.Should().Contain("succinct but sarcastic response");
    }
    
    [Test]
    public void AskedForCurrentTimeRequest_DefaultConstructor_MessageShouldNotBeEmpty()
    {
        // Act
        var request = new AskedForCurrentTimeRequest();
        
        // Assert
        request.UserMessage.Should().NotBeNullOrEmpty();
    }
    
    [Test]
    public void CannotGoThatWayRequest_Constructor_SetsUserMessageCorrectly()
    {
        // Arrange
        string location = "Forest";
        string direction = "north";
        
        // Act
        var request = new CannotGoThatWayRequest(location, direction);
        
        // Assert
        request.UserMessage.Should().Contain($"The player is in this location: \"{location}\"");
        request.UserMessage.Should().Contain($"They tried to go {direction}");
        request.UserMessage.Should().Contain("Respond with a very short, sarcastic and simple message");
        request.UserMessage.Should().Contain("cannot go that way");
    }
    
    [Test]
    public void CannotGoThatWayRequest_Constructor_WithDifferentParameters_SetsUserMessageCorrectly()
    {
        // Arrange
        string location = "Castle";
        string direction = "east";
        
        // Act
        var request = new CannotGoThatWayRequest(location, direction);
        
        // Assert
        request.UserMessage.Should().Contain($"The player is in this location: \"{location}\"");
        request.UserMessage.Should().Contain($"They tried to go {direction}");
        request.UserMessage.Should().Contain("not possible from this location");
        request.UserMessage.Should().Contain("Do not be creative about why or what is preventing them");
        request.UserMessage.Should().Contain("do not alter the state of the game or provide additional information");
    }
    
    [Test]
    public void CannotGoThatWayRequest_Constructor_LocationWithSpecialCharacters_SetsUserMessageCorrectly()
    {
        // Arrange
        string location = "Wizard's Tower - 3rd Floor";
        string direction = "up";
        
        // Act
        var request = new CannotGoThatWayRequest(location, direction);
        
        // Assert
        request.UserMessage.Should().Contain($"The player is in this location: \"{location}\"");
        request.UserMessage.Should().Contain($"They tried to go {direction}");
        request.UserMessage.Should().NotBeNullOrEmpty();
    }
    
    [Test]
    public void AfterRestoreGameRequest_Constructor_SetsUserMessageCorrectly()
    {
        // Arrange
        string game = "Zork";
        
        // Act
        var request = new AfterRestoreGameRequest(game);
        
        // Assert
        request.UserMessage.Should().Contain($"The adventurer has restored their game from a previous saved game and is now in this location: \"{game}.\"");
        request.UserMessage.Should().Contain("Tell them in a funny sentence or two that their game restored successfully");
        request.UserMessage.Should().Contain("wish them better luck this time");
    }
    
    [Test]
    public void AfterRestoreGameRequest_Constructor_WithDifferentGame_SetsUserMessageCorrectly()
    {
        // Arrange
        string game = "Planetfall";
        
        // Act
        var request = new AfterRestoreGameRequest(game);
        
        // Assert
        request.UserMessage.Should().Contain($"The adventurer has restored their game from a previous saved game and is now in this location: \"{game}.\"");
        request.UserMessage.Should().NotBeNullOrEmpty();
    }
    
    [Test]
    public void AfterRestoreGameRequest_Constructor_WithGameNameWithSpecialCharacters_SetsUserMessageCorrectly()
    {
        // Arrange
        string game = "The Hitchhiker's Guide to the Galaxy";
        
        // Act
        var request = new AfterRestoreGameRequest(game);
        
        // Assert
        request.UserMessage.Should().Contain($"The adventurer has restored their game from a previous saved game and is now in this location: \"{game}.\"");
        request.UserMessage.Should().Contain("Tell them in a funny sentence or two");
        request.UserMessage.Should().NotBeNullOrEmpty();
    }
    
    [Test]
    public void CannotExitSubLocationRequest_Constructor_SetsUserMessageCorrectly()
    {
        // Arrange
        string location = "Forest";
        string subLocationName = "tree house";
        
        // Act
        var request = new CannotExitSubLocationRequest(location, subLocationName);
        
        // Assert
        request.UserMessage.Should().Contain($"The player is in this location: \"{location}\"");
        request.UserMessage.Should().Contain($"They tried to leave some kind of sub-location called {subLocationName}");
        request.UserMessage.Should().Contain("Respond with a very short, sarcastic and simple message");
        request.UserMessage.Should().Contain("cannot do this");
        request.UserMessage.Should().Contain("Do not be creative about why");
    }
    
    [Test]
    public void CannotExitSubLocationRequest_Constructor_WithDifferentParameters_SetsUserMessageCorrectly()
    {
        // Arrange
        string location = "Castle";
        string subLocationName = "throne";
        
        // Act
        var request = new CannotExitSubLocationRequest(location, subLocationName);
        
        // Assert
        request.UserMessage.Should().Contain($"The player is in this location: \"{location}\"");
        request.UserMessage.Should().Contain($"They tried to leave some kind of sub-location called {subLocationName}");
        request.UserMessage.Should().Contain("do not alter the state of the game or provide additional information");
    }
    
    [Test]
    public void CannotExitSubLocationRequest_Constructor_WithSpecialCharacters_SetsUserMessageCorrectly()
    {
        // Arrange
        string location = "Wizard's Tower - 3rd Floor";
        string subLocationName = "magical portal";
        
        // Act
        var request = new CannotExitSubLocationRequest(location, subLocationName);
        
        // Assert
        request.UserMessage.Should().Contain($"The player is in this location: \"{location}\"");
        request.UserMessage.Should().Contain($"They tried to leave some kind of sub-location called {subLocationName}");
        request.UserMessage.Should().NotBeNullOrEmpty();
    }
    
    [Test]
    public void CannotEnterSubLocationRequest_Constructor_SetsUserMessageCorrectly()
    {
        // Arrange
        string location = "Forest";
        string subLocationName = "cave";
        
        // Act
        var request = new CannotEnterSubLocationRequest(location, subLocationName);
        
        // Assert
        request.UserMessage.Should().Contain($"The player is in this location: \"{location}\"");
        request.UserMessage.Should().Contain($"They tried to enter some kind of sub-location called {subLocationName}");
        request.UserMessage.Should().Contain("that is not available from this location");
        request.UserMessage.Should().Contain("Respond with a very short, sarcastic and simple message");
        request.UserMessage.Should().Contain("cannot do this");
    }
    
    [Test]
    public void CannotEnterSubLocationRequest_Constructor_WithDifferentParameters_SetsUserMessageCorrectly()
    {
        // Arrange
        string location = "Castle";
        string subLocationName = "secret passage";
        
        // Act
        var request = new CannotEnterSubLocationRequest(location, subLocationName);
        
        // Assert
        request.UserMessage.Should().Contain($"The player is in this location: \"{location}\"");
        request.UserMessage.Should().Contain($"They tried to enter some kind of sub-location called {subLocationName}");
        request.UserMessage.Should().Contain("Do not be creative about why");
        request.UserMessage.Should().Contain("do not alter the state of the game or provide additional information");
    }
    
    [Test]
    public void CannotEnterSubLocationRequest_Constructor_WithSpecialCharacters_SetsUserMessageCorrectly()
    {
        // Arrange
        string location = "Wizard's Tower - 3rd Floor";
        string subLocationName = "dragon's lair";
        
        // Act
        var request = new CannotEnterSubLocationRequest(location, subLocationName);
        
        // Assert
        request.UserMessage.Should().Contain($"The player is in this location: \"{location}\"");
        request.UserMessage.Should().Contain($"They tried to enter some kind of sub-location called {subLocationName}");
        request.UserMessage.Should().NotBeNullOrEmpty();
    }
    
    [Test]
    public void DropSomethingTheyDoNotHave_Constructor_SetsUserMessageCorrectly()
    {
        // Arrange
        string input = "drop sword";
        
        // Act
        var request = new DropSomethingTheyDoNotHave(input);
        
        // Assert
        request.UserMessage.Should().Contain($"The player said '{input}'");
        request.UserMessage.Should().Contain("but the thing or things they asked to drop is/are not in their inventory");
        request.UserMessage.Should().Contain("Provide the narrator's very succinct but sarcastic response");
    }
    
    [Test]
    public void DropSomethingTheyDoNotHave_Constructor_WithDifferentInput_SetsUserMessageCorrectly()
    {
        // Arrange
        string input = "drop magic wand";
        
        // Act
        var request = new DropSomethingTheyDoNotHave(input);
        
        // Assert
        request.UserMessage.Should().Contain($"The player said '{input}'");
        request.UserMessage.Should().Contain("not in their inventory");
        request.UserMessage.Should().NotBeNullOrEmpty();
    }
    
    [Test]
    public void DropSomethingTheyDoNotHave_Constructor_WithSpecialCharacters_SetsUserMessageCorrectly()
    {
        // Arrange
        string input = "drop king's crown";
        
        // Act
        var request = new DropSomethingTheyDoNotHave(input);
        
        // Assert
        request.UserMessage.Should().Contain($"The player said '{input}'");
        request.UserMessage.Should().Contain("sarcastic response");
        request.UserMessage.Should().NotBeNullOrEmpty();
    }

}
