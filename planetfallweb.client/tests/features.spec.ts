/**
 * Zork Game Features Tests
 * 
 * These tests verify miscellaneous features of the game.
 * 
 * NOTE: API Mocking
 * To avoid dependency on the backend API (which may not always be running),
 * these tests use Playwright's route interception to mock API responses.
 * This allows the tests to run reliably without requiring the actual backend.
 */

import {test, expect} from '@playwright/test';
import { closeWelcomeModal, handlePlanetfallRoute, handleSaveGameRoute, handleRestoreGameRoute, handleGetSavedGamesRoute } from './testHelpers';

test.describe('Game Features', () => {

    // Set up API mocking before each test
    test.beforeEach(async ({ page }) => {
        // Intercept requests to the API endpoints
        await page.route('http://localhost:5000/Planetfall', handlePlanetfallRoute);

        // Explicitly intercept GET requests to /saveGame for getSavedGames
        await page.route('http://localhost:5000/Planetfall/saveGame?*', async (route) => {
            if (route.request().method() === 'GET') {
                await handleGetSavedGamesRoute(route);
            } else {
                await route.continue();
            }
        });

        // Explicitly intercept POST requests to /saveGame for saveGame
        await page.route('http://localhost:5000/Planetfall/saveGame', async (route) => {
            if (route.request().method() === 'POST') {
                await handleSaveGameRoute(route);
            } else if (route.request().method() === 'GET' && !route.request().url().includes('?')) {
                // This is a fallback for GET requests without query params
                await handleGetSavedGamesRoute(route);
            } else {
                await route.continue();
            }
        });

        await page.route('http://localhost:5000/Planetfall/restoreGame', handleRestoreGameRoute);
    });

    test('Copy game transcript - copy game transcript functionality works and displays success message', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the Game button to be visible
        await page.waitForSelector('[data-testid="game-button"]', {state: 'visible'});

        // Click the Game button
        await page.locator('[data-testid="game-button"]').click();

        // Wait for the Game menu to appear
        await page.waitForSelector('#game-menu', {state: 'visible'});

        // Mock the clipboard API
        await page.evaluate(() => {
            // Create a mock clipboard API
            Object.defineProperty(navigator, 'clipboard', {
                value: {
                    writeText: () => Promise.resolve(),
                    write: () => Promise.resolve()
                },
                writable: true
            });
        });

        // Click the "Copy Game Transcript" menu item
        await page.locator('#game-menu li:has-text("Copy Game Transcript")').click();

        // Verify that a success message is displayed
        const snackbar = page.locator('div.MuiSnackbar-root');
        await expect(snackbar).toBeVisible();
        await expect(snackbar).toContainText('Game text copied to clipboard with formatting');
    });

});