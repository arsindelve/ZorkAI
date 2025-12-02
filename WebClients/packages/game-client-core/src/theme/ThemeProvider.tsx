/**
 * Theme Provider Component
 *
 * Manages theme application through CSS variables and context.
 * Automatically applies theme changes to the DOM.
 */

import React, { createContext, useContext, useEffect, ReactNode } from 'react';
import { GameTheme, GameThemeConfig, CSS_VARIABLES } from './types';

interface ThemeContextValue {
  theme: GameTheme;
  applyTheme: () => void;
}

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined);

/**
 * Hook to access the current theme
 */
export const useTheme = (): ThemeContextValue => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};

interface ThemeProviderProps {
  config: GameThemeConfig;
  children: ReactNode;
}

/**
 * ThemeProvider Component
 *
 * Wraps the application and provides theme context to all child components.
 * Automatically injects CSS variables and applies visual effects.
 *
 * @example
 * ```tsx
 * import { ThemeProvider } from '@zork-ai/game-client-core/theme';
 * import { zorkTheme } from './config/theme.config';
 *
 * function App() {
 *   return (
 *     <ThemeProvider config={zorkTheme}>
 *       <YourApp />
 *     </ThemeProvider>
 *   );
 * }
 * ```
 */
export const ThemeProvider: React.FC<ThemeProviderProps> = ({
  config,
  children
}) => {
  const { theme, customCSS } = config;

  /**
   * Apply theme to the DOM
   */
  const applyTheme = (): void => {
    const root = document.documentElement;

    // Apply color variables
    root.style.setProperty(CSS_VARIABLES.PRIMARY, theme.colors.primary);
    root.style.setProperty(CSS_VARIABLES.SECONDARY, theme.colors.secondary);
    root.style.setProperty(CSS_VARIABLES.ACCENT, theme.colors.accent);
    root.style.setProperty(CSS_VARIABLES.BG_DARK, theme.colors.background.dark);
    root.style.setProperty(CSS_VARIABLES.BG_MEDIUM, theme.colors.background.medium);
    root.style.setProperty(CSS_VARIABLES.BG_LIGHT, theme.colors.background.light);
    root.style.setProperty(CSS_VARIABLES.TEXT_PRIMARY, theme.colors.text.primary);
    root.style.setProperty(CSS_VARIABLES.TEXT_SECONDARY, theme.colors.text.secondary);
    root.style.setProperty(CSS_VARIABLES.TEXT_MUTED, theme.colors.text.muted);
    root.style.setProperty(CSS_VARIABLES.BORDER, theme.colors.border);
    root.style.setProperty(CSS_VARIABLES.COMPASS, theme.colors.compass);

    // Apply typography variables
    root.style.setProperty(CSS_VARIABLES.FONT_HEADING, theme.fonts.heading);
    root.style.setProperty(CSS_VARIABLES.FONT_BODY, theme.fonts.body);
    root.style.setProperty(CSS_VARIABLES.FONT_MONO, theme.fonts.mono);

    // Apply layout variables
    root.style.setProperty(CSS_VARIABLES.MAX_WIDTH, theme.layout.maxWidth);
    root.style.setProperty(CSS_VARIABLES.PADDING, theme.layout.padding);
    root.style.setProperty(CSS_VARIABLES.BORDER_RADIUS, theme.layout.borderRadius);

    // Apply component-specific variables
    root.style.setProperty(CSS_VARIABLES.INPUT_BG, theme.components.input.backgroundColor);
    root.style.setProperty(CSS_VARIABLES.INPUT_BORDER, theme.components.input.borderColor);
    root.style.setProperty(CSS_VARIABLES.INPUT_TEXT, theme.components.input.textColor);
    root.style.setProperty(CSS_VARIABLES.INPUT_FOCUS, theme.components.input.focusColor);

    root.style.setProperty(CSS_VARIABLES.BUTTON_PRIMARY_BG, theme.components.button.primaryBg);
    root.style.setProperty(CSS_VARIABLES.BUTTON_PRIMARY_HOVER, theme.components.button.primaryHoverBg);
    root.style.setProperty(CSS_VARIABLES.BUTTON_SECONDARY_BG, theme.components.button.secondaryBg);
    root.style.setProperty(CSS_VARIABLES.BUTTON_SECONDARY_HOVER, theme.components.button.secondaryHoverBg);

    root.style.setProperty(CSS_VARIABLES.MODAL_BG, theme.components.modal.backgroundColor);
    root.style.setProperty(CSS_VARIABLES.MODAL_OVERLAY, theme.components.modal.overlayColor);
    root.style.setProperty(CSS_VARIABLES.MODAL_BORDER, theme.components.modal.borderColor);

    // Apply effect classes conditionally
    root.classList.toggle('enable-glow', theme.effects.enableGlow);
    root.classList.toggle('enable-scanlines', theme.effects.enableScanlines);
    root.classList.toggle('enable-glass', theme.effects.enableGlassMorphism);
    root.classList.toggle('enable-animations', theme.effects.enableAnimations);

    // Apply glow-specific variables if enabled
    if (theme.effects.enableGlow && theme.effects.glowColor) {
      root.style.setProperty('--glow-color', theme.effects.glowColor);
    }
    if (theme.effects.enableGlow && theme.effects.glowIntensity) {
      root.style.setProperty('--glow-intensity', `${theme.effects.glowIntensity}px`);
    }

    // Apply background styles
    applyBackgroundStyles(theme.background);

    // Inject custom CSS if provided
    if (customCSS) {
      injectCustomCSS(customCSS);
    }
  };

  /**
   * Apply background styles to document body
   */
  const applyBackgroundStyles = (bg: typeof theme.background): void => {
    const body = document.body;

    if (bg.type === 'image' || bg.type === 'pattern') {
      body.style.backgroundImage = `url(${bg.value})`;
      body.style.backgroundAttachment = bg.attachment || 'scroll';
      body.style.backgroundRepeat = bg.repeat || 'no-repeat';
      body.style.backgroundPosition = bg.position || 'center';
      body.style.backgroundSize = bg.size || 'cover';

      // Apply overlay if configured
      if (bg.overlay?.enabled) {
        const overlayDiv = document.getElementById('theme-bg-overlay') ||
          document.createElement('div');
        overlayDiv.id = 'theme-bg-overlay';
        overlayDiv.style.position = 'fixed';
        overlayDiv.style.top = '0';
        overlayDiv.style.left = '0';
        overlayDiv.style.right = '0';
        overlayDiv.style.bottom = '0';
        overlayDiv.style.pointerEvents = 'none';
        overlayDiv.style.zIndex = '-1';

        if (bg.overlay.gradient) {
          overlayDiv.style.background = bg.overlay.gradient;
        }
        if (bg.overlay.opacity !== undefined) {
          overlayDiv.style.opacity = String(bg.overlay.opacity);
        }

        if (!overlayDiv.parentNode) {
          body.appendChild(overlayDiv);
        }
      }
    } else if (bg.type === 'gradient') {
      body.style.background = bg.value;
    } else if (bg.type === 'solid') {
      body.style.backgroundColor = bg.value;
    }
  };

  /**
   * Inject custom CSS into the document head
   */
  const injectCustomCSS = (css: string): void => {
    const styleId = 'game-theme-custom-css';
    let styleElement = document.getElementById(styleId) as HTMLStyleElement | null;

    if (!styleElement) {
      styleElement = document.createElement('style');
      styleElement.id = styleId;
      document.head.appendChild(styleElement);
    }

    styleElement.textContent = css;
  };

  // Apply theme on mount and when theme changes
  useEffect(() => {
    applyTheme();

    // Cleanup function
    return () => {
      // Remove custom CSS on unmount
      const customStyleElement = document.getElementById('game-theme-custom-css');
      if (customStyleElement) {
        customStyleElement.remove();
      }

      // Remove background overlay if it exists
      const overlayElement = document.getElementById('theme-bg-overlay');
      if (overlayElement) {
        overlayElement.remove();
      }
    };
  }, [theme, customCSS]);

  return (
    <ThemeContext.Provider value={{ theme, applyTheme }}>
      {children}
    </ThemeContext.Provider>
  );
};
