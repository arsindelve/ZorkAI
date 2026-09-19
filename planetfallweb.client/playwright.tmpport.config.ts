import baseConfig from './playwright.config';
import {defineConfig} from '@playwright/test';
export default defineConfig({
    ...baseConfig,
    retries: 0,
    use: {...baseConfig.use, baseURL: 'http://localhost:5199'},
    webServer: {
        command: 'npm run dev -- --port 5199 --strictPort',
        url: 'http://localhost:5199',
        reuseExistingServer: false,
        stdout: 'pipe',
        stderr: 'pipe',
    },
});
