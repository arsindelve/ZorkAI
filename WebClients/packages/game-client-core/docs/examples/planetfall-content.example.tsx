/**
 * Example Content Configuration: Planetfall
 *
 * This is an example content configuration for Planetfall, demonstrating
 * how to structure game-specific text, metadata, and links for a sci-fi game.
 *
 * Game-specific packages will create their own content configurations
 * based on this structure.
 */

import { GameContentConfig } from '../../content';

export const planetfallContentExample: GameContentConfig = {
  metadata: {
    name: 'Planetfall',
    fullTitle: 'Planetfall',
    subtitle: 'A Science Fiction Adventure',
    year: '1983',
    version: '0.2.12',
    creators: ['Steve Meretzky'],
    publisher: 'Infocom',
    description: 'A science fiction text adventure where you crash-land on a mysterious planet and befriend Floyd, an endearing robot companion. Explore an abandoned space station, solve puzzles, and uncover the dark secrets of Resida.',
    genre: ['Interactive Fiction', 'Science Fiction', 'Adventure'],
  },

  welcome: {
    title: 'Welcome to Planetfall AI - A Modern Reimagining of the 1983 Classic!',
    paragraphs: [
      'You are about to experience Planetfall, Steve Meretzky\'s beloved science fiction text adventure. In this game, you\'ll find yourself marooned on the mysterious planet Resida after your ship is destroyed.',
      'As you explore an seemingly abandoned planetary station, you\'ll meet Floyd - a cheerful maintenance robot who becomes your loyal companion. Together, you\'ll uncover the dark fate that befell this once-thriving outpost.',
      'This modern version enhances the original game with AI-powered natural language understanding, making it easier to interact with the world and your robotic friend Floyd in more natural, conversational ways.',
    ],
    exampleCommands: [
      {
        command: 'look around',
        description: 'Examine your surroundings',
      },
      {
        command: 'take kit',
        description: 'Pick up items you find',
      },
      {
        command: 'go west',
        description: 'Navigate through the station',
      },
      {
        command: 'talk to Floyd',
        description: 'Interact with Floyd the robot (AI-powered)',
      },
      {
        command: 'Floyd, get the card',
        description: 'Ask Floyd to help you with tasks',
      },
    ],
    showVideoButton: false, // No intro video for Planetfall
  },

  about: {
    title: 'About Planetfall',
    description: 'Planetfall is a science fiction interactive fiction game created by Steve Meretzky and published by Infocom in 1983. It\'s beloved for its humor, engaging story, and the memorable character of Floyd the robot.',
    sections: [
      {
        title: 'Getting Started',
        items: [
          {
            label: 'What is Planetfall.AI?',
            href: '#',
            external: false,
            description: 'Learn about this AI-enhanced version',
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
            label: 'Planetfall Manual (PDF)',
            href: 'https://archive.org/details/Planetfall_1983_Infocom',
            external: true,
            description: 'Original game manual from Internet Archive',
          },
          {
            label: 'Planetfall Map',
            href: 'http://www.infocom-if.org/downloads/planetfall_map.pdf',
            external: true,
            description: 'Map of the planetary station',
          },
          {
            label: 'Planetfall Walkthrough',
            href: 'https://gamefaqs.gamespot.com/pc/564888-planetfall/faqs/9357',
            external: true,
            description: 'Complete game solution (spoilers!)',
          },
        ],
      },
      {
        title: 'More Information',
        items: [
          {
            label: 'Wikipedia: Planetfall',
            href: 'https://en.wikipedia.org/wiki/Planetfall',
            external: true,
            description: 'Detailed history and information',
          },
          {
            label: 'Play Original Infocom Version',
            href: 'https://playclassic.games/games/adventure-dos-games-online/play-planetfall-online/',
            external: true,
            description: 'Experience the original 1983 version',
          },
          {
            label: 'The Digital Antiquarian on Planetfall',
            href: 'https://www.filfre.net/2013/03/planetfall/',
            external: true,
            description: 'Deep dive into the game\'s creation and impact',
          },
        ],
      },
    ],
  },

  assets: {
    favicon: '/favicon.ico',
    logo: '/Planetfall.webp',
    backgroundImage: 'https://zork-game-saves.s3.us-west-2.amazonaws.com/nebula.webp',
    screenshots: [
      '/screenshots/planetfall-1.png',
      '/screenshots/planetfall-2.png',
      '/screenshots/planetfall-3.png',
    ],
  },

  api: {
    baseUrl: process.env.VITE_API_BASE_URL || 'http://localhost:5000',
    endpoints: {
      game: '/Planetfall',
      save: '/Planetfall/saveGame',
      restore: '/Planetfall/restoreGame',
      session: '/Planetfall/session',
    },
  },

  analytics: {
    mixpanelToken: process.env.VITE_MIXPANEL_TOKEN || '',
    enableTracking: true,
    appInsightsKey: process.env.VITE_APP_INSIGHTS_KEY,
  },

  custom: {
    // Game-specific custom data
    floydPersonality: 'cheerful',
    difficultyLevel: 'intermediate',
    autoSaveInterval: 5, // minutes
  },
};
