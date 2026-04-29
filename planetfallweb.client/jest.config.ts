export default {
  preset: 'ts-jest',
  testEnvironment: 'jsdom',
  moduleNameMapper: {
    // Handle CSS imports (with CSS modules)
    '\\.css$': 'identity-obj-proxy',
    // Handle image imports
    '\\.(jpg|jpeg|png|gif|webp|svg)$': '<rootDir>/__mocks__/fileMock.js',
    // Auto-mock Mixpanel from shared-types (components import from ../utils/Mixpanel)
    '/utils/Mixpanel': '<rootDir>/src/__mocks__/Mixpanel.ts',
    // Auto-mock GameContext from shared-types
    '/context/GameContext': '<rootDir>/src/__mocks__/GameContext.ts',
  },
  setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
  testMatch: ['<rootDir>/src/**/__tests__/**/*.{ts,tsx}', '<rootDir>/src/**/*.{spec,test}.{ts,tsx}'],
  testPathIgnorePatterns: ['<rootDir>/node_modules/', '<rootDir>/src/__skipped_tests__/'],
  transform: {
    '^.+\\.(ts|tsx)$': ['ts-jest', {
      tsconfig: 'tsconfig.json',
    }],
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json', 'node'],
};
