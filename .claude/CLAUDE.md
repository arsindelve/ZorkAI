# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# ZorkAI - Text Adventure Game Engine with AI Integration

## Project Overview
This is a sophisticated C# .NET 8.0 recreation of classic Infocom-style text adventure games (like Zork) with modern AI integration. The engine can run multiple games including Zork I and Planetfall.

## Essential Development Commands

### Building and Testing
- **Build entire solution**: `dotnet build`
- **Run all tests**: `dotnet test`
- **Run tests for specific project**: `dotnet test UnitTests` or `dotnet test ZorkOne.Tests`
- **Run specific test class**: `dotnet test --filter "TestClass=ContextTests"`
- **Run specific test**: `dotnet test --filter "BlatherTests.ThePlayer_TriesToBlather_BlatherIsAlive"`
- **Check .NET version**: `dotnet --version` (requires .NET 8.0+)

### Test Projects Structure
- **UnitTests**: Core engine tests (667+ comprehensive tests)
  - IntentEngine/: Decision engine tests (GiveSomethingToSomeone, Move, etc.)
  - AWS/: DynamoDb and CloudWatch logic tests
  - Engine/: Core game engine tests
  - GlobalCommands/, SingleNounProcessors/, MultiNounProcessors/: Command processor tests
- **ZorkOne.Tests**: Game-specific tests for Zork I
- **Planetfall.Tests**: Game-specific tests for Planetfall
- **Lambda.Tests**: AWS Lambda API tests
- **Planetfall-Lambda.Tests**: Planetfall Lambda API tests
- **IntegrationTests**: Cross-service integration tests (marked [Explicit], require AWS credentials)

## Key Architecture Components

### Core Game Engine (`GameEngine/`)
- **Context.cs**: Game state management - inventory, scoring, moves, light sources, actors
- **IntentParser.cs**: Parses user input into game intents (move, interact, system commands)
- **Repository.cs**: Singleton pattern for all game items/locations - lazy-loaded, prevents duplicates
- **GameEngine.cs**: Main game loop orchestration
- **ConversationHandler.cs**: Detects and processes conversations between player and NPCs (ICanBeTalkedTo entities)
- **SentenceSplitter.cs**: Enables multi-sentence commands (e.g., "take lamp. go north. look")

### Intent Processing (`GameEngine/IntentEngine/`)
- **SimpleInteractionEngine.cs**: Handles item interactions (take, use, examine)
- **MoveEngine.cs**: Movement between locations with weight limits and generation
- **GiveSomethingToSomeoneDecisionEngine.cs**: Complex multi-object interactions

### AI Integration (`Model/AIParsing/`, `Model/AIGeneration/`, `ChatLambda/`)
- **OpenAI integration** for parsing complex natural language commands
- **AWS Bedrock support** for text generation
- **ChatLambda**: Conversation parsing service that determines if player input is conversational vs command-based
- **Smart fallbacks** - tries simple parsing first, then AI if needed
- **Generated narrative responses** for failed actions
- **IParseConversation**: Interface for detecting and rewriting conversational input for NPC interactions

