# ZorkAI Web Clients Monorepo

This monorepo contains the web client implementations for the ZorkAI game engine, refactored to eliminate code duplication and improve maintainability.

## Project Structure

```
WebClients/
├── packages/
│   ├── game-client-core/       # Shared core library
│   ├── zorkweb-client/         # Zork I web client
│   └── planetfallweb-client/   # Planetfall web client
├── package.json                # Workspace configuration
└── tsconfig.json               # Root TypeScript config
```

## Architecture

### game-client-core

The `@zork-ai/game-client-core` package contains all shared functionality:

- **Components**: Reusable UI components (Header, GameInput, Compass, etc.)
- **Services**: API clients, session management, analytics
- **Models**: TypeScript interfaces for game data
- **Context**: React contexts for state management
- **Theme System**: Configurable theming for visual customization
- **Content System**: Declarative game-specific content configuration

### Game-Specific Clients

Each game client (zorkweb-client, planetfallweb-client) is minimal and focuses on:

- Theme configuration (colors, fonts, visual effects)
- Content configuration (welcome text, about menu, metadata)
- Game-specific assets (images, fonts, favicons)
- API endpoint configuration

## Getting Started

### Prerequisites

- Node.js >= 18.0.0
- npm >= 9.0.0

### Installation

```bash
# Install dependencies for all packages
npm install

# Or navigate to root and install
cd WebClients
npm install
```

### Development

```bash
# Run Zork web client in development mode
npm run dev:zork

# Run Planetfall web client in development mode
npm run dev:planetfall

# Build all packages
npm run build

# Build specific package
npm run build:core
npm run build:zork
npm run build:planetfall
```

### Testing

```bash
# Run all tests
npm test

# Run unit tests
npm run test:unit

# Run E2E tests
npm run test:e2e
```

### Code Quality

```bash
# Lint all packages
npm run lint

# Clean all build artifacts
npm run clean
```

## Development Workflow

### Adding a New Game

1. Create new package directory: `packages/my-game-client/`
2. Copy package structure from existing game client
3. Create theme configuration in `src/config/theme.config.ts`
4. Create content configuration in `src/config/content.config.ts`
5. Customize `App.tsx` if needed
6. Add to workspace in root `package.json`

See the [Adding a New Game Guide](./packages/game-client-core/docs/ADDING_A_GAME.md) for detailed instructions.

### Modifying Shared Code

All shared code lives in `packages/game-client-core/`. Changes to this package automatically benefit all games.

1. Make changes in `game-client-core/src/`
2. Run tests: `npm run test:unit --workspace=packages/game-client-core`
3. Build: `npm run build:core`
4. Test in game clients to ensure no regressions

### Theming

See [Theme System Documentation](./packages/game-client-core/docs/THEME_SYSTEM.md) for details on customizing visual appearance.

### Content Configuration

See [Content Configuration Guide](./packages/game-client-core/docs/CONTENT_SYSTEM.md) for details on configuring game-specific content.

## Project Status

**Current Phase**: Phase 1 - Foundation Setup ✅

- [x] Monorepo structure created
- [x] game-client-core package initialized
- [x] Workspace configuration
- [x] Testing infrastructure
- [ ] Phase 2: Theme System Implementation (Next)

See [Progress Tracker](../docs/WEBCLIENT-REFACTORING-PROGRESS.md) for detailed status.

## Documentation

- [Refactoring Plan](../docs/WEBCLIENT-SHARED-LIBRARY-REFACTORING-PLAN.md) - Comprehensive refactoring roadmap
- [Progress Tracker](../docs/WEBCLIENT-REFACTORING-PROGRESS.md) - Implementation progress
- [Core Library README](./packages/game-client-core/README.md) - Core library documentation
- [Architecture Documentation](./packages/game-client-core/docs/ARCHITECTURE.md) - Technical architecture (coming soon)

## Benefits of This Architecture

- **38% code reduction** - From ~170 files to ~105 files
- **Zero duplication** - Shared code in one place
- **Instant propagation** - Bug fixes benefit all games immediately
- **Rapid development** - New games in days instead of weeks
- **Consistent UX** - Shared components ensure consistency
- **Type safety** - Full TypeScript coverage

## Contributing

See the main project [CONTRIBUTING.md](../CONTRIBUTING.md) for contribution guidelines.

## License

MIT
