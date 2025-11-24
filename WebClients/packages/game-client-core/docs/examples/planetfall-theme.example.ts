/**
 * Example Theme Configuration: Planetfall
 *
 * This is an example theme configuration for Planetfall, demonstrating
 * a sci-fi futuristic aesthetic with warm amber and deep blue colors,
 * advanced animations, and visual effects.
 *
 * Game-specific packages will create their own theme configurations
 * based on this structure.
 */

import { GameThemeConfig } from '../../theme';

export const planetfallThemeExample: GameThemeConfig = {
  theme: {
    name: 'planetfall-scifi',
    title: 'Planetfall AI',

    colors: {
      primary: '#ff9500',      // Warm amber
      secondary: '#0ea5e9',    // Deep blue (sky-500)
      accent: '#fbbf24',       // Gold (amber-400)
      background: {
        dark: '#0a1929',
        medium: '#1e293b',     // slate-800
        light: '#334155',      // slate-700
      },
      text: {
        primary: '#f8fafc',    // slate-50
        secondary: '#e2e8f0',  // slate-200
        muted: '#94a3b8',      // slate-400
      },
      border: 'rgba(51, 65, 85, 0.6)',
      compass: '#0095ff',
    },

    fonts: {
      heading: 'Bebas Neue, sans-serif',
      body: 'Roboto, sans-serif',
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
      enableScanlines: true,
      enableGlassMorphism: true,
      enableAnimations: true,
      glowColor: '#ff9500',
      glowIntensity: 12,
    },

    background: {
      type: 'image',
      value: 'https://zork-game-saves.s3.us-west-2.amazonaws.com/nebula.webp',
      attachment: 'fixed',
      size: 'cover',
      position: 'center',
      overlay: {
        enabled: true,
        gradient: 'linear-gradient(to bottom, rgba(10, 25, 41, 0.7), rgba(10, 25, 41, 0.9))',
        opacity: 0.8,
      },
    },

    layout: {
      maxWidth: '100vw',
      padding: '1rem',
      borderRadius: '0.75rem',
      showFooter: true,
      footerContent: undefined, // Will be provided by game-specific implementation
    },

    components: {
      input: {
        backgroundColor: 'rgba(10, 25, 41, 0.9)',
        borderColor: 'color-mix(in srgb, #ff9500 30%, transparent)',
        textColor: '#f8fafc',
        focusColor: '#ff9500',
      },
      button: {
        primaryBg: '#ff9500',
        primaryHoverBg: '#fbbf24',
        secondaryBg: '#1e293b',
        secondaryHoverBg: '#334155',
      },
      modal: {
        backgroundColor: 'rgba(10, 25, 41, 0.95)',
        overlayColor: 'rgba(0, 0, 0, 0.85)',
        borderColor: 'color-mix(in srgb, #ff9500 30%, transparent)',
      },
    },
  },

  customCSS: `
    /* Planetfall sci-fi effects */

    /* Pulsing glow animation */
    @keyframes glowPulse {
      0%, 100% {
        box-shadow: 0 0 10px var(--glow-color, #ff9500);
      }
      50% {
        box-shadow: 0 0 20px var(--glow-color, #ff9500),
                    0 0 30px var(--color-accent);
      }
    }

    /* Scanline animation */
    @keyframes scanline {
      0% { transform: translateY(-100%); }
      100% { transform: translateY(100vh); }
    }

    /* Slide down animation for modals */
    @keyframes slideDown {
      from {
        opacity: 0;
        transform: translateY(-20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    /* Apply pulsing glow to elements with this class */
    .enable-animations .pulse-glow {
      animation: glowPulse 2s ease-in-out infinite;
    }

    /* CRT-style scanline effect */
    .enable-scanlines::before {
      content: '';
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 2px;
      background: linear-gradient(
        to bottom,
        transparent,
        rgba(255, 149, 0, 0.3),
        transparent
      );
      animation: scanline 8s linear infinite;
      pointer-events: none;
      z-index: 9999;
    }

    /* Glass morphism effect */
    .enable-glass {
      backdrop-filter: blur(10px);
      -webkit-backdrop-filter: blur(10px);
      background: rgba(10, 25, 41, 0.7) !important;
    }

    /* Neon border effect */
    .neon-border {
      border: 2px solid;
      border-image: linear-gradient(
        135deg,
        var(--color-primary),
        var(--color-secondary),
        var(--color-accent)
      ) 1;
      box-shadow:
        0 0 10px color-mix(in srgb, var(--color-primary) 50%, transparent),
        inset 0 0 10px color-mix(in srgb, var(--color-primary) 20%, transparent);
    }

    /* Futuristic text glow */
    .enable-glow .sci-fi-text {
      text-shadow:
        0 0 10px var(--glow-color, #ff9500),
        0 0 20px var(--glow-color, #ff9500),
        0 0 30px var(--glow-color, #ff9500);
    }

    /* Modal slide-down animation */
    .enable-animations .modal-content {
      animation: slideDown 0.3s ease-out;
    }
  `,
};