### Game Content
- **ZorkOne/**: Complete Zork I implementation with all locations/items
- **Planetfall/**: Space-themed adventure game
- **Extensible** - new games inherit from base engine

## Solution Structure
The solution is organized into logical groups:
- **Games folder**: ZorkOne/, Planetfall/, ZorkTwo/ - each game with its own implementation, tests, web client, and Lambda
- **AWS folder**: Cloud services (DynamoDb/, SecretsManager/, CloudWatch/, Bedrock/)
- **Core libraries**: GameEngine/, Model/, OpenAI/, Utilities/, ChatLambda/
- **Lambda APIs**: Individual Lambda functions for each game deployment
  - `Lambda/` - Zork One Lambda API
  - `Planetfall-Lambda/` - Planetfall Lambda API
- **Web clients**: React/TypeScript SPAs for browser gameplay
  - `zorkweb.client/` - Zork One web interface
  - `planetfallweb.client/` - Planetfall web interface
  - `ZorkWeb.Server/` - ASP.NET Core server for web clients

## Technical Highlights

### Smart Repository Pattern
- All items/locations are singletons managed by Repository
- **Lazy loading** - items instantiated only when needed
- **No duplicates** - ensures game state consistency
- **Cross-location items** (like windows) maintain state

### AI-Enhanced Parsing
- **Hierarchical parsing**: System commands → Global commands → AI parsing
- **Context-aware**: AI gets location description for better understanding
- **Fallback strategy**: Simple regex first, then expensive AI calls
- **Logging integration** with CloudWatch for monitoring

### Light Source System
- **Complex visibility logic** - inventory, room, containers
- **Multiple light types** - constant, toggleable, containers
- **Critical for gameplay** - affects what actions are possible

### Movement System
- **Weight restrictions** per location (tight squeezes)
- **Dynamic generation** - 20% chance of AI-generated "can't go" messages
- **Custom failure messages** for specific blocked paths

### Conversation System
- **ICanBeTalkedTo interface** - NPCs and items can engage in AI-powered conversations
- **ConversationHandler** - Detects conversational input and routes to appropriate character
- **Multi-character support** - Automatically finds the target character based on noun matching
- **Context-aware responses** - Characters like Floyd use AI to respond naturally while maintaining personality
- **Dual-mode parsing** - ChatLambda determines if input is conversational or command-based

### Multi-Sentence Command Support
- **Period-separated commands** - Players can chain actions: "take lamp. turn it on. go north"
- **Sequential processing** - Each sentence processed in order with state maintained between them
- **Smart interruption** - Stops processing if a command requires user input (save, disambiguation)
- **Individual responses** - Each command gets its own response, combined with double newlines

## Game Flow
1. **User input** → IntentParser → Intent object
2. **Intent routing** to appropriate engine (Move, Interaction, etc.)
3. **State changes** through Repository and Context
4. **Response generation** - static or AI-generated text
5. **Turn processing** - actors act, state updates

## Testing Strategy Learned
- **Focus on core functionality** over edge cases
- **Use real game objects** instead of complex mocks
- **Repository integration tests** work better than isolated unit tests
- **Avoid brittle random generation mocking**

## Key Patterns
- **Command Pattern**: Intents represent user actions
- **Strategy Pattern**: Different engines handle different intent types
- **Singleton Repository**: Central game state management
- **Factory Pattern**: Item and location creation
- **Template Method**: Base classes for items/locations with game-specific overrides

## Performance Considerations
- **AI calls are expensive** - cache when possible, use sparingly
- **Repository lazy loading** prevents memory bloat
- **Static analysis** preferred over AI for simple commands

## Games Implemented
- **Zork I**: Classic underground adventure, treasure hunting
- **Planetfall**: Space station survival/exploration
- **Extensible framework** for new games

This is a production-quality game engine that successfully combines classic text adventure mechanics with modern AI capabilities for enhanced natural language understanding and dynamic content generation.

## Recent Testing & Development Notes

### Comprehensive Test Coverage Achieved (667+ tests, 100% passing)
- **Context.cs**: 26 tests covering inventory, scoring, item searching, game state
- **IntentParser.cs**: 22 tests covering command parsing, AI integration, logging
- **SimpleInteractionEngine.cs**: 10 tests covering item interactions with real Repository
- **MoveEngine.cs**: Core movement logic, weight validation, failure scenarios
- **GiveSomethingToSomeoneDecisionEngine.cs**: 18 tests covering verb matching, noun ordering, item validation, NPC interactions
- **DynamoDb repositories**: 6 logic tests + 10 [Explicit] integration tests for AWS persistence
- **CloudWatch logging**: 13 tests covering serialization, correlation IDs, interface contracts

### Key Testing Insights
- **Real object integration** works better than complex mocking for this architecture
- **Repository pattern** makes testing more realistic - use actual game items in tests
- **Focus on core business logic** rather than edge cases with complex dependencies
- **AI integration testing** requires environment setup but validates critical paths

### Code Quality Observations
- **Excellent separation of concerns** - each engine handles specific intent types
- **Thoughtful error handling** - graceful degradation when AI services unavailable
- **Performance-conscious design** - AI calls only when simpler parsing fails
- **Extensible architecture** - new games can reuse entire engine infrastructure

### Design Philosophy
This project exemplifies **thoughtful AI integration** - using AI to enhance rather than replace carefully designed game mechanics. The hierarchical parsing approach (simple → complex → AI) shows understanding of both performance and user experience considerations.

The codebase demonstrates production-level C# development with proper dependency injection, logging, monitoring, and testing practices while maintaining the essential character of classic text adventures.

## AWS Lambda Integration & Deployment

### Lambda Project Structure (`Lambda/src/Lambda/`)
- **ZorkOneController.cs**: RESTful API endpoints for game interaction
  - `POST /` - Main game interaction endpoint
  - `GET /` - Session restoration and game history
  - `POST /restoreGame` - Load saved games
  - `POST /saveGame` - Persist game state
  - `GET /saveGame` - List all saved games
- **LambdaEntryPoint.cs**: AWS Lambda function entry point extending `APIGatewayProxyFunction`
- **Startup.cs**: ASP.NET Core configuration for Lambda deployment

### Web API Design Patterns
- **Base64 encoding** for game state serialization/deserialization
- **Session management** with DynamoDB integration for state persistence
- **Error handling** with meaningful HTTP status codes and messages
- **AI integration** for enhanced narrative responses during save/restore operations

### Lambda Testing Strategy (26 comprehensive tests)
- **Controller testing** with full dependency injection mocking
- **Request/Response model validation** ensuring correct parameter ordering
- **Session repository integration** testing state persistence workflows
- **AWS service mocking** for offline development and testing
- **Parameter validation** - critical for API contract correctness

### Key Lambda Implementation Insights
- **Constructor parameter ordering** is critical for record types - easy source of bugs
- **Mock setup completeness** - all repository methods must be properly configured
- **State management complexity** - games maintain rich state requiring careful serialization
- **AI integration consistency** - same generation patterns used in console and web versions

### API Architecture Benefits
- **Stateless design** - each request contains full context needed
- **Scalable deployment** - Lambda auto-scaling handles traffic spikes
- **Cost-effective** - pay-per-request model ideal for text adventures
- **Multi-channel support** - same engine serves console, web, and API consumers

### Production Deployment Considerations
- **Session state size** - Base64 encoding adds ~33% overhead to serialized game state
- **AI service timeouts** - generation requests need proper timeout handling
- **DynamoDB performance** - session retrieval patterns optimized for single-table design
- **Error resilience** - graceful degradation when AI services unavailable

This Lambda integration demonstrates how the core game engine's excellent architecture enables seamless deployment across multiple platforms while maintaining consistent gameplay experience and leveraging cloud services for scalability and persistence.

## Development Best Practices for This Codebase

### Testing Philosophy
- **Use real Repository objects** in tests rather than mocking - the Repository pattern works better with integration-style testing
- **Load items into Repository first**: Call `Repository.GetItem<ItemType>()` before testing to ensure items are available for lookups
- **Test core business logic** rather than focusing on edge cases with complex dependencies
- **AI integration tests** require environment setup but validate critical user experience paths
- **All tests must be deterministic** - never allow randomness in unit tests
- **Always mock IRandomChooser** - see Randomness Pattern section below
- **Nested TestFixtures** - organize tests by method/scenario for better readability and maintainability

### Working with the Repository Pattern
- Items and locations are singletons - calling `Repository.GetItem<Lamp>()` always returns the same instance
- **Lazy loading**: Items are only created when first requested, improving memory efficiency
- **State consistency**: Since items are singletons, state changes persist across the entire game session
- Use `Repository.GetItem<T>()` and `Repository.GetLocation<T>()` to access game objects
- **Critical**: Always call `Repository.Reset()` in test `[SetUp]` to ensure clean state between tests
- **Item lookups**: `Repository.GetItem(string noun)` searches all loaded items by their `NounsForMatching` property

### Randomness Pattern (IRandomChooser)
- **Never use `Random` directly** in game code - always use `IRandomChooser` interface
- **Interface location**: `Model/Interface/IRandomChooser.cs`
- **Production implementation**: `GameEngine/RandomChooser.cs`
- **Available methods**:
  - `Choose<T>(List<T> items)` - select random item from list
  - `RollDice(int sides)` - returns 1 to sides (inclusive)
  - `RollDiceSuccess(int sides)` - returns true if roll equals 1
- **Adding randomness to a class**:
  ```csharp
  [UsedImplicitly] [JsonIgnore]
  public IRandomChooser Chooser { get; set; } = new RandomChooser();
  ```
- **Mocking in tests** (required for all tests with randomness):
  ```csharp
  var mockChooser = new Mock<IRandomChooser>();
  mockChooser.Setup(r => r.RollDice(100)).Returns(1); // Force specific outcome
  mockChooser.Setup(r => r.Choose(It.IsAny<List<string>>())).Returns("specific item");
  GetItem<MyItem>().Chooser = mockChooser.Object;
  ```
- **Examples**: See `Laser.cs`, `Floyd.cs`, `SleepEngine.cs` for usage patterns

### AI Integration Guidelines  
- **Hierarchical parsing**: Try simple pattern matching first, then fall back to expensive AI calls
- **Context matters**: AI parsers receive location descriptions to better understand commands
- **Performance**: Cache AI responses when possible, avoid unnecessary API calls
- **Graceful degradation**: System should work even when AI services are unavailable

### Testing AWS Components (DynamoDb, CloudWatch)
- **Production architecture**: DynamoDbRepositoryBase and CloudWatchLogger create AWS clients directly (not injected)
- **Testing approach**: Test pure logic (GUID generation, data formatting, serialization) without AWS calls
- **Integration tests**: Mark tests requiring AWS credentials with `[Explicit("Requires AWS credentials - Integration test")]`
- **Interface testing**: Production code uses interfaces (ICloudWatchLogger, ISavedGameRepository), test via interface contracts
- **Logic tests**: Focus on testable aspects - date/time conversion, tuple parsing, JSON serialization, string formatting
- Full integration tests exist in IntegrationTests/ project but are marked [Explicit] to prevent CI/CD failures

### Common API Gotchas
- **HasItem<T>()**: Generic method, not `HasItem(IItem item)` - use `troll.HasItem<BloodyAxe>()`
- **Context.Take(IItem)**: Returns void, not bool - don't try to verify return value
- **Context.RemoveItem()**: May be called instead of Take() in some interactions
- **MultiNounIntent.Preposition**: Required property - must be set in initializers
- **ICanContainItems**: Context implements this for RemoveItem() behavior - mock with `mockContext.As<ICanContainItems>()`

### Adding New Tests
1. Follow nested TestFixture pattern for organization (see GiveSomethingToSomeoneDecisionEngineTests.cs)
2. Use FluentAssertions for readable assertions (`.Should().Be()`, `.Should().Contain()`)
3. Name tests with Should_ExpectedBehavior_When_Condition format
4. Always include SetUp/TearDown with Repository.Reset()
5. Test happy path first, then edge cases, then error scenarios

---

## How-To Guides: Practical Development

### Adding a New Item

Items are the building blocks of the game world. Follow these steps to add a new item:

**1. Create the Item Class**

```csharp
// Location: ZorkOne/Item/MagicRing.cs (or appropriate game folder)
using GameEngine.Item;
using Model.Interface;
using Model.Item;

namespace ZorkOne.Item;

public class MagicRing : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeTurnedOn
{
    // Required: Define how players refer to this item
    public override string[] NounsForMatching => ["ring", "magic ring", "gold ring", "jewelry"];

    // Required: Size affects weight restrictions (1-10 typical range)
    public override int Size => 1;

    // ICanBeTakenAndDropped: Description when on ground
    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "A gold ring with mystical engravings lies here. ";
    }

    // ICanBeExamined: Description when examined
    string ICanBeExamined.ExaminationDescription =>
        "The ring glows faintly with an inner light. Ancient runes are carved along its band. ";

    // ICanBeTurnedOn: What happens when activated
    public bool IsOn { get; set; }

    public string OnBeingTurnedOn(IContext context)
    {
        IsOn = true;
        return "The ring begins to glow brightly! ";
    }

    public string OnBeingTurnedOff(IContext context)
    {
        IsOn = false;
        return "The ring's glow fades. ";
    }
}
```

**2. Common Item Interfaces**

Choose interfaces based on item behavior:

- **ICanBeTakenAndDropped** - Portable items (most items)
  - Required: `OnTheGroundDescription(ILocation)` - description when on floor

- **ICanBeExamined** - Items that can be examined
  - Required: `ExaminationDescription` - detailed description

- **ICanBeOpened** - Containers, doors
  - Required: `IsOpen`, `OnBeingOpened(IContext)`

- **ICanBeTurnedOn/Off** - Toggleable items (lamps, machines)
  - Required: `IsOn`, `OnBeingTurnedOn(IContext)`, `OnBeingTurnedOff(IContext)`

- **ILightSource** - Provides illumination in dark areas
  - Required: `IsOn`, `IsLit`, `LightSourceDescription`

- **ICanBeEaten** - Consumable food items
  - Required: `OnEating(IContext)` - returns message, removes item from game

- **ICanBeRead** - Readable items (books, notes)
  - Required: `ReadDescription` - text content

**3. Place Item in Starting Location**

```csharp
// In your game's initialization (e.g., ZorkOne/ZorkI.cs or location class)
var treasureRoom = Repository.GetLocation<TreasureRoom>();
var ring = Repository.GetItem<MagicRing>();
treasureRoom.ItemPlacedHere(ring);
```

**4. Add Custom Behavior (Optional)**

For complex interactions, create an item processor:

```csharp
// GameEngine/Item/ItemProcessor/WearRingProcessor.cs
public class WearRingProcessor : IInteractionProcessor
{
    public Task<InteractionResult?> Process(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (!action.MatchVerb(["wear", "put on"]))
            return Task.FromResult<InteractionResult?>(null);

        var ring = Repository.GetItem<MagicRing>();
        if (!context.Items.Contains(ring))
            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("You don't have the ring! "));

        // Custom wearing logic
        return Task.FromResult<InteractionResult?>(
            new PositiveInteractionResult("You slip the ring onto your finger. It fits perfectly! "));
    }
}
```

**5. Examples to Study**

- **Simple portable**: `ZorkOne/Item/Lunch.cs` - basic item with eating
- **Container**: `ZorkOne/Item/TrophyCase.cs` - holds other items
- **Light source**: `ZorkOne/Item/Lamp.cs` - provides light, can be turned on/off
- **Complex NPC**: `ZorkOne/Item/Troll.cs` - combat, item acceptance, turn-based behavior

---

### Adding a New Command/Verb

Commands allow players to interact with the game world. Here's how to add new verbs:

**1. Add Verb to Verb Collections**

```csharp
// Model/Verbs.cs
public static class Verbs
{
    // Add new verb array or extend existing one
    public static readonly string[] WearVerbs = ["wear", "put on", "don"];
    public static readonly string[] RemoveVerbs = ["remove", "take off", "doff"];

    // Or extend existing array
    public static readonly string[] UseVerbs =
        ["use", "apply", "activate", "press", "push", "wield"]; // added "wield"
}
```

**2. Create a Command Processor**

For **global commands** (work anywhere):

```csharp
// GameEngine/StaticCommand/Implementation/InventoryProcessor.cs
public class InventoryProcessor : IStaticCommandProcessor
{
    public Task<string?> Process(string? input, IContext context, IGenerationClient client)
    {
        // Match command variations
        if (input is null || !input.Trim().Equals("inventory", StringComparison.OrdinalIgnoreCase)
            && !input.Trim().Equals("i", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<string?>(null);

        // Implement logic
        if (!context.Items.Any())
            return Task.FromResult<string?>("You are empty-handed. ");

        var itemList = string.Join(", ", context.Items.Select(i => i.NounsForMatching[0]));
        return Task.FromResult<string?>($"You are carrying: {itemList}. ");
    }
}
```

For **item-specific commands**:

```csharp
// GameEngine/Item/ItemProcessor/ClimbProcessor.cs
public class ClimbProcessor : IInteractionProcessor
{
    public Task<InteractionResult?> Process(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (!action.MatchVerb(["climb", "scale", "ascend"]))
            return Task.FromResult<InteractionResult?>(null);

        // Check if item can be climbed
        var item = Repository.GetItem(action.Noun);
        if (item is not IClimbable climbable)
            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("You can't climb that! "));

        return Task.FromResult<InteractionResult?>(climbable.OnBeingClimbed(context));
    }
}
```

**3. Register the Processor**

Add to appropriate engine in `GameEngine/GameEngine.cs` initialization:

```csharp
// For global commands
_staticCommands.Add(new InventoryProcessor());

// For item interactions
_itemProcessorFactory.RegisterProcessor(new ClimbProcessor());
```

**4. Test the Command**

```csharp
[Test]
public async Task Should_ShowInventory_When_CommandIsI()
{
    Repository.Reset();
    var context = new Context();
    var sword = Repository.GetItem<Sword>();
    context.Items.Add(sword);

    var processor = new InventoryProcessor();
    var result = await processor.Process("i", context, null);

    result.Should().Contain("sword");
}
```

**Examples to Study:**
- **Simple command**: `GameEngine/StaticCommand/Implementation/ScoreProcessor.cs`
- **Complex command**: `GameEngine/StaticCommand/Implementation/GodModeProcessor.cs`
- **Item processor**: `GameEngine/Item/ItemProcessor/TakeProcessor.cs`

---

### Adding a New NPC (Non-Player Character)

NPCs add life to the game world. They can move, fight, hold conversations, and respond to player actions.

**1. Create the NPC Class**

```csharp
// ZorkOne/Item/Wizard.cs
using GameEngine.IntentEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Wizard : ContainerBase,
    ICanBeExamined,
    ITurnBasedActor,
    ICanBeGivenThings,
    ICanBeAttacked,
    ICanHaveConversation
{
    // Identity
    public override string[] NounsForMatching => ["wizard", "mage", "old man"];

    // State tracking
    public bool IsHostile { get; set; }
    public int HitPoints { get; set; } = 10;
    public bool HasSpokenToPlayer { get; set; }

    // Decision engine for accepting items
    private readonly GiveSomethingToSomeoneDecisionEngine<Wizard> _giveEngine = new();

    // Description
    string ICanBeExamined.ExaminationDescription =>
        IsHostile
            ? "An angry wizard glares at you with glowing eyes! "
            : "A kindly old wizard smiles at you. ";

    // Turn-based behavior (called each game turn)
    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (HitPoints <= 0)
            return Task.FromResult(string.Empty);

        // NPC logic here
        if (IsHostile && context.CurrentLocation == CurrentLocation)
        {
            return Task.FromResult("The wizard waves his wand menacingly! ");
        }

        return Task.FromResult(string.Empty);
    }

    // Item acceptance
    InteractionResult ICanBeGivenThings.OfferThisThing(IItem item, IContext context)
    {
        // Accept spell books, reject everything else
        if (item is SpellBook)
        {
            context.RemoveItem(item);
            ItemPlacedHere(item);
            IsHostile = false;
            return new PositiveInteractionResult(
                "The wizard accepts the book gratefully. \"Thank you, traveler!\" ");
        }

        IsHostile = true;
        return new PositiveInteractionResult(
            "The wizard rejects your offering and becomes angry! ");
    }

    // Combat
    public InteractionResult OnBeingAttacked(IItem weapon, IContext context)
    {
        HitPoints -= 2;

        if (HitPoints <= 0)
        {
            CurrentLocation?.RemoveItem(this);
            return new PositiveInteractionResult(
                "The wizard collapses in defeat and vanishes in a puff of smoke! ");
        }

        IsHostile = true;
        return new PositiveInteractionResult(
            "You strike the wizard! He retaliates with a spell! ");
    }

    // Multi-noun interaction handling
    public override async Task<InteractionResult> RespondToMultiNounInteraction(
        MultiNounIntent action, IContext context, IGenerationClient client)
    {
        // Handle "give X to wizard"
        var giveResult = _giveEngine.AreWeGivingSomethingToSomeone(action, this, context);
        if (giveResult is not null)
            return giveResult;

        return await base.RespondToMultiNounInteraction(action, context, client);
    }
}
```

**2. NPC Behavior Patterns**

- **Stationary NPC**: Just implement interfaces, don't move
- **Wandering NPC**: Override `Act()` to change `CurrentLocation` randomly
- **Following NPC**: Track player location in `Act()`, see `Floyd.cs:56-85`
- **Aggressive NPC**: Attack player in `Act()` when in same room, see `Troll.cs:43-67`

**3. Dialogue System (Optional)**

```csharp
// Implement ICanHaveConversation
public string? ParseConversation(string conversation, IContext context,
    IGenerationClient generationClient)
{
    // Simple keyword matching
    if (conversation.Contains("spell", StringComparison.OrdinalIgnoreCase))
        return "I know many spells. Which would you like to learn? ";

    if (conversation.Contains("help", StringComparison.OrdinalIgnoreCase))
        return "I can teach you magic, but you must prove yourself worthy. ";

    // Use AI for open-ended conversation
    return generationClient.GenerateCompanionSpeech(
        companionName: "Wizard",
        companionDescription: "A wise old wizard",
        playerStatement: conversation,
        context: context
    );
}
```

**4. Place NPC in World**

```csharp
var towerTop = Repository.GetLocation<TowerTop>();
var wizard = Repository.GetItem<Wizard>();
towerTop.ItemPlacedHere(wizard);

// Add to actor list for turn processing
context.Actors.Add(wizard);
```

**Examples to Study:**
- **Simple NPC**: `ZorkOne/Item/Cyclops.cs` - stationary, accepts food
- **Combat NPC**: `ZorkOne/Item/Troll.cs` - guards passage, can be defeated
- **Companion NPC**: `Planetfall/Item/Kalamontee/Mech/FloydPart/Floyd.cs` - follows player, has dialogue, complex behavior

---

### Adding a New Location

Locations are the rooms and areas players explore.

**1. Create the Location Class**

```csharp
// ZorkOne/Location/MysticGrove.cs
using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class MysticGrove : LocationBase
{
    // Required: Location name (shown to player)
    public override string Name => "Mystic Grove";

    // Required: Description (shown on entry and with "look")
    public override string Description =>
        "You are in a clearing surrounded by ancient trees. " +
        "Mystical energy fills the air. The trees seem to whisper secrets. ";

    // Optional: Map grouping for multi-area games
    public override string GetMapName() => "Forest";

    // Optional: Custom behaviors
    public override bool IsLit => true; // Always lit (has ambient light)

    // Define exits (return null for blocked directions)
    protected override ILocation? GetNorth() => Repository.GetLocation<DarkForest>();
    protected override ILocation? GetSouth() => Repository.GetLocation<ForestPath>();
    protected override ILocation? GetEast() => null; // Dense trees
    protected override ILocation? GetWest() => null; // Dense trees

    // Optional: Entry restrictions
    public override string? BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        // Check for required item
        var talisman = Repository.GetItem<MagicTalisman>();
        if (!context.Items.Contains(talisman))
            return "A magical barrier prevents your entry! ";

        return null; // null = allow entry
    }

    // Optional: Custom movement messages
    public override string? GetCantGoThatWayMessage(Direction direction, IContext context,
        IGenerationClient? generationClient)
    {
        if (direction is Direction.E or Direction.W)
            return "The ancient trees block your path with their gnarled branches. ";

        return base.GetCantGoThatWayMessage(direction, context, generationClient);
    }
}
```

**2. Special Location Features**

**Dark Location** (requires light source):
```csharp
public override bool IsLit => false; // Player needs lamp/torch

public override string DarknessDescription =>
    "It is pitch black. You are likely to be eaten by a grue. ";
```

**Weighted Location** (size restrictions):
```csharp
public override int WeightLimit => 5; // Only small items fit

public override string GetCantCarryThatMuchMessage(IItem item)
{
    return $"The {item.NounsForMatching[0]} won't fit through the narrow passage! ";
}
```

**SubLocation** (on/in things):
```csharp
// For "get in boat" scenarios
public class InsideBoat : LocationBase, ISubLocation
{
    public ILocation ParentLocation => Repository.GetLocation<LakeShore>();

    public override string Name => "Inside Boat";
    public override string Description => "You are sitting in a small boat. ";
}
```

**3. Connect to Existing Locations**

Update adjacent locations to point back:

```csharp
// In ForestPath.cs
protected override ILocation? GetNorth() => Repository.GetLocation<MysticGrove>();
```

**4. Initialize with Items**

```csharp
// In game initialization
var grove = Repository.GetLocation<MysticGrove>();
var staff = Repository.GetItem<MagicStaff>();
var potion = Repository.GetItem<HealingPotion>();

grove.ItemPlacedHere(staff);
grove.ItemPlacedHere(potion);
```

**Examples to Study:**
- **Simple room**: `ZorkOne/Location/WestOfHouse.cs` - basic location with exits
- **Complex room**: `ZorkOne/Location/TrollRoom.cs` - NPC guarding, conditional exits
- **Dark room**: `ZorkOne/Location/MineEntrance.cs` - requires light source
- **Puzzle room**: `ZorkOne/Location/RoundRoom.cs` - special mechanics

---

### Adding Custom Item Interactions

For specialized item behaviors, create interaction processors:

**1. Create Processor Class**

```csharp
// GameEngine/Item/ItemProcessor/PolishProcessor.cs
public class PolishProcessor : IInteractionProcessor
{
    public Task<InteractionResult?> Process(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        // Only handle "polish" verb
        if (!action.MatchVerb(["polish", "shine", "clean", "rub"]))
            return Task.FromResult<InteractionResult?>(null);

        var item = Repository.GetItem(action.Noun);

        // Only specific items can be polished
        if (item is MagicRing ring)
        {
            if (!context.Items.Contains(ring))
                return Task.FromResult<InteractionResult?>(
                    new PositiveInteractionResult("You don't have the ring! "));

            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult(
                    "You polish the ring. It glows more brightly than before! "));
        }

        // Default response for non-polishable items
        return Task.FromResult<InteractionResult?>(
            new PositiveInteractionResult("That doesn't need polishing. "));
    }
}
```

**2. Register Processor**

```csharp
// In ItemProcessorFactory or engine initialization
_processors.Add(new PolishProcessor());
```

**Examples to Study:**
- **Simple processor**: `GameEngine/Item/ItemProcessor/SmellInteractionProcessor.cs`
- **Complex processor**: `GameEngine/Item/ItemProcessor/ClothingOnAndOffProcessor.cs`
- **Multi-item processor**: `GameEngine/IntentEngine/GiveSomethingToSomeoneDecisionEngine.cs`

---

### Quick Reference: Common Tasks

| Task | File Location | Key Method/Property |
|------|---------------|---------------------|
| Add item noun | Item class | `NounsForMatching` override |
| Make item portable | Item class | Implement `ICanBeTakenAndDropped` |
| Add item description | Item class | Implement `ICanBeExamined` |
| Make item light source | Item class | Implement `ILightSource` |
| Add NPC behavior | NPC class | Implement `ITurnBasedActor.Act()` |
| Add dialogue | NPC class | Implement `ICanHaveConversation` |
| Create location | Location class | Inherit `LocationBase`, override exits |
| Block direction | Location class | Return `null` from `GetNorth()` etc |
| Require light | Location class | `IsLit => false` |
| Add verb synonym | `Model/Verbs.cs` | Add to appropriate array |
| Create command | `StaticCommand/Implementation/` | Implement `IStaticCommandProcessor` |
| Custom interaction | `Item/ItemProcessor/` | Implement `IInteractionProcessor` |

---

### Adding New Games
1. Create game-specific folder under solution (following ZorkOne/Planetfall pattern)
2. Implement game-specific items/locations inheriting from base classes
3. Create corresponding test project
4. Add Lambda deployment if needed
5. Web client can reuse core engine through API calls