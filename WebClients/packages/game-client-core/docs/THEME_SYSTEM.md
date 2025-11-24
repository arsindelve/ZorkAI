# Theme System Documentation

The theme system provides a flexible, configurable way to customize the visual appearance of game clients while using the same core components.

## Overview

The theme system uses CSS variables and React context to allow each game to define its own visual identity:

- **Colors**: Primary, secondary, accent, background, text, borders
- **Typography**: Fonts and sizes
- **Visual Effects**: Glow, scanlines, glass morphism, animations
- **Background**: Images, patterns, gradients, or solid colors
- **Layout**: Width, padding, border radius, footer
- **Component Styles**: Custom styling for inputs, buttons, and modals

## Quick Start

### 1. Create a Theme Configuration

```typescript
import { GameThemeConfig } from '@zork-ai/game-client-core/theme';

export const myGameTheme: GameThemeConfig = {
  theme: {
    name: 'my-game',
    title: 'My Game',

    colors: {
      primary: '#84cc16',
      secondary: '#78716c',
      accent: '#a3e635',
      background: {
        dark: '#1c1917',
        medium: '#292524',
        light: '#44403c',
      },
      text: {
        primary: '#fafaf9',
        secondary: '#e7e5e4',
        muted: '#a8a29e',
      },
      border: 'rgba(68, 64, 60, 0.5)',
      compass: '#84cc16',
    },

    fonts: {
      heading: 'Platypi, serif',
      body: 'Lato, sans-serif',
      mono: 'Roboto Mono, monospace',
      sizes: {
        xs: '0.75rem',
        sm: '0.875rem',
        base: '1rem',
        lg: '1.125rem',
        xl: '1.25rem',
        '2xl': '1.5rem',
        '3xl': '1.875rem',
      },
    },

    effects: {
      enableGlow: true,
      enableScanlines: false,
      enableGlassMorphism: false,
      enableAnimations: true,
      glowColor: '#84cc16',
      glowIntensity: 8,
    },

    background: {
      type: 'pattern',
      value: '/assets/background.png',
      repeat: 'repeat',
      attachment: 'scroll',
    },

    layout: {
      maxWidth: '100vw',
      padding: '1rem',
      borderRadius: '0.5rem',
      showFooter: false,
    },

    components: {
      input: {
        backgroundColor: 'rgba(28, 25, 23, 0.9)',
        borderColor: 'rgba(68, 64, 60, 0.5)',
        textColor: '#fafaf9',
        focusColor: '#84cc16',
      },
      button: {
        primaryBg: '#84cc16',
        primaryHoverBg: '#a3e635',
        secondaryBg: '#292524',
        secondaryHoverBg: '#44403c',
      },
      modal: {
        backgroundColor: 'rgba(28, 25, 23, 0.95)',
        overlayColor: 'rgba(0, 0, 0, 0.75)',
        borderColor: 'rgba(68, 64, 60, 0.5)',
      },
    },
  },

  // Optional custom CSS
  customCSS: `
    .my-custom-class {
      /* Your custom styles */
    }
  `,
};
```

### 2. Wrap Your App with ThemeProvider

```typescript
import { ThemeProvider } from '@zork-ai/game-client-core/theme';
import { myGameTheme } from './config/theme.config';

function App() {
  return (
    <ThemeProvider config={myGameTheme}>
      <YourGameComponents />
    </ThemeProvider>
  );
}
```

### 3. Use Theme in Components

```typescript
import { useTheme } from '@zork-ai/game-client-core/theme';

function MyComponent() {
  const { theme } = useTheme();

  return (
    <div style={{ color: theme.colors.primary }}>
      {theme.title}
    </div>
  );
}
```

## CSS Variables

The theme system automatically injects CSS variables that you can use in your styles:

### Colors
- `--color-primary`
- `--color-secondary`
- `--color-accent`
- `--color-bg-dark`
- `--color-bg-medium`
- `--color-bg-light`
- `--color-text-primary`
- `--color-text-secondary`
- `--color-text-muted`
- `--color-border`
- `--color-compass`

### Typography
- `--font-heading`
- `--font-body`
- `--font-mono`

### Layout
- `--layout-max-width`
- `--layout-padding`
- `--layout-border-radius`

### Component-Specific
- `--input-bg`, `--input-border`, `--input-text`, `--input-focus`
- `--button-primary-bg`, `--button-primary-hover`
- `--button-secondary-bg`, `--button-secondary-hover`
- `--modal-bg`, `--modal-overlay`, `--modal-border`

### Effect-Specific
- `--glow-color`
- `--glow-intensity`

### Using CSS Variables

```css
.my-element {
  background-color: var(--color-bg-dark);
  color: var(--color-primary);
  font-family: var(--font-heading);
  border: 1px solid var(--color-border);
}

.glow-text {
  text-shadow: 0 0 var(--glow-intensity) var(--glow-color);
}
```

## Visual Effects

### Glow Effect

Enable text and element glow:

```typescript
effects: {
  enableGlow: true,
  glowColor: '#84cc16',
  glowIntensity: 8,
}
```

Elements with the class `enable-glow` will receive glow effects.

### Scanlines

Enable CRT-style scanlines:

```typescript
effects: {
  enableScanlines: true,
}
```

### Glass Morphism

Enable frosted glass backdrop blur effect:

```typescript
effects: {
  enableGlassMorphism: true,
}
```

