/**
 * Example Theme Configuration: Zork
 *
 * This is an example theme configuration for Zork I, demonstrating
 * a classic dungeon crawler aesthetic with green text and stone/gray colors.
 *
 * Game-specific packages will create their own theme configurations
 * based on this structure.
 */

import { GameThemeConfig } from '../../theme';

export const zorkThemeExample: GameThemeConfig = {
  theme: {
    name: 'zork-classic',
    title: 'Zork AI',

    colors: {
      primary: '#84cc16',      // lime-500 - classic Zork green
      secondary: '#78716c',    // stone-500
      accent: '#a3e635',       // lime-400
      background: {
        dark: '#1c1917',       // stone-900
        medium: '#292524',     // stone-800
        light: '#44403c',      // stone-700
      },
      text: {
        primary: '#fafaf9',    // stone-50
        secondary: '#e7e5e4',  // stone-200
        muted: '#a8a29e',      // stone-400
      },
      border: 'rgba(68, 64, 60, 0.5)',  // stone-700/50
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
      value: '/assets/back2.png',
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

  customCSS: `
    /* Zork-specific custom styles */
    .game-text {
      text-shadow: 0 0 8px var(--color-primary);
    }

    .enable-glow .glow-effect {
      box-shadow: 0 0 10px var(--color-primary),
                  0 0 20px var(--color-primary);
    }

    /* Text glow for terminal-like effect */
    .enable-glow .terminal-text {
      text-shadow: 0 0 var(--glow-intensity, 8px) var(--glow-color, #84cc16);
    }
  `,
};
