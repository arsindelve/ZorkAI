/**
 * Example Content Configuration: Zork
 *
 * This is an example content configuration for Zork I, demonstrating
 * how to structure game-specific text, metadata, and links.
 *
 * Game-specific packages will create their own content configurations
 * based on this structure.
 */

import { GameContentConfig } from '../../content';

export const zorkContentExample: GameContentConfig = {
  metadata: {
    name: 'Zork I',
    fullTitle: 'Zork I: The Great Underground Empire',
    year: '1980',
    version: '0.2.12',
    creators: [
      'Tim Anderson',
      'Marc Blank',
      'Bruce Daniels',
      'Dave Lebling'
    ],
    publisher: 'Infocom',
    description: 'A classic text adventure game set in the Great Underground Empire. Explore ancient ruins, solve intricate puzzles, and hunt for treasure in this iconic interactive fiction experience.',
    genre: ['Interactive Fiction', 'Adventure', 'Puzzle'],
  },

  welcome: {
    title: 'Welcome to Zork AI - A Modern Reimagining of the 1980s Classic!',
    paragraphs: [
      'You are about to experience Zork I: The Great Underground Empire, one of the most influential text adventure games ever created. In this game, you will explore an ancient underground realm filled with mystery, danger, and untold treasures.',
      'This modern version enhances the classic Zork experience with AI-powered natural language understanding, allowing you to interact with the game world in more intuitive and flexible ways than ever before.',
      'Your adventure begins west of a white house, with a boarded front door and a small mailbox nearby. The choices you make and the paths you take will determine your fate in the Great Underground Empire.',
    ],
    exampleCommands: [
      {
        command: 'open mailbox',
        description: 'Interact with objects in your surroundings',
      },
      {
        command: 'go south',
        description: 'Navigate through the game world',
      },
      {
        command: 'take lamp',
        description: 'Pick up items and manage inventory',
      },
      {
        command: 'examine sword',
        description: 'Investigate objects more closely',
      },
      {
        command: 'tell me about the empire',
        description: 'Ask questions using natural language (AI-powered)',
      },
    ],
    showVideoButton: true,
    videoId: 'your-youtube-video-id',
  },

  about: {
    title: 'About Zork I',
    description: 'Zork I: The Great Underground Empire is a text adventure game originally developed by MIT students and published by Infocom in 1980. It became one of the most successful and influential interactive fiction games of all time.',
    sections: [
      {
        title: 'Getting Started',
        items: [
          {
            label: 'What is this game?',
            href: '#',
            external: false,
            description: 'Learn about Zork and this AI-enhanced version',
          },
          {
            label: 'How to play',
            href: '#',
            external: false,
            description: 'Basic commands and gameplay tips',
          },
        ],
      },
      {
        title: 'Resources',
        items: [
          {
            label: '1984 Infocom Manual (PDF)',
            href: 'https://archive.org/details/Zork_I_The_Great_Underground_Empire_1984_Infocom',
            external: true,
            description: 'Original game manual from Internet Archive',
          },
          {
            label: '1982 TRS-80 Manual (PDF)',
            href: 'https://archive.org/details/Zork_I_The_Great_Underground_Empire_1982_Infocom_TRS-80',
            external: true,
            description: 'Earlier version manual for TRS-80',
          },
          {
            label: 'Zork Map',
            href: 'https://www.thezorklibrary.com/maps/zork1.html',
            external: true,
            description: 'Interactive map of the game world',
          },
          {
            label: 'Zork Walkthrough',
            href: 'https://www.walkthroughking.com/text/zork1.aspx',
            external: true,
            description: 'Complete game solution (spoilers!)',
          },
        ],
      },
      {
        title: 'More Information',
        items: [
          {
            label: 'Play Original Zork Online',
            href: 'https://playclassic.games/games/adventure-dos-games-online/play-zork-great-underground-empire-online/',
            external: true,
            description: 'Experience the original 1980 version',
          },
          {
            label: 'Wikipedia: Zork',
            href: 'https://en.wikipedia.org/wiki/Zork',
            external: true,
            description: 'Detailed history and information',
          },
          {
            label: 'The Zork Library',
            href: 'https://www.thezorklibrary.com/',
            external: true,
            description: 'Comprehensive Zork resource site',
          },
        ],
      },
    ],
  },

  assets: {
    favicon: '/favicon.ico',
    logo: '/Zork.webp',
    backgroundImage: '/back2.png',
    screenshots: [
      '/screenshots/zork-1.png',
      '/screenshots/zork-2.png',
      '/screenshots/zork-3.png',
    ],
  },

  api: {
    baseUrl: process.env.VITE_API_BASE_URL || 'http://localhost:5000',
    endpoints: {
      game: '/ZorkOne',
      save: '/ZorkOne/saveGame',
      restore: '/ZorkOne/restoreGame',
      session: '/ZorkOne/session',
    },
  },

  analytics: {
    mixpanelToken: process.env.VITE_MIXPANEL_TOKEN || '',
    enableTracking: true,
    appInsightsKey: process.env.VITE_APP_INSIGHTS_KEY,
  },

  custom: {
    // Game-specific custom data can go here
    maxInventoryWeight: 100,
    autoSaveInterval: 5, // minutes
  },
};
