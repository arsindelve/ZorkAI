/**
 * Theme System Type Definitions
 *
 * Defines the structure for game-specific visual theming.
 * Each game can provide its own theme configuration while using
 * the same core components.
 */

import { ReactNode } from 'react';

export interface ColorPalette {
  primary: string;
  secondary: string;
  accent: string;
  background: {
    dark: string;
    medium: string;
    light: string;
  };
  text: {
    primary: string;
    secondary: string;
    muted: string;
  };
  border: string;
  compass: string;
}

export interface Typography {
  heading: string;
  body: string;
  mono: string;
  sizes: {
    xs: string;
    sm: string;
    base: string;
    lg: string;
    xl: string;
    '2xl': string;
    '3xl': string;
  };
}

export interface VisualEffects {
  enableGlow: boolean;
  enableScanlines: boolean;
  enableGlassMorphism: boolean;
  enableAnimations: boolean;
  glowColor?: string;
  glowIntensity?: number;
}

export interface BackgroundConfig {
  type: 'image' | 'gradient' | 'pattern' | 'solid';
  value: string;
  attachment?: 'fixed' | 'scroll';
  repeat?: 'repeat' | 'no-repeat' | 'repeat-x' | 'repeat-y';
  position?: string;
  size?: string;
  overlay?: {
    enabled: boolean;
    gradient?: string;
    opacity?: number;
  };
}

export interface LayoutConfig {
  maxWidth: string;
  padding: string;
  borderRadius: string;
  showFooter: boolean;
  footerContent?: ReactNode;
}

export interface ComponentStyles {
  input: {
    backgroundColor: string;
    borderColor: string;
    textColor: string;
    focusColor: string;
  };
  button: {
    primaryBg: string;
    primaryHoverBg: string;
    secondaryBg: string;
    secondaryHoverBg: string;
  };
  modal: {
    backgroundColor: string;
    overlayColor: string;
    borderColor: string;
  };
}

export interface GameTheme {
  /** Unique theme identifier */
  name: string;

  /** Display title for the game */
  title: string;

  /** Color system */
  colors: ColorPalette;

  /** Typography settings */
  fonts: Typography;

  /** Visual effects configuration */
  effects: VisualEffects;

  /** Background configuration */
  background: BackgroundConfig;

  /** Layout settings */
  layout: LayoutConfig;

  /** Component-specific style overrides */
  components: ComponentStyles;
}

export interface GameThemeConfig {
  /** Theme configuration */
  theme: GameTheme;

  /** Optional custom CSS for advanced customization */
  customCSS?: string;
}

/**
 * CSS variable names used by the theme system
 */
export const CSS_VARIABLES = {
  // Colors
  PRIMARY: '--color-primary',
  SECONDARY: '--color-secondary',
  ACCENT: '--color-accent',
  BG_DARK: '--color-bg-dark',
  BG_MEDIUM: '--color-bg-medium',
  BG_LIGHT: '--color-bg-light',
  TEXT_PRIMARY: '--color-text-primary',
  TEXT_SECONDARY: '--color-text-secondary',
  TEXT_MUTED: '--color-text-muted',
  BORDER: '--color-border',
  COMPASS: '--color-compass',

  // Typography
  FONT_HEADING: '--font-heading',
  FONT_BODY: '--font-body',
  FONT_MONO: '--font-mono',

  // Layout
  MAX_WIDTH: '--layout-max-width',
  PADDING: '--layout-padding',
  BORDER_RADIUS: '--layout-border-radius',

  // Component-specific
  INPUT_BG: '--input-bg',
  INPUT_BORDER: '--input-border',
  INPUT_TEXT: '--input-text',
  INPUT_FOCUS: '--input-focus',

  BUTTON_PRIMARY_BG: '--button-primary-bg',
  BUTTON_PRIMARY_HOVER: '--button-primary-hover',
  BUTTON_SECONDARY_BG: '--button-secondary-bg',
  BUTTON_SECONDARY_HOVER: '--button-secondary-hover',

  MODAL_BG: '--modal-bg',
  MODAL_OVERLAY: '--modal-overlay',
  MODAL_BORDER: '--modal-border',
} as const;
