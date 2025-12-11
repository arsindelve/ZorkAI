/**
 * Content Provider Component
 *
 * Manages game-specific content through React context.
 * Provides access to game metadata, welcome text, about links, etc.
 */

import React, { createContext, useContext, ReactNode } from 'react';
import { GameContentConfig } from './types';

interface ContentContextValue {
  content: GameContentConfig;
}

const ContentContext = createContext<ContentContextValue | undefined>(undefined);

/**
 * Hook to access game content
 */
export const useGameContent = (): ContentContextValue => {
  const context = useContext(ContentContext);
  if (!context) {
    throw new Error('useGameContent must be used within a ContentProvider');
  }
  return context;
};

/**
 * Convenience hooks for specific content sections
 */
export const useGameMetadata = () => {
  const { content } = useGameContent();
  return content.metadata;
};

export const useWelcomeContent = () => {
  const { content } = useGameContent();
  return content.welcome;
};

export const useAboutContent = () => {
  const { content } = useGameContent();
  return content.about;
};

export const useGameAssets = () => {
  const { content } = useGameContent();
  return content.assets;
};

export const useAPIConfig = () => {
  const { content } = useGameContent();
  return content.api;
};

export const useAnalytics = () => {
  const { content } = useGameContent();
  return content.analytics;
};

interface ContentProviderProps {
  content: GameContentConfig;
  children: ReactNode;
}

/**
 * ContentProvider Component
 *
 * Wraps the application and provides game content to all child components.
 *
 * @example
 * ```tsx
 * import { ContentProvider } from '@zork-ai/game-client-core/content';
 * import { zorkContent } from './config/content.config';
 *
 * function App() {
 *   return (
 *     <ContentProvider content={zorkContent}>
 *       <YourApp />
 *     </ContentProvider>
 *   );
 * }
 * ```
 */
export const ContentProvider: React.FC<ContentProviderProps> = ({
  content,
  children
}) => {
  // Validate required content sections
  if (!content.metadata) {
    throw new Error('ContentProvider: metadata is required');
  }

  if (!content.metadata.name || !content.metadata.fullTitle) {
    throw new Error('ContentProvider: metadata.name and metadata.fullTitle are required');
  }

  return (
    <ContentContext.Provider value={{ content }}>
      {children}
    </ContentContext.Provider>
  );
};

/**
 * Higher-order component to inject content as props
 */
export function withGameContent<P extends { content?: GameContentConfig }>(
  Component: React.ComponentType<P>
): React.FC<Omit<P, 'content'>> {
  return (props: Omit<P, 'content'>) => {
    const { content } = useGameContent();
    return <Component {...(props as P)} content={content} />;
  };
}
