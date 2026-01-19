# Creating a New Game with API Controller

This guide provides exhaustive detail on creating a new text adventure game in the ZorkAI framework, including an AWS Lambda API controller. This was learned from building the EscapeRoom tutorial game.

## Table of Contents

1. [Overview](#overview)
2. [Project Structure](#project-structure)
3. [Phase 1: Game Project Infrastructure](#phase-1-game-project-infrastructure)
4. [Phase 2: Locations](#phase-2-locations)
5. [Phase 3: Items](#phase-3-items)
6. [Phase 4: Special Mechanics](#phase-4-special-mechanics)
7. [Phase 5: Lambda Controller](#phase-5-lambda-controller)
8. [Phase 6: Tests](#phase-6-tests)
9. [Phase 7: Solution Integration](#phase-7-solution-integration)
10. [Phase 8: AWS Resources](#phase-8-aws-resources)
11. [Testing and Verification](#testing-and-verification)
12. [Common Patterns and Interfaces](#common-patterns-and-interfaces)
13. [Troubleshooting](#troubleshooting)

---

## Overview

A complete game consists of:
- **Game Project** (`YourGame/`) - Core game logic, locations, items
- **Lambda Project** (`YourGame-Lambda/`) - AWS Lambda API controller
- **Test Project** (`YourGame.Tests/`) - Unit and integration tests

The game integrates with the existing `GameEngine` and `Model` libraries which provide:
- Command parsing and intent processing
- Item and location base classes
- Context/state management
- AI integration for natural language processing
- Movement, inventory, and interaction systems

---

## Project Structure

```
ZorkAI/
├── YourGame/
│   ├── YourGame.csproj
│   ├── YourGameGame.cs              # IGameSpecificComponents implementation
│   ├── YourGameContext.cs           # Game-specific context
│   ├── GlobalUsings.cs
│   ├── GlobalCommand/
│   │   └── YourGameGlobalCommandFactory.cs
│   ├── Location/
│   │   ├── StartingRoom.cs
│   │   ├── Room2.cs
│   │   └── ...
│   ├── Item/
│   │   ├── Item1.cs
│   │   ├── Item2.cs
│   │   └── ...
│   └── Command/                     # Optional: custom processors
│       └── DeathProcessor.cs
│
├── YourGame-Lambda/
│   └── src/
│       └── YourGame-Lambda/
│           ├── YourGame-Lambda.csproj
│           ├── Startup.cs
│           ├── LambdaEntryPoint.cs
│           ├── LocalEntryPoint.cs
│           └── Controllers/
│               └── YourGameController.cs
│
└── YourGame.Tests/
    ├── YourGame.Tests.csproj
    ├── GlobalUsings.cs
    ├── WalkthroughTests.cs
    ├── LocationTests.cs
    └── ItemTests.cs
```

---

## Phase 1: Game Project Infrastructure

### 1.1 Create Project File

**File: `YourGame/YourGame.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
      <ProjectReference Include="..\GameEngine\GameEngine.csproj" />
      <ProjectReference Include="..\Model\Model.csproj" />
    </ItemGroup>

</Project>
```

### 1.2 Create GlobalUsings

**File: `YourGame/GlobalUsings.cs`**

```csharp
global using GameEngine.Item;
global using YourGame.Item;
global using YourGame.Location;
```

### 1.3 Create Game Context

The context holds all game state: inventory, score, moves, flags, death count, etc.

**File: `YourGame/YourGameContext.cs`**

```csharp
using Game.StaticCommand.Implementation;
using GameEngine;
using Model.Interface;

namespace YourGame;

public class YourGameContext : Context
{
    // Game-specific state flags
    public bool HasWon { get; set; }
    public int DeathCounter { get; set; }

    // Example: custom flags for puzzles
    public bool PuzzleSolved { get; set; }

    // Required overrides for death handling
    public override int GetDeathCount() => DeathCounter;
    public override void SetDeathCount(int count) => DeathCounter = count;

    // Score display (shown on death and win)
    public override string CurrentScore =>
        $"In {Moves} move{(Moves == 1 ? "" : "s")}, you scored {Score} points out of a possible 100.";
}
```

### 1.4 Create Main Game Class

This class tells the engine how to create your game.

**File: `YourGame/YourGameGame.cs`**

```csharp
using GameEngine;
using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.Interface;
using YourGame.GlobalCommand;

namespace YourGame;

public class YourGameGame : IGameSpecificComponents
{
    // The room where the player starts
    public Type StartingLocation => typeof(StartingRoom);

    // Used for DynamoDB table naming
    public string GameName => "YourGame";

    // DynamoDB table for session state
    public string SessionTableName => "yourgame_session";

    // Introduction text shown when game starts
    public string IntroText =>
        "Welcome to Your Game!\n\n" +
        "A text adventure awaits. Type commands like 'look', 'take key', 'go north', etc.\n\n";

    // Factory for game-specific global commands
    public IGlobalCommandFactory GetGlobalCommandFactory(IContext context) =>
        new YourGameGlobalCommandFactory((YourGameContext)context);

    // Create the game context
    public IContext GetContext(IGenerationClient generationClient) =>
        new YourGameContext { Client = generationClient };

    // Optional: Custom item processor (usually just return the default)
    public IItemProcessorFactory GetItemProcessorFactory() => new ItemProcessorFactory();
}
```

### 1.5 Create Global Command Factory

Handles game-wide commands like "score", "inventory", "quit", etc.

**File: `YourGame/GlobalCommand/YourGameGlobalCommandFactory.cs`**

```csharp
using Game.StaticCommand.Implementation;
using GameEngine;
using GameEngine.StaticCommand;
using GameEngine.StaticCommand.Implementation;
using Model.Interface;

namespace YourGame.GlobalCommand;

public class YourGameGlobalCommandFactory : IGlobalCommandFactory
{
    private readonly YourGameContext _context;

    public YourGameGlobalCommandFactory(YourGameContext context)
    {
        _context = context;
    }

    public List<IGlobalCommand> GetGlobalCommands()
    {
        // Standard commands available in all games
        return
        [
            new LookProcessor(),
            new ExitsProcessor(),
            new InventoryProcessor(),
            new DropProcessor(),
            new ScoreProcessor(),
            new DiagnosticProcessor(),
            new TakeAllProcessor(),
            new WaitProcessor(),
            new AgainProcessor(),
            new OpenAndCloseProcessor(),
            new PickUpItemThatWouldFallProcessor()
        ];
    }
}
```

---

## Phase 2: Locations

Locations are the rooms/areas the player explores.

### 2.1 Basic Location Template

**File: `YourGame/Location/StartingRoom.cs`**

```csharp
using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace YourGame.Location;

public class StartingRoom : LocationBase
{
    // Name shown in game (e.g., "Starting Room")
    public override string Name => "Starting Room";

    // Description shown when entering or looking
    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in a simple room. " +
               "A door leads north. " +
               "There's a table in the center of the room.";
    }

    // Define exits - maps directions to destination locations
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<NextRoom>() } }
            // Add more directions as needed:
            // { Direction.S, new MovementParameters { Location = GetLocation<SouthRoom>() } },
            // { Direction.E, new MovementParameters { Location = GetLocation<EastRoom>() } },
            // { Direction.W, new MovementParameters { Location = GetLocation<WestRoom>() } }
        };
    }

    // Place items in this location at game start
    public override void Init()
    {
        StartWithItem<Table>();
        StartWithItem<Key>();
    }
}
```

### 2.2 Dark Location (Requires Light Source)

**File: `YourGame/Location/DarkCellar.cs`**

```csharp
using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace YourGame.Location;

public class DarkCellar : DarkLocationBase  // Note: DarkLocationBase, not LocationBase
{
    public override string Name => "Dark Cellar";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in a musty cellar. Cobwebs hang from the ceiling. " +
               "Stairs lead up to the north.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<StartingRoom>() } }
        };
    }

    public override void Init()
    {
        StartWithItem<TreasureChest>();
    }
}
```

**Important**: Players in a dark location without a light source:
- Cannot see the room description
- Cannot interact with items
- See "It is pitch black. You are likely to be eaten by a grue."

### 2.3 Location with Entry Event (Death Location)

**File: `YourGame/Location/DeathTrap.cs`**

```csharp
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using YourGame.Command;

namespace YourGame.Location;

public class DeathTrap : LocationBase
{
    public override string Name => "Spike Pit";

    protected override string GetContextBasedDescription(IContext context)
    {
        // This is rarely seen since player dies on entry
        return "A pit filled with spikes.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>();
    }

    public override void Init()
    {
        // No items - this is a death location
    }

    // This is called AFTER the player enters the location
    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var deathMessage = "You fall into a pit of spikes! " +
                          "This was clearly a terrible idea.";

        var result = new DeathProcessor().Process(deathMessage, context);

        // IMPORTANT: Return the InteractionMessage, not empty string
        return Task.FromResult(result.InteractionMessage);
    }
}
```

### 2.4 Location with Conditional Exit

**File: `YourGame/Location/LockedRoom.cs`**

```csharp
using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace YourGame.Location;

public class LockedRoom : LocationBase
{
    public override string Name => "Locked Room";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "A secure room. The only exit is south.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // You can add conditional logic here
        var parameters = new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters { Location = GetLocation<StartingRoom>() } }
        };

        // Example: Only allow east exit if puzzle is solved
        if (context is YourGameContext gameContext && gameContext.PuzzleSolved)
        {
            parameters.Add(Direction.E, new MovementParameters { Location = GetLocation<SecretRoom>() });
        }

        return parameters;
    }

    public override void Init() { }
}
```

---

## Phase 3: Items

Items are objects the player can interact with.

### 3.1 Simple Takeable Item

**File: `YourGame/Item/Key.cs`**

```csharp
using GameEngine.Item;
using Model.Interface;

namespace YourGame.Item;

public class Key : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    // Words the player can use to refer to this item
    public override string[] NounsForMatching => ["key", "brass key", "small key"];

    // Item weight (affects carrying capacity)
    public override int Size => 2;

    // ICanBeExamined implementation
    public string ExaminationDescription => "A small brass key with ornate engravings.";

    // ICanBeTakenAndDropped - description when on ground
    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a small brass key here.";
    }
}
```

### 3.2 Item with Points (Scoring)

**File: `YourGame/Item/Treasure.cs`**

```csharp
using GameEngine.Item;
using Model.Interface;

namespace YourGame.Item;

public class Treasure : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["treasure", "gold", "gold coins", "coins"];

    public override int Size => 5;

    // Points awarded when first picked up
    public int NumberOfPoints => 25;

    public string ExaminationDescription => "A pile of glittering gold coins.";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A pile of gold coins glitters here.";
    }
}
```

### 3.3 Light Source

**File: `YourGame/Item/Flashlight.cs`**

```csharp
using GameEngine.Item;
using Model.Interface;

namespace YourGame.Item;

public class Flashlight : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, IAmALightSourceThatTurnsOnAndOff
{
    public override string[] NounsForMatching => ["flashlight", "light", "torch", "lamp"];

    public override int Size => 3;

    public string ExaminationDescription =>
        IsOn ? "A flashlight. It's currently on, providing light."
             : "A flashlight. It's currently off.";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return IsOn ? "There is a flashlight here (providing light)."
                    : "There is a flashlight here.";
    }

    // IAmALightSourceThatTurnsOnAndOff implementation
    public bool IsOn { get; set; }

    public string? OnBeingTurnedOn(IContext context)
    {
        IsOn = true;
        return "The flashlight is now on.";
    }

    public string? OnBeingTurnedOff(IContext context)
    {
        IsOn = false;
        return "The flashlight is now off.";
    }

    // This makes it provide light when on
    public bool IsLit => IsOn;
}
```

### 3.4 Container (Openable with Items Inside)

**File: `YourGame/Item/Chest.cs`**

```csharp
using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace YourGame.Item;

public class Chest : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["chest", "wooden chest", "treasure chest"];

    // Cannot be picked up
    public override string? CannotBeTakenDescription => "The chest is too heavy to carry.";

    public override string Name => "wooden chest";

    // State-aware examination
    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen
            ? Items.Any()
                ? "A wooden chest, now open. You can see something inside."
                : "A wooden chest, now open. It's empty."
            : "A wooden chest with a heavy lid.";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a wooden chest here.";
    }

    // Message shown when opened
    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any()
            ? "Opening the chest reveals its contents."
            : "Opened.";
    }

    // Place items inside at game start
    public override void Init()
    {
        StartWithItemInside<Treasure>();
    }

    // Support "search chest" and "look in chest"
    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["search", "look in", "look inside"], NounsForMatching))
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult(ExaminationDescription));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
```

### 3.5 Readable Item

**File: `YourGame/Item/Note.cs`**

```csharp
using GameEngine.Item;
using Model.Interface;

namespace YourGame.Item;

public class Note : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["note", "paper", "message"];

    public override int Size => 1;

    public string ExaminationDescription => "A folded piece of paper with writing on it.";

    // Content shown when player types "read note"
    public string ReadDescription =>
        "The note reads:\n\n" +
        "\"The key to escape lies beneath the old oak tree.\"\n\n" +
        "It's signed with an illegible scrawl.";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a note on the ground.";
    }
}
```

### 3.6 Lockable Door with Key

**File: `YourGame/Item/ExitDoor.cs`**

```csharp
using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace YourGame.Item;

public class ExitDoor : OpenAndCloseBase, ICanBeExamined, ICannotBeTaken
{
    public override string[] NounsForMatching => ["door", "exit door", "exit", "front door"];

    private bool _isLocked = true;
    private bool _hasAwardedUnlockPoints;

    public string ExaminationDescription =>
        _isLocked
            ? "A heavy door with a brass lock. It appears to be locked."
            : ((IOpenAndClose)this).IsOpen
                ? "The exit door stands open. Freedom awaits!"
                : "The exit door is unlocked but closed.";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a large door here.";
    }

    public override string NeverPickedUpDescription(ILocation? currentLocation)
    {
        return string.Empty; // Don't list separately - mentioned in room description
    }

    public override void Init() { }

    // Handle "open door" when locked
    public override InteractionResult CanBeOpened(IContext context)
    {
        if (_isLocked)
            return new PositiveInteractionResult("The door is locked.");

        return base.CanBeOpened(context);
    }

    // Called when door is opened (after unlock)
    public override string OnOpening(IContext context)
    {
        if (context is YourGameContext gameContext)
        {
            gameContext.HasWon = true;
            gameContext.AddPoints(50);
        }

        return "\n\n*** You have escaped! Congratulations! ***\n\n" +
               context.CurrentScore;
    }

    // Handle "unlock door with key"
    public override Task<InteractionResult?> RespondToMultiNounInteraction(
        MultiNounIntent action, IContext context, IGenerationClient client)
    {
        // Check for "unlock door with key"
        if (action.Match(["unlock"], NounsForMatching, ["with", "using"]))
        {
            var key = Repository.GetItem<Key>();

            // Check if player has the key
            if (!context.HasItem<Key>())
                return Task.FromResult<InteractionResult?>(
                    new PositiveInteractionResult("You don't have the right key."));

            // Check if referencing the correct key
            if (!key.NounsForMatching.Contains(action.NounTwo?.ToLower() ?? ""))
                return Task.FromResult<InteractionResult?>(
                    new PositiveInteractionResult("That won't unlock the door."));

            // Already unlocked?
            if (!_isLocked)
                return Task.FromResult<InteractionResult?>(
                    new PositiveInteractionResult("The door is already unlocked."));

            // Unlock the door
            _isLocked = false;
            AwardUnlockPoints(context);

            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("You unlock the door with the brass key. Click!"));
        }

        return base.RespondToMultiNounInteraction(action, context, client);
    }

    private void AwardUnlockPoints(IContext context)
    {
        if (_hasAwardedUnlockPoints) return;
        _hasAwardedUnlockPoints = true;
        context.AddPoints(10);
    }
}
```

### 3.7 Scenery Item (Cannot Be Taken)

**File: `YourGame/Item/Table.cs`**

```csharp
using GameEngine.Item;
using Model.Interface;

namespace YourGame.Item;

public class Table : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["table", "wooden table"];

    // This prevents the item from being picked up
    public override string? CannotBeTakenDescription => "The table is too heavy to move.";

    public string ExaminationDescription => "A sturdy wooden table.";

    // Don't list in room description (mentioned in room text)
    public override string NeverPickedUpDescription(ILocation? currentLocation)
    {
        return string.Empty;
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a wooden table here.";
    }
}
```

### 3.8 Item with Custom Interaction

**File: `YourGame/Item/Button.cs`**

```csharp
using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace YourGame.Item;

public class Button : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["button", "red button", "big button"];

    public override string? CannotBeTakenDescription => "The button is attached to the wall.";

    public string ExaminationDescription => "A big red button on the wall. It looks tempting to press.";

    public override string NeverPickedUpDescription(ILocation? currentLocation) => string.Empty;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a big red button on the wall.";
    }

    // Handle "push button", "press button"
    public override Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["push", "press", "hit"], NounsForMatching))
        {
            // Do something when button is pressed
            if (context is YourGameContext gameContext)
            {
                gameContext.PuzzleSolved = true;
            }

            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("Click! You hear a mechanism activate somewhere."));
        }

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
```

---

## Phase 4: Special Mechanics

### 4.1 Death Processor

**File: `YourGame/Command/DeathProcessor.cs`**

```csharp
using GameEngine;
using Model.Interface;

namespace YourGame.Command;

public class DeathProcessor
{
    public DeathInteractionResult Process(string deathMessage, IContext context)
    {
        if (context is not YourGameContext gameContext)
            throw new ArgumentException("Context must be YourGameContext");

        gameContext.DeathCounter++;

        var result = deathMessage +
                     "\n\n\t*** You have died ***\n\n" +
                     context.CurrentScore + "\n\n" +
                     "But every adventurer deserves another chance...\n\n";

        var deathResult = new DeathInteractionResult(result, gameContext.DeathCounter);
        gameContext.PendingDeath = deathResult;
        return deathResult;
    }
}
```

### 4.2 Win Condition Handling

Win conditions are typically handled in item interactions (see ExitDoor example above). The key elements:

1. Set a flag in context: `gameContext.HasWon = true`
2. Add final points: `gameContext.AddPoints(50)`
3. Return victory message with score

---

## Phase 5: Lambda Controller

### 5.1 Create Lambda Project File

**File: `YourGame-Lambda/src/YourGame-Lambda/YourGame-Lambda.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <GenerateRuntimeConfigurationFiles>true</GenerateRuntimeConfigurationFiles>
        <AWSProjectType>Lambda</AWSProjectType>
        <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
        <PublishReadyToRun>true</PublishReadyToRun>
    </PropertyGroup>
    <ItemGroup>
        <PackageReference Include="Amazon.Lambda.AspNetCoreServer" Version="9.0.0" />
        <PackageReference Include="Amazon.Lambda.Serialization.SystemTextJson" Version="2.4.3" />
        <PackageReference Include="AWSSDK.SecretsManager" Version="3.7.400.33" />
    </ItemGroup>
    <ItemGroup>
        <ProjectReference Include="..\..\..\Bedrock\Bedrock.csproj" />
        <ProjectReference Include="..\..\..\CloudWatch\CloudWatch.csproj" />
        <ProjectReference Include="..\..\..\DynamoDb\DynamoDb.csproj" />
        <ProjectReference Include="..\..\..\GameEngine\GameEngine.csproj" />
        <ProjectReference Include="..\..\..\Model\Model.csproj" />
        <ProjectReference Include="..\..\..\OpenAI\OpenAI.csproj" />
        <ProjectReference Include="..\..\..\SecretsManager\SecretsManager.csproj" />
        <ProjectReference Include="..\..\..\YourGame\YourGame.csproj" />
    </ItemGroup>
</Project>
```

### 5.2 Create Startup.cs

**File: `YourGame-Lambda/src/YourGame-Lambda/Startup.cs`**

```csharp
namespace YourGame_Lambda;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}
```

### 5.3 Create LambdaEntryPoint.cs

**File: `YourGame-Lambda/src/YourGame-Lambda/LambdaEntryPoint.cs`**

```csharp
using Amazon.Lambda.AspNetCoreServer;

namespace YourGame_Lambda;

public class LambdaEntryPoint : APIGatewayProxyFunction
{
    protected override void Init(IWebHostBuilder builder)
    {
        builder.UseStartup<Startup>();
    }
}
```

### 5.4 Create LocalEntryPoint.cs

**File: `YourGame-Lambda/src/YourGame-Lambda/LocalEntryPoint.cs`**

```csharp
namespace YourGame_Lambda;

public static class LocalEntryPoint
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

### 5.5 Create Controller

**File: `YourGame-Lambda/src/YourGame-Lambda/Controllers/YourGameController.cs`**

```csharp
using System.Text;
using System.Text.Json;
using Bedrock;
using CloudWatch;
using DynamoDb;
using GameEngine;
using Microsoft.AspNetCore.Mvc;
using Model.Interface;
using SecretsManager;
using YourGame;

namespace YourGame_Lambda.Controllers;

[ApiController]
[Route("[controller]")]
public class YourGameController : ControllerBase
{
    private ISessionRepository SessionRepository { get; } =
        new SessionRepository("yourgame_session");

    private ISavedGameRepository SavedGameRepository { get; } =
        new SavedGameRepository("yourgame_savegame");

    private readonly ICloudWatchLogger _cloudWatchLogger = new CloudWatchLogger("yourgame", "game");

    // Main game interaction endpoint
    [HttpPost]
    public async Task<string> Post([FromBody] GameRequest request)
    {
        _cloudWatchLogger.SetCorrelationId(request.SessionId);
        var engine = await GetEngine(request.SessionId);

        // Process the player's input
        var result = await engine.GetResponse(request.Input);

        // Save session state
        await SaveSession(request.SessionId, engine);

        _cloudWatchLogger.Log($"Input: {request.Input}", "info");
        _cloudWatchLogger.Log($"Response: {result}", "info");

        return result;
    }

    // Get session state (for restoring game)
    [HttpGet]
    public async Task<GameResponse> Get([FromQuery] string sessionId)
    {
        _cloudWatchLogger.SetCorrelationId(sessionId);
        var engine = await GetEngine(sessionId);

        // Check if this is a new game
        if (string.IsNullOrEmpty(await SessionRepository.GetSession(sessionId)))
        {
            // New game - return intro
            var intro = await engine.GetIntroduction();
            await SaveSession(sessionId, engine);

            return new GameResponse
            {
                Response = intro,
                History = new List<HistoryItem>()
            };
        }

        // Existing game - just return acknowledgment
        return new GameResponse
        {
            Response = "",
            History = new List<HistoryItem>()
        };
    }

    // Save game
    [HttpPost("saveGame")]
    public async Task<string> SaveGame([FromBody] SaveGameRequest request)
    {
        var engine = await GetEngine(request.SessionId);
        var stateJson = JsonSerializer.Serialize(engine.Context);
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(stateJson));

        await SavedGameRepository.SaveGame(request.SaveName, encoded);

        return $"Game saved as '{request.SaveName}'.";
    }

    // Restore game
    [HttpPost("restoreGame")]
    public async Task<string> RestoreGame([FromBody] RestoreGameRequest request)
    {
        var savedState = await SavedGameRepository.GetSavedGame(request.SaveName);
        if (string.IsNullOrEmpty(savedState))
            return $"No saved game found with name '{request.SaveName}'.";

        // Store in session
        await SessionRepository.StoreSession(request.SessionId, savedState);

        return $"Game '{request.SaveName}' restored.";
    }

    // List saved games
    [HttpGet("saveGame")]
    public async Task<List<string>> GetSavedGames()
    {
        return await SavedGameRepository.GetSavedGames();
    }

    private async Task<GameEngine<YourGameContext>> GetEngine(string sessionId)
    {
        var secrets = new Secrets();
        var prompt = await secrets.GetSecret("YourGamePrompt");
        var generationClient = new BedrockClient(prompt);

        var game = new YourGameGame();
        var context = (YourGameContext)game.GetContext(generationClient);
        var engine = new GameEngine<YourGameContext>(game, context);

        // Load existing session if available
        var existingState = await SessionRepository.GetSession(sessionId);
        if (!string.IsNullOrEmpty(existingState))
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(existingState));
            var loadedContext = JsonSerializer.Deserialize<YourGameContext>(decoded);
            if (loadedContext != null)
            {
                loadedContext.Client = generationClient;
                engine = new GameEngine<YourGameContext>(game, loadedContext);
            }
        }

        return engine;
    }

    private async Task SaveSession(string sessionId, GameEngine<YourGameContext> engine)
    {
        var stateJson = JsonSerializer.Serialize(engine.Context);
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(stateJson));
        await SessionRepository.StoreSession(sessionId, encoded);
    }
}

// Request/Response models
public record GameRequest(string Input, string SessionId);
public record GameResponse
{
    public string Response { get; init; } = "";
    public List<HistoryItem> History { get; init; } = new();
}
public record HistoryItem(string Input, string Output);
public record SaveGameRequest(string SessionId, string SaveName);
public record RestoreGameRequest(string SessionId, string SaveName);
```

---

## Phase 6: Tests

### 6.1 Create Test Project

**File: `YourGame.Tests/YourGame.Tests.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="coverlet.collector" Version="6.0.2"/>
        <PackageReference Include="FluentAssertions" Version="7.0.0"/>
        <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0"/>
        <PackageReference Include="Moq" Version="4.20.72"/>
        <PackageReference Include="NUnit" Version="4.2.2"/>
        <PackageReference Include="NUnit.Analyzers" Version="4.4.0"/>
        <PackageReference Include="NUnit3TestAdapter" Version="4.6.0"/>
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\GameEngine\GameEngine.csproj"/>
        <ProjectReference Include="..\Model\Model.csproj"/>
        <ProjectReference Include="..\YourGame\YourGame.csproj"/>
    </ItemGroup>

</Project>
```

### 6.2 Create GlobalUsings

**File: `YourGame.Tests/GlobalUsings.cs`**

```csharp
global using NUnit.Framework;
global using FluentAssertions;
global using GameEngine;
global using YourGame;
global using YourGame.Item;
global using YourGame.Location;
```

### 6.3 Create Walkthrough Test

**File: `YourGame.Tests/WalkthroughTests.cs`**

```csharp
namespace YourGame.Tests;

[TestFixture]
public class WalkthroughTests
{
    private GameEngine<YourGameContext> _engine = null!;
    private YourGameContext _context = null!;

    [SetUp]
    public void Setup()
    {
        Repository.Reset();
        var game = new YourGameGame();
        _context = (YourGameContext)game.GetContext(null!);
        _engine = new GameEngine<YourGameContext>(game, _context);
    }

    [Test]
    public async Task CompleteWalkthrough_ShouldWinGame()
    {
        // Start game
        var intro = await _engine.GetIntroduction();
        intro.Should().Contain("Welcome");

        // Execute walkthrough commands
        var commands = new[]
        {
            "look",           // See starting room
            "take key",       // Get key
            "n",              // Go north
            "unlock door with key",
            "open door"       // Win!
        };

        string lastResponse = "";
        foreach (var cmd in commands)
        {
            lastResponse = await _engine.GetResponse(cmd);
        }

        // Verify win
        _context.HasWon.Should().BeTrue();
        lastResponse.Should().Contain("escaped");
    }

    [Test]
    public async Task Score_ShouldAccumulate_ThroughWalkthrough()
    {
        await _engine.GetIntroduction();

        // Each action that gives points
        await _engine.GetResponse("take treasure");  // +25 if treasure gives points
        await _engine.GetResponse("n");
        await _engine.GetResponse("unlock door with key");  // +10
        await _engine.GetResponse("open door");  // +50

        _context.Score.Should().Be(85);  // Adjust based on your scoring
    }
}
```

### 6.4 Create Location Tests

**File: `YourGame.Tests/LocationTests.cs`**

```csharp
namespace YourGame.Tests;

[TestFixture]
public class LocationTests
{
    private GameEngine<YourGameContext> _engine = null!;
    private YourGameContext _context = null!;

    [SetUp]
    public void Setup()
    {
        Repository.Reset();
        var game = new YourGameGame();
        _context = (YourGameContext)game.GetContext(null!);
        _engine = new GameEngine<YourGameContext>(game, _context);
    }

    [Test]
    public async Task StartingRoom_ShouldBeInitialLocation()
    {
        await _engine.GetIntroduction();
        _context.CurrentLocation.Should().BeOfType<StartingRoom>();
    }

    [Test]
    public async Task Navigation_North_ShouldMoveToNextRoom()
    {
        await _engine.GetIntroduction();
        await _engine.GetResponse("n");
        _context.CurrentLocation.Should().BeOfType<NextRoom>();
    }

    [Test]
    public async Task DarkRoom_WithoutLight_ShouldBeUnusable()
    {
        await _engine.GetIntroduction();
        // Navigate to dark room
        await _engine.GetResponse("d");  // Assuming down goes to cellar

        var result = await _engine.GetResponse("look");
        result.Should().Contain("dark").Or.Contain("pitch black");
    }

    [Test]
    public async Task DarkRoom_WithFlashlight_ShouldBeVisible()
    {
        await _engine.GetIntroduction();

        // Get flashlight and turn it on
        await _engine.GetResponse("take flashlight");
        await _engine.GetResponse("turn on flashlight");

        // Go to dark room
        await _engine.GetResponse("d");

        var result = await _engine.GetResponse("look");
        result.Should().NotContain("pitch black");
        result.Should().Contain("cellar");  // Should see room description
    }
}
```

### 6.5 Create Item Tests

**File: `YourGame.Tests/ItemTests.cs`**

```csharp
namespace YourGame.Tests;

[TestFixture]
public class ItemTests
{
    private GameEngine<YourGameContext> _engine = null!;
    private YourGameContext _context = null!;

    [SetUp]
    public void Setup()
    {
        Repository.Reset();
        var game = new YourGameGame();
        _context = (YourGameContext)game.GetContext(null!);
        _engine = new GameEngine<YourGameContext>(game, _context);
    }

    [Test]
    public async Task Key_CanBeTaken()
    {
        await _engine.GetIntroduction();
        var result = await _engine.GetResponse("take key");

        result.Should().Contain("Taken");
        _context.HasItem<Key>().Should().BeTrue();
    }

    [Test]
    public async Task Key_CanBeExamined()
    {
        await _engine.GetIntroduction();
        var result = await _engine.GetResponse("examine key");

        result.Should().Contain("brass");
    }

    [Test]
    public async Task Chest_CanBeOpened()
    {
        await _engine.GetIntroduction();
        await _engine.GetResponse("n");  // Go to room with chest

        var result = await _engine.GetResponse("open chest");
        result.Should().Contain("Opening").Or.Contain("Opened");
    }

    [Test]
    public async Task Chest_ContainsTreasure()
    {
        await _engine.GetIntroduction();
        await _engine.GetResponse("n");
        await _engine.GetResponse("open chest");
        var result = await _engine.GetResponse("examine chest");

        result.Should().Contain("treasure").Or.Contain("gold");
    }

    [Test]
    public async Task Table_CannotBeTaken()
    {
        await _engine.GetIntroduction();
        var result = await _engine.GetResponse("take table");

        result.Should().Contain("heavy");
        _context.HasItem<Table>().Should().BeFalse();
    }

    [Test]
    public async Task Door_CannotBeOpenedWhenLocked()
    {
        await _engine.GetIntroduction();
        await _engine.GetResponse("n");  // Go to door room

        var result = await _engine.GetResponse("open door");
        result.Should().Contain("locked");
    }

    [Test]
    public async Task Door_CanBeUnlockedWithKey()
    {
        await _engine.GetIntroduction();
        await _engine.GetResponse("take key");
        await _engine.GetResponse("n");

        var result = await _engine.GetResponse("unlock door with key");
        result.Should().Contain("unlock").Or.Contain("Click");
    }
}
```

---

## Phase 7: Solution Integration

### 7.1 Add to Solution File

Edit `Zork.sln` and add the new projects. Add them under a "Games" solution folder:

```
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "YourGame", "YourGame\YourGame.csproj", "{NEW-GUID-1}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "YourGame.Tests", "YourGame.Tests\YourGame.Tests.csproj", "{NEW-GUID-2}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "YourGame-Lambda", "YourGame-Lambda\src\YourGame-Lambda\YourGame-Lambda.csproj", "{NEW-GUID-3}"
EndProject
```

### 7.2 Update Console Program

**Edit: `Console/Program.cs`**

Add a case for your game:

```csharp
case "YourGame":
    var yourGame = new YourGameGame();
    var yourGameContext = (YourGameContext)yourGame.GetContext(generationClient);
    var yourGameEngine = new GameEngine<YourGameContext>(yourGame, yourGameContext);
    await RunGame(yourGameEngine);
    break;
```

---

## Phase 8: AWS Resources

### 8.1 Create DynamoDB Tables

```bash
# Session table (stores current game state)
aws dynamodb create-table \
    --table-name yourgame_session \
    --attribute-definitions AttributeName=sessionId,AttributeType=S \
    --key-schema AttributeName=sessionId,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST

# Saved game table (stores named saves)
aws dynamodb create-table \
    --table-name yourgame_savegame \
    --attribute-definitions AttributeName=saveName,AttributeType=S \
    --key-schema AttributeName=saveName,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST
```

### 8.2 Create Secrets Manager Secret

```bash
aws secretsmanager create-secret \
    --name YourGamePrompt \
    --secret-string "You are a text adventure game narrator. Provide atmospheric, engaging responses in the style of classic Infocom games like Zork. Keep responses concise but evocative."
```

---

## Testing and Verification

### Build and Test

```bash
# Build entire solution
dotnet build

# Run your game's tests
dotnet test YourGame.Tests

# Run all tests
dotnet test
```

### Test via Console

```bash
dotnet run --project Console YourGame
```

### Test Lambda Locally

```bash
cd YourGame-Lambda/src/YourGame-Lambda
dotnet run
```

Then test with curl:

```bash
# New game (GET to initialize)
curl "http://localhost:5000/YourGame?sessionId=test1"

# Play (POST commands)
curl -X POST "http://localhost:5000/YourGame" \
  -H "Content-Type: application/json" \
  -d '{"input": "look", "sessionId": "test1"}'

curl -X POST "http://localhost:5000/YourGame" \
  -H "Content-Type: application/json" \
  -d '{"input": "take key", "sessionId": "test1"}'
```

---

## Common Patterns and Interfaces

### Item Interfaces Quick Reference

| Interface | Purpose | Required Properties/Methods |
|-----------|---------|----------------------------|
| `ICanBeTakenAndDropped` | Portable items | `OnTheGroundDescription(ILocation)` |
| `ICanBeExamined` | Examinable items | `ExaminationDescription` |
| `ICanBeRead` | Readable items | `ReadDescription` |
| `ICanBeOpened` / `IOpenAndClose` | Containers, doors | `IsOpen`, `OnBeingOpened()` |
| `IAmALightSourceThatTurnsOnAndOff` | Light sources | `IsOn`, `IsLit`, `OnBeingTurnedOn()`, `OnBeingTurnedOff()` |
| `IGivePointsWhenFirstPickedUp` | Auto-scoring items | `NumberOfPoints` |
| `ICannotBeTaken` | Scenery items | (marker interface) |

### Base Classes Quick Reference

| Base Class | Purpose | Key Features |
|------------|---------|--------------|
| `ItemBase` | Simple items | `NounsForMatching`, `Size`, `CannotBeTakenDescription` |
| `OpenAndCloseBase` | Openable items | `IsOpen`, `CanBeOpened()`, `OnOpening()` |
| `OpenAndCloseContainerBase` | Containers with items | `Items`, `StartWithItemInside<T>()`, `NowOpen()` |
| `LocationBase` | Normal rooms | `Name`, `GetContextBasedDescription()`, `Map()`, `Init()` |
| `DarkLocationBase` | Dark rooms | Same as LocationBase, but requires light |

### Interaction Methods

```csharp
// Simple verb + noun: "take key", "examine box"
public override Task<InteractionResult?> RespondToSimpleInteraction(
    SimpleIntent action, IContext context, IGenerationClient client,
    IItemProcessorFactory itemProcessorFactory)

// Multi-noun: "unlock door with key", "put ball in box"
public override Task<InteractionResult?> RespondToMultiNounInteraction(
    MultiNounIntent action, IContext context, IGenerationClient client)

// Matching helpers
action.Match(["verb1", "verb2"], NounsForMatching)  // verb + this item
action.Match(["unlock"], NounsForMatching, ["with", "using"])  // verb + item + preposition
action.MatchVerb(["push", "press"])  // just verb matching
```

---

## Troubleshooting

### Common Issues

1. **"Item not found" errors**
   - Ensure item is placed with `StartWithItem<T>()` in location's `Init()`
   - Check `NounsForMatching` includes the word player is using

2. **Death message not appearing**
   - In `AfterEnterLocation`, return `result.InteractionMessage`, not empty string

3. **Lambda 404 errors**
   - Route is `/YourGame` not `/YourGameController`
   - Check controller has `[Route("[controller]")]` attribute

4. **Changes not reflected in Lambda**
   - Run `dotnet build` before `dotnet run`

5. **Dark room always dark**
   - Use `DarkLocationBase`, not `LocationBase`
   - Player needs `IAmALightSourceThatTurnsOnAndOff` item that `IsOn`

6. **Container items not accessible**
   - Container must be opened first
   - Use `OpenAndCloseContainerBase`
   - Items placed with `StartWithItemInside<T>()`

7. **Score not updating**
   - Implement `IGivePointsWhenFirstPickedUp` on items
   - Call `context.AddPoints(n)` for manual scoring

8. **Tests failing with null reference**
   - Call `Repository.Reset()` in `[SetUp]`
   - Ensure `GetContext()` is called to initialize

### Debug Tips

- Add logging in controller: `_cloudWatchLogger.Log($"Debug: {message}", "info")`
- Test locally before deploying to AWS
- Use fresh session IDs when testing (state is cached)
- Check DynamoDB tables for stored state

---

## Complete File Checklist

When creating a new game, ensure you have:

### Core Game (Required)
- [ ] `YourGame/YourGame.csproj`
- [ ] `YourGame/YourGameGame.cs`
- [ ] `YourGame/YourGameContext.cs`
- [ ] `YourGame/GlobalUsings.cs`
- [ ] `YourGame/GlobalCommand/YourGameGlobalCommandFactory.cs`

### Locations (At Least One)
- [ ] `YourGame/Location/StartingRoom.cs` (required - starting location)
- [ ] Additional locations as needed

### Items (As Needed)
- [ ] Core puzzle items (keys, etc.)
- [ ] Containers
- [ ] Scenery
- [ ] Light sources (if dark rooms exist)

### Special Mechanics (Optional)
- [ ] `YourGame/Command/DeathProcessor.cs` (if deaths possible)

### Lambda API (Required for Web)
- [ ] `YourGame-Lambda/src/YourGame-Lambda/YourGame-Lambda.csproj`
- [ ] `YourGame-Lambda/src/YourGame-Lambda/Startup.cs`
- [ ] `YourGame-Lambda/src/YourGame-Lambda/LambdaEntryPoint.cs`
- [ ] `YourGame-Lambda/src/YourGame-Lambda/LocalEntryPoint.cs`
- [ ] `YourGame-Lambda/src/YourGame-Lambda/Controllers/YourGameController.cs`

### Tests (Required)
- [ ] `YourGame.Tests/YourGame.Tests.csproj`
- [ ] `YourGame.Tests/GlobalUsings.cs`
- [ ] `YourGame.Tests/WalkthroughTests.cs`
- [ ] Additional test files as needed

### Integration
- [ ] Update `Zork.sln`
- [ ] Update `Console/Program.cs`

### AWS (Required for Deployment)
- [ ] DynamoDB session table
- [ ] DynamoDB savegame table
- [ ] Secrets Manager prompt secret
