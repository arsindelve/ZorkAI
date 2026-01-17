/**
 * Planetfall Game State Tests
 *
 * These tests verify the functionality of game state display (location, score, time).
 *
 * NOTE: API Mocking
 * To avoid dependency on the backend API (which may not always be running),
 * these tests use Playwright's route interception to mock API responses.
 * This allows the tests to run reliably without requiring the actual backend.
 */

import {test, expect} from '@playwright/test';
import { closeWelcomeModal, waitForGameResponse, handlePlanetfallRoute, handleSaveGameRoute, handleRestoreGameRoute, handleGetSavedGamesRoute } from './testHelpers';

test.describe('Game State', () => {

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

    test('Location - when API returns a location, that location name is displayed in the header', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the input field to be visible
        await page.waitForSelector('[data-testid="game-input"]', {state: 'visible', timeout: 10000});

        // Verify initial location in header
        const headerLocation = page.locator('[data-testid="header-location"]');

        // Type the "North" command in the input field
        await page.fill('[data-testid="game-input"]', 'North');

        // Make sure the input field has the correct value before proceeding
        await expect(page.locator('[data-testid="game-input"]')).toHaveValue('North');

        // Click the Go button to submit the command
        await page.click('[data-testid="go-button"]');

        // Wait for the game response to appear
        await waitForGameResponse(page);

        // Verify that the location name in the header has changed to "North of House"
        await expect(headerLocation).toHaveText('North of House');
    });

    test('Score - when API returns a score, that score is displayed in the header', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the input field to be visible
        await page.waitForSelector('[data-testid="game-input"]', {state: 'visible', timeout: 10000});

        // Verify initial score in header
        const headerScore = page.locator('[data-testid="header-score"]');

        // Type the "inventory" command in the input field (which returns a score of 10 in the mock response)
        await page.fill('[data-testid="game-input"]', 'inventory');

        // Make sure the input field has the correct value before proceeding
        await expect(page.locator('[data-testid="game-input"]')).toHaveValue('inventory');

        // Click the Go button to submit the command
        await page.click('[data-testid="go-button"]');

        // Wait for the game response to appear
        await waitForGameResponse(page);

        // Verify that the score in the header has changed to "10"
        await expect(headerScore).toHaveText('Score:  10');
    });

    test('Time - when API returns time, that time is displayed in the header', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the input field to be visible
        await page.waitForSelector('[data-testid="game-input"]', {state: 'visible', timeout: 10000});

        // Verify time element in header
        const headerTime = page.locator('[data-testid="header-time"]');

        // Type the "look" command in the input field (which returns a time value of 4654 in the mock response)
        await page.fill('[data-testid="game-input"]', 'look');

        // Make sure the input field has the correct value before proceeding
        await expect(page.locator('[data-testid="game-input"]')).toHaveValue('look');

        // Click the Go button to submit the command
        await page.click('[data-testid="go-button"]');

        // Wait for the game response to appear
        await waitForGameResponse(page);

        // Verify that the time in the header has changed to "4654"
        await expect(headerTime).toHaveText('Time:  4654');
    });

});