# Content Configuration System Documentation

The content system provides a structured way to manage game-specific text, metadata, and links while using shared display components.

## Overview

The content system allows each game to define:

- **Metadata**: Game name, title, year, creators, description
- **Welcome Content**: Introduction text, example commands
- **About Content**: Resources, links, documentation
- **Assets**: Favicon, logo, screenshots
- **API Configuration**: Endpoints for game backend
- **Analytics**: Tracking configuration

## Quick Start

### 1. Create a Content Configuration

```typescript
import { GameContentConfig } from '@zork-ai/game-client-core/content';

export const myGameContent: GameContentConfig = {
  metadata: {
    name: 'My Game',
    fullTitle: 'My Awesome Game',
    year: '2025',
    version: '1.0.0',
    creators: ['Your Name'],
    publisher: 'Your Company',
    description: 'A fantastic text adventure',
    genre: ['Interactive Fiction', 'Adventure'],
  },

  welcome: {
    title: 'Welcome to My Game!',
    paragraphs: [
      'Your adventure begins here...',
      'This is the second paragraph of your introduction.',
    ],
    exampleCommands: [
      {
        command: 'look around',
        description: 'Examine your surroundings',
      },
      {
        command: 'take sword',
        description: 'Pick up items',
      },
    ],
    showVideoButton: false,
  },

  about: {
    title: 'About My Game',
    description: 'Learn more about this game',
    sections: [
      {
        title: 'Resources',
        items: [
          {
            label: 'Game Manual',
            href: '/manual.pdf',
            external: false,
          },
          {
            label: 'Walkthrough',
            href: 'https://example.com/walkthrough',
            external: true,
          },
        ],
      },
    ],
  },

  assets: {
    favicon: '/favicon.ico',
    logo: '/logo.png',
    backgroundImage: '/background.jpg',
  },

  api: {
    baseUrl: 'http://localhost:5000',
    endpoints: {
      game: '/MyGame',
      save: '/MyGame/saveGame',
      restore: '/MyGame/restoreGame',
      session: '/MyGame/session',
    },
  },
};
```

### 2. Wrap Your App with ContentProvider

```typescript
import { ContentProvider } from '@zork-ai/game-client-core/content';
import { myGameContent } from './config/content.config';

function App() {
  return (
    <ContentProvider content={myGameContent}>
      <YourGameComponents />
    </ContentProvider>
  );
}
```

### 3. Use Content in Components

```typescript
import { useGameContent, useGameMetadata } from '@zork-ai/game-client-core/content';

function MyComponent() {
  const { content } = useGameContent();
  const metadata = useGameMetadata();

  return (
    <div>
      <h1>{metadata.fullTitle}</h1>
      <p>{metadata.description}</p>
    </div>
  );
}
```

## Content Structure

### Game Metadata

Required information about your game:

```typescript
metadata: {
  name: 'Zork I',                    // Short name
  fullTitle: 'Zork I: The Great Underground Empire',
  subtitle: 'Optional subtitle',     // Optional
  year: '1980',                      // Original release year
  version: '0.2.12',                 // Current version
  creators: ['Tim Anderson', '...'], // Original creators
  publisher: 'Infocom',
  description: 'Game description...',
  genre: ['Interactive Fiction', 'Adventure'],
}
```

### Welcome Content

Content for the welcome modal shown to new players:

```typescript
welcome: {
  title: 'Welcome to Your Game!',
  paragraphs: [
    'First paragraph of introduction...',
    'Second paragraph...',
    'Third paragraph...',
  ],
  exampleCommands: [
    {
      command: 'look around',
      description: 'What this command does',
    },
    {
      command: 'take sword',
      description: 'How to pick up items',
    },
  ],
  showVideoButton: true,              // Show intro video button?
  videoId: 'youtube-video-id',        // If showVideoButton is true
  additionalButtons: [                // Optional custom buttons
    {
      label: 'Quick Start Guide',
      onClick: () => console.log('Clicked!'),
    },
  ],
}
```

### About Content

Organized sections of resources and links:

```typescript
about: {
  title: 'About My Game',
  description: 'Brief description for the about modal',
  sections: [
    {
      title: 'Getting Started',
      items: [
        {
          label: 'How to Play',
          href: '/guide',
          external: false,
          description: 'Optional tooltip',
        },
      ],
    },
    {
      title: 'Resources',
      items: [
        {
          label: 'Game Manual (PDF)',
          href: 'https://example.com/manual.pdf',
          external: true,
        },
        {
          label: 'Interactive Map',
          href: 'https://example.com/map',
          external: true,
        },
      ],
    },
  ],
}
```

### Assets

Paths to game assets:

