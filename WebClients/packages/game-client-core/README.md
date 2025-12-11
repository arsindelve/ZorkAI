# @zork-ai/game-client-core

Shared core library for ZorkAI web game clients. This package contains all common functionality used across different game implementations (Zork, Planetfall, etc.).

## Features

- **UI Components**: Reusable React components for game interface
- **State Management**: Game context and session handling
- **API Services**: Backend communication and data management
- **Theme System**: Configurable theming for visual customization
- **Content System**: Declarative game-specific content configuration
- **Data Models**: TypeScript interfaces for game data

## Installation

```bash
npm install @zork-ai/game-client-core
```

## Usage

### Basic Setup

```typescript
import { ThemeProvider, ContentProvider } from '@zork-ai/game-client-core/context';
import { GameTheme } from '@zork-ai/game-client-core/theme';
import { GameContent } from '@zork-ai/game-client-core/content';

const App = () => {
  return (
    <ThemeProvider theme={myGameTheme}>
      <ContentProvider content={myGameContent}>
        {/* Your game components */}
      </ContentProvider>
    </ThemeProvider>
  );
};
```

### Using Components

```typescript
import { Header, GameInput, Compass } from '@zork-ai/game-client-core/components';
import { GameMenu } from '@zork-ai/game-client-core/menu';

// Components automatically use theme and content from context
```

### Creating a Theme

```typescript
import { GameTheme } from '@zork-ai/game-client-core/theme';

export const myGameTheme: GameTheme = {
  name: 'my-game',
  title: 'My Game',
  colors: {
    primary: '#84cc16',
    secondary: '#78716c',
    // ... more colors
  },
  fonts: {
    heading: 'Platypi, serif',
    body: 'Lato, sans-serif',
    mono: 'Roboto Mono, monospace',
  },
  effects: {
    enableGlow: true,
    enableScanlines: false,
    // ... more effects
  },
  // ... more theme configuration
};
```

### Configuring Game Content

```typescript
import { GameContentConfig } from '@zork-ai/game-client-core/content';

export const myGameContent: GameContentConfig = {
  metadata: {
    name: 'My Game',
    fullTitle: 'My Awesome Game',
    year: '2025',
    creators: ['Your Name'],
    // ... more metadata
  },
  welcome: {
    title: 'Welcome to My Game!',
    paragraphs: ['Game description...'],
    exampleCommands: [
      { command: 'look around', description: 'Examine surroundings' },
    ],
  },
  // ... more content
};
```

## Package Exports

- `@zork-ai/game-client-core` - Main entry point
- `@zork-ai/game-client-core/components` - UI components
- `@zork-ai/game-client-core/menu` - Menu components
- `@zork-ai/game-client-core/modal` - Modal components
- `@zork-ai/game-client-core/model` - Data models
- `@zork-ai/game-client-core/services` - API services
- `@zork-ai/game-client-core/context` - React contexts
- `@zork-ai/game-client-core/theme` - Theme system
- `@zork-ai/game-client-core/content` - Content configuration
- `@zork-ai/game-client-core/utils` - Utility functions

## Development

```bash
# Build the library
npm run build

# Watch mode for development
npm run dev

# Run tests
npm run test:unit

# Lint code
npm run lint
```

## Architecture

This library is built with:
- **React 18** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool
- **Material-UI** - Component library
- **Emotion** - CSS-in-JS
- **Axios** - HTTP client
- **React Query** - Data fetching

## Contributing

See the main project CONTRIBUTING.md for guidelines on contributing to this library.

## License

MIT
