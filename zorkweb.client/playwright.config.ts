import { defineConfig, devices } from '@playwright/test';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

// Get the directory name of the current module
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Ensure test artifacts directory exists
const testArtifactsDir = path.join(__dirname, 'test-artifacts');
if (!fs.existsSync(testArtifactsDir)) {
  fs.mkdirSync(testArtifactsDir, { recursive: true });
}

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : 2,
  // Use multiple reporters for better debugging
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['json', { outputFile: 'test-artifacts/test-results.json' }],
    ['list'] // Shows test progress in the console
  ],
  // Output directory for test artifacts
  outputDir: 'test-artifacts',
  use: {
    baseURL: 'http://localhost:5173',
    // Always capture traces for better debugging
    trace: 'on',
    // Capture screenshots on failure and at the end of each test
    screenshot: 'on',
    // Record video for failed tests
    video: 'retain-on-failure',
    // Capture console logs
    contextOptions: {
      logger: {
        isEnabled: (name) => name === 'browser',
        log: (name, severity, message) => console.log(`${name} ${severity}: ${message}`)
      }
    },
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
    stdout: 'pipe',
    stderr: 'pipe',
  },
});
