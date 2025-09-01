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
- **UnitTests**: Core engine tests (466+ comprehensive tests)
- **ZorkOne.Tests**: Game-specific tests for Zork I
- **Planetfall.Tests**: Game-specific tests for Planetfall
- **Lambda.Tests**: AWS Lambda API tests
- **Planetfall-Lambda.Tests**: Planetfall Lambda API tests
- **IntegrationTests**: Cross-service integration tests

## Key Architecture Components

### Core Game Engine (`GameEngine/`)
- **Context.cs**: Game state management - inventory, scoring, moves, light sources, actors
- **IntentParser.cs**: Parses user input into game intents (move, interact, system commands)
- **Repository.cs**: Singleton pattern for all game items/locations - lazy-loaded, prevents duplicates
- **GameEngine.cs**: Main game loop orchestration

### Intent Processing (`GameEngine/IntentEngine/`)
- **SimpleInteractionEngine.cs**: Handles item interactions (take, use, examine)
- **MoveEngine.cs**: Movement between locations with weight limits and generation
- **GiveSomethingToSomeoneDecisionEngine.cs**: Complex multi-object interactions

### AI Integration (`Model/AIParsing/`, `Model/AIGeneration/`)
- **OpenAI integration** for parsing complex natural language commands
- **AWS Bedrock support** for text generation
- **Smart fallbacks** - tries simple parsing first, then AI if needed
- **Generated narrative responses** for failed actions

### Game Content
- **ZorkOne/**: Complete Zork I implementation with all locations/items
- **Planetfall/**: Space-themed adventure game
- **Extensible** - new games inherit from base engine

## Solution Structure
The solution is organized into logical groups:
- **Games folder**: ZorkOne/, Planetfall/, ZorkTwo/ - each game with its own implementation, tests, web client, and Lambda
- **AWS folder**: Cloud services (DynamoDb/, SecretsManager/, CloudWatch/, Bedrock/)
- **Core libraries**: GameEngine/, Model/, OpenAI/, Utilities/
- **Lambda APIs**: Individual Lambda functions for each game deployment
- **Web clients**: React/TypeScript SPAs for browser gameplay

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

### Comprehensive Test Coverage Achieved (466 tests, 99.8% passing)
- **Context.cs**: 26 tests covering inventory, scoring, item searching, game state
- **IntentParser.cs**: 22 tests covering command parsing, AI integration, logging
- **SimpleInteractionEngine.cs**: 10 tests covering item interactions with real Repository
- **MoveEngine.cs**: Core movement logic, weight validation, failure scenarios

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
- **Test core business logic** rather than focusing on edge cases with complex dependencies  
- **AI integration tests** require environment setup but validate critical user experience paths
- **Avoid mocking random generation** - focus on deterministic core functionality

### Working with the Repository Pattern
- Items and locations are singletons - calling `Repository.GetItem<Lamp>()` always returns the same instance
- **Lazy loading**: Items are only created when first requested, improving memory efficiency
- **State consistency**: Since items are singletons, state changes persist across the entire game session
- Use `Repository.GetItem<T>()` and `Repository.GetLocation<T>()` to access game objects

### AI Integration Guidelines  
- **Hierarchical parsing**: Try simple pattern matching first, then fall back to expensive AI calls
- **Context matters**: AI parsers receive location descriptions to better understand commands
- **Performance**: Cache AI responses when possible, avoid unnecessary API calls
- **Graceful degradation**: System should work even when AI services are unavailable

### Adding New Games
1. Create game-specific folder under solution (following ZorkOne/Planetfall pattern)
2. Implement game-specific items/locations inheriting from base classes
3. Create corresponding test project 
4. Add Lambda deployment if needed
5. Web client can reuse core engine through API calls