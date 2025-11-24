import baseConfig from './config/jest.config.base';

export default {
  ...baseConfig,
  displayName: 'game-client-core',
  roots: ['<rootDir>/src'],
};