Elements with the class `enable-glass` will receive the blur effect.

### Animations

Enable custom animations:

```typescript
effects: {
  enableAnimations: true,
}
```

## Background Configuration

### Image or Pattern

```typescript
background: {
  type: 'image', // or 'pattern'
  value: '/assets/background.png',
  attachment: 'fixed',  // or 'scroll'
  repeat: 'no-repeat',  // or 'repeat', 'repeat-x', 'repeat-y'
  position: 'center',
  size: 'cover',
  overlay: {
    enabled: true,
    gradient: 'linear-gradient(to bottom, rgba(0,0,0,0.7), rgba(0,0,0,0.9))',
    opacity: 0.8,
  },
}
```

### Gradient

```typescript
background: {
  type: 'gradient',
  value: 'linear-gradient(135deg, #1c1917, #292524)',
}
```

### Solid Color

```typescript
background: {
  type: 'solid',
  value: '#1c1917',
}
```

## Custom CSS

For advanced customization beyond the theme configuration, use the `customCSS` property:

```typescript
const myTheme: GameThemeConfig = {
  theme: { /* ... */ },

  customCSS: `
    /* Custom animations */
    @keyframes myAnimation {
      0% { opacity: 0; }
      100% { opacity: 1; }
    }

    /* Custom classes */
    .my-custom-element {
      animation: myAnimation 1s ease-in-out;
      background: var(--color-primary);
    }

    /* Override specific elements */
    .game-container {
      border: 2px solid var(--color-accent);
    }
  `,
};
```

## Example Themes

### Zork (Classic Dungeon Crawler)

See [zork-theme.example.ts](./examples/zork-theme.example.ts) for a complete example of:
- Green terminal-style text
- Stone/gray color palette
- Simple glow effects
- Pattern background

### Planetfall (Sci-Fi Futuristic)

See [planetfall-theme.example.ts](./examples/planetfall-theme.example.ts) for a complete example of:
- Warm amber and deep blue colors
- Advanced animations (glow pulse, scanlines)
- Glass morphism effects
- Nebula background image with overlay

## Best Practices

1. **Use CSS Variables**: Prefer CSS variables over hardcoded colors in your styles for consistency
2. **Theme Context**: Components should use `useTheme()` to access theme values dynamically
3. **Performance**: Custom CSS is injected once on theme load - avoid excessive custom styles
4. **Accessibility**: Ensure sufficient color contrast between text and backgrounds
5. **Testing**: Test your theme with all visual effects enabled and disabled
6. **Documentation**: Document any custom CSS classes you add for future maintainers

## Troubleshooting

### Theme Not Applying

Ensure your app is wrapped in `<ThemeProvider>`:

```typescript
function App() {
  return (
    <ThemeProvider config={myTheme}>
      <YourApp />
    </ThemeProvider>
  );
}
```

### CSS Variables Not Working

Check that the property path is correct:

```css
/* Correct */
background: var(--color-primary);

/* Incorrect */
background: var(--primary);
```

### Custom CSS Not Applying

Ensure your `customCSS` is valid CSS and doesn't have syntax errors. Check the browser console for CSS parsing errors.

### Effects Not Visible

Verify that:
1. The effect is enabled in your theme config
2. Your elements have the correct class names (`enable-glow`, `enable-glass`, etc.)
3. CSS for the effects is properly defined

## Migration Guide

### From Hardcoded Styles to Theme System

**Before:**
```typescript
<div style={{ backgroundColor: '#1c1917', color: '#84cc16' }}>
  Hello
</div>
```

**After:**
```typescript
<div style={{
  backgroundColor: 'var(--color-bg-dark)',
  color: 'var(--color-primary)'
}}>
  Hello
</div>
```

Or using the theme context:

```typescript
const { theme } = useTheme();

<div style={{
  backgroundColor: theme.colors.background.dark,
  color: theme.colors.primary
}}>
  Hello
</div>
```

## Advanced Topics

### Dynamic Theme Switching

```typescript
const [currentTheme, setCurrentTheme] = useState(zorkTheme);

function switchToPlanetfall() {
  setCurrentTheme(planetfallTheme);
}

return (
  <ThemeProvider config={currentTheme}>
    <YourApp />
    <button onClick={switchToPlanetfall}>
      Switch Theme
    </button>
  </ThemeProvider>
);
```

### Theme Composition

```typescript
import { GameTheme } from '@zork-ai/game-client-core/theme';

const baseTheme: GameTheme = { /* common settings */ };

const zorkTheme: GameTheme = {
  ...baseTheme,
  colors: { /* Zork-specific colors */ },
};

const planetfallTheme: GameTheme = {
  ...baseTheme,
  colors: { /* Planetfall-specific colors */ },
};
```

## API Reference

### Types

- `GameTheme`: Main theme configuration interface
- `GameThemeConfig`: Theme + optional custom CSS
- `ColorPalette`: Color system definition
- `Typography`: Font and size configuration
- `VisualEffects`: Effect toggles and settings
- `BackgroundConfig`: Background styling
- `LayoutConfig`: Layout settings
- `ComponentStyles`: Component-specific overrides

### Hooks

- `useTheme()`: Access current theme and apply function

### Components

- `<ThemeProvider>`: Root theme provider component

### Constants

- `CSS_VARIABLES`: CSS variable name constants

---

For more information, see the [main README](../README.md) or explore the [example themes](./examples/).