```typescript
assets: {
  favicon: '/favicon.ico',           // Required
  logo: '/logo.png',                 // Optional
  backgroundImage: '/background.jpg', // Optional
  screenshots: [                      // Optional
    '/screenshots/1.png',
    '/screenshots/2.png',
  ],
}
```

### API Configuration

Backend endpoint configuration:

```typescript
api: {
  baseUrl: process.env.VITE_API_BASE_URL || 'http://localhost:5000',
  endpoints: {
    game: '/MyGame',                 // Main game endpoint
    save: '/MyGame/saveGame',        // Save endpoint
    restore: '/MyGame/restoreGame',  // Restore endpoint
    session: '/MyGame/session',      // Session endpoint
  },
}
```

### Analytics Configuration

Optional analytics setup:

```typescript
analytics: {
  mixpanelToken: process.env.VITE_MIXPANEL_TOKEN || '',
  enableTracking: true,
  appInsightsKey: process.env.VITE_APP_INSIGHTS_KEY, // Optional
}
```

### Custom Data

Store any game-specific data:

```typescript
custom: {
  maxInventoryWeight: 100,
  autoSaveInterval: 5,
  difficultyLevel: 'normal',
  // Any custom fields you need
}
```

## Available Hooks

### useGameContent()

Access the complete content configuration:

```typescript
import { useGameContent } from '@zork-ai/game-client-core/content';

function MyComponent() {
  const { content } = useGameContent();

  return <div>{content.metadata.name}</div>;
}
```

### useGameMetadata()

Convenience hook for metadata only:

```typescript
import { useGameMetadata } from '@zork-ai/game-client-core/content';

function Header() {
  const metadata = useGameMetadata();

  return (
    <header>
      <h1>{metadata.fullTitle}</h1>
      <p>By {metadata.creators.join(', ')}</p>
    </header>
  );
}
```

### useWelcomeContent()

Access welcome modal content:

```typescript
import { useWelcomeContent } from '@zork-ai/game-client-core/content';

function WelcomeModal() {
  const welcome = useWelcomeContent();

  return (
    <div>
      <h2>{welcome.title}</h2>
      {welcome.paragraphs.map((p, i) => (
        <p key={i}>{p}</p>
      ))}
    </div>
  );
}
```

### useAboutContent()

Access about modal content:

```typescript
import { useAboutContent } from '@zork-ai/game-client-core/content';

function AboutModal() {
  const about = useAboutContent();

  return (
    <div>
      <h2>{about.title}</h2>
      {about.sections.map((section, i) => (
        <section key={i}>
          <h3>{section.title}</h3>
          <ul>
            {section.items.map((item, j) => (
              <li key={j}>
                <a href={item.href} target={item.external ? '_blank' : '_self'}>
                  {item.label}
                </a>
              </li>
            ))}
          </ul>
        </section>
      ))}
    </div>
  );
}
```

### useGameAssets()

Access asset paths:

```typescript
import { useGameAssets } from '@zork-ai/game-client-core/content';

function Logo() {
  const assets = useGameAssets();

  return <img src={assets.logo} alt="Game Logo" />;
}
```

### useAPIConfig()

Access API configuration:

```typescript
import { useAPIConfig } from '@zork-ai/game-client-core/content';
import axios from 'axios';

function GameClient() {
  const api = useAPIConfig();

  const sendCommand = async (command: string) => {
    const response = await axios.post(
      `${api.baseUrl}${api.endpoints.game}`,
      { command }
    );
    return response.data;
  };

  // ...
}
```

### useAnalytics()

Access analytics configuration:

```typescript
import { useAnalytics } from '@zork-ai/game-client-core/content';
import mixpanel from 'mixpanel-browser';

function Analytics() {
  const analytics = useAnalytics();

  useEffect(() => {
    if (analytics?.enableTracking && analytics.mixpanelToken) {
      mixpanel.init(analytics.mixpanelToken);
    }
  }, [analytics]);

  // ...
}
```

## Example Configurations

### Zork (Classic Adventure)

See [zork-content.example.tsx](./examples/zork-content.example.tsx) for a complete example featuring:
- Multiple creator credits
- Intro video support
- Extensive resource links (manuals, maps, walkthroughs)
- Classic adventure game examples

### Planetfall (Sci-Fi)

See [planetfall-content.example.tsx](./examples/planetfall-content.example.tsx) for a complete example featuring:
- Subtitle usage
- Floyd robot interaction examples
- Sci-fi themed resources
- Custom game-specific data

## Best Practices

1. **Required Fields**: Always provide at minimum `metadata.name` and `metadata.fullTitle`
2. **External Links**: Always set `external: true` for links outside your domain
3. **Environment Variables**: Use environment variables for sensitive data like API keys
4. **Descriptions**: Provide clear, concise descriptions for better UX
5. **Example Commands**: Include 4-6 diverse example commands covering different mechanics
6. **Accessibility**: Provide `description` tooltips for menu items when helpful

