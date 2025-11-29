/**
 * Content Configuration Type Definitions
 *
 * Defines the structure for game-specific content (text, metadata, links).
 * Each game provides its own content configuration while using the same
 * core components for display.
 */

import { ReactNode } from 'react';

/**
 * Game metadata information
 */
export interface GameMetadata {
  /** Short game name (e.g., "Zork I", "Planetfall") */
  name: string;

  /** Full game title */
  fullTitle: string;

  /** Optional subtitle */
  subtitle?: string;

  /** Year of original release */
  year: string;

  /** Current version of this implementation */
  version: string;

  /** Original game creators */
  creators: string[];

  /** Original publisher */
  publisher: string;

  /** Game description */
  description: string;

  /** Genre tags */
  genre: string[];
}

/**
 * Example command for demonstration
 */
export interface ExampleCommand {
  /** The command text */
  command: string;

  /** Description of what this command does */
  description: string;
}

/**
 * Welcome modal content
 */
export interface WelcomeContent {
  /** Welcome modal title */
  title: string;

  /** Introduction paragraphs */
  paragraphs: string[];

  /** Example commands to try */
  exampleCommands: ExampleCommand[];

  /** Whether to show intro video button */
  showVideoButton: boolean;

  /** YouTube video ID (if showVideoButton is true) */
  videoId?: string;

  /** Additional custom buttons */
  additionalButtons?: Array<{
    label: string;
    onClick: () => void;
    icon?: ReactNode;
  }>;
}

/**
 * Menu item (link)
 */
export interface MenuItem {
  /** Display label */
  label: string;

  /** URL or route */
  href: string;

  /** Whether link opens in new tab */
  external: boolean;

  /** Optional icon */
  icon?: ReactNode;

  /** Optional description tooltip */
  description?: string;
}

/**
 * About menu section
 */
export interface AboutSection {
  /** Section title */
  title: string;

  /** Menu items in this section */
  items: MenuItem[];
}

/**
 * About modal content
 */
export interface AboutContent {
  /** About modal title */
  title: string;

  /** Game description */
  description: string;

  /** Organized sections of links and resources */
  sections: AboutSection[];
}

/**
 * Asset paths
 */
export interface GameAssets {
  /** Favicon path */
  favicon: string;

  /** Logo image path */
  logo?: string;

  /** Background image path */
  backgroundImage?: string;

  /** Screenshot paths for gallery */
  screenshots?: string[];
}

/**
 * API endpoint configuration
 */
export interface APIConfig {
  /** Base URL for API */
  baseUrl: string;

  /** Endpoint paths */
  endpoints: {
    /** Main game interaction endpoint */
    game: string;

    /** Save game endpoint */
    save: string;

    /** Restore game endpoint */
    restore: string;

    /** Session endpoint */
    session: string;
  };
}

/**
 * Analytics configuration
 */
export interface AnalyticsConfig {
  /** Mixpanel token */
  mixpanelToken: string;

  /** Whether tracking is enabled */
  enableTracking: boolean;

  /** Optional Application Insights key */
  appInsightsKey?: string;
}

/**
 * Complete game content configuration
 */
export interface GameContentConfig {
  /** Game metadata */
  metadata: GameMetadata;

  /** Welcome modal content */
  welcome: WelcomeContent;

  /** About modal content */
  about: AboutContent;

  /** Asset paths */
  assets: GameAssets;

  /** API configuration */
  api: APIConfig;

  /** Analytics configuration (optional) */
  analytics?: AnalyticsConfig;

  /** Optional custom content */
  custom?: Record<string, unknown>;
}

/**
 * Helper type for game-specific content overrides
 */
export type PartialGameContent = Partial<GameContentConfig> & {
  metadata: GameMetadata; // metadata is always required
};