## Advanced Usage

### Higher-Order Component

Inject content as props:

```typescript
import { withGameContent } from '@zork-ai/game-client-core/content';

interface MyComponentProps {
  content?: GameContentConfig;
  // other props...
}

const MyComponent: React.FC<MyComponentProps> = ({ content }) => {
  return <div>{content?.metadata.name}</div>;
};

export default withGameContent(MyComponent);
```

### Dynamic Content

Update content dynamically (for multi-game apps):

```typescript
const [currentGame, setCurrentGame] = useState(zorkContent);

function switchToPlanefall() {
  setCurrentGame(planetfallContent);
}

return (
  <ContentProvider content={currentGame}>
    <App />
    <button onClick={switchToPlanefall}>Switch Game</button>
  </ContentProvider>
);
```

### Content Composition

Extend base content for game variants:

```typescript
const baseContent = {
  metadata: { /* common fields */ },
  // ...
};

const zorkOneContent: GameContentConfig = {
  ...baseContent,
  metadata: {
    ...baseContent.metadata,
    name: 'Zork I',
    // Zork I specific overrides
  },
};

const zorkTwoContent: GameContentConfig = {
  ...baseContent,
  metadata: {
    ...baseContent.metadata,
    name: 'Zork II',
    // Zork II specific overrides
  },
};
```

## Validation

The ContentProvider validates required fields on initialization:

```typescript
// ✅ Valid - has required fields
<ContentProvider content={{
  metadata: { name: 'Game', fullTitle: 'My Game', /* ... */ },
  // ...
}} />

// ❌ Error - missing metadata
<ContentProvider content={{
  welcome: { /* ... */ },
  // Missing metadata!
}} />

// ❌ Error - incomplete metadata
<ContentProvider content={{
  metadata: { name: 'Game' }, // Missing fullTitle!
  // ...
}} />
```

## Troubleshooting

### "must be used within ContentProvider" Error

Ensure your component is wrapped in ContentProvider:

```typescript
// ✅ Correct
function App() {
  return (
    <ContentProvider content={myContent}>
      <MyComponent /> {/* Can use useGameContent() */}
    </ContentProvider>
  );
}

// ❌ Wrong
function App() {
  return <MyComponent />; {/* Error! No ContentProvider */}
}
```

### Type Errors with Custom Data

Use type assertion for custom fields:

```typescript
const { content } = useGameContent();
const maxWeight = (content.custom?.maxInventoryWeight as number) ?? 100;
```

Or extend the type:

```typescript
interface MyGameContent extends GameContentConfig {
  custom: {
    maxInventoryWeight: number;
    autoSaveInterval: number;
  };
}

const myContent: MyGameContent = { /* ... */ };
```

## Migration Guide

### From Hardcoded Content

**Before:**
```typescript
function Header() {
  return <h1>Zork I: The Great Underground Empire</h1>;
}
```

**After:**
```typescript
import { useGameMetadata } from '@zork-ai/game-client-core/content';

function Header() {
  const metadata = useGameMetadata();
  return <h1>{metadata.fullTitle}</h1>;
}
```

### From Props to Context

**Before:**
```typescript
<Header title="Zork I" creators={['Tim Anderson', '...']} />
```

**After:**
```typescript
import { useGameMetadata } from '@zork-ai/game-client-core/content';

function Header() {
  const metadata = useGameMetadata();
  return (
    <div>
      <h1>{metadata.name}</h1>
      <p>By {metadata.creators.join(', ')}</p>
    </div>
  );
}

// No props needed!
<Header />
```

## API Reference

### Types

- `GameContentConfig`: Complete content configuration
- `GameMetadata`: Game information
- `WelcomeContent`: Welcome modal content
- `AboutContent`: About modal content
- `ExampleCommand`: Command example
- `MenuItem`: Link item
- `AboutSection`: Section of links
- `GameAssets`: Asset paths
- `APIConfig`: API endpoints
- `AnalyticsConfig`: Analytics setup

### Components

- `<ContentProvider>`: Root content provider

### Hooks

- `useGameContent()`: Access complete content
- `useGameMetadata()`: Access metadata
- `useWelcomeContent()`: Access welcome content
- `useAboutContent()`: Access about content
- `useGameAssets()`: Access assets
- `useAPIConfig()`: Access API config
- `useAnalytics()`: Access analytics config

### Utilities

- `withGameContent()`: HOC for injecting content

---

For more information, see the [main README](../README.md) or explore the [example content](./examples/).
