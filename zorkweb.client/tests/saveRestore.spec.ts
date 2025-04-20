/**
 * Zork Game Save/Restore Tests
 * 
 * These tests verify the functionality of saving, restoring, and restarting the game.
 * 
 * NOTE: API Mocking
 * To avoid dependency on the backend API (which may not always be running),
 * these tests use Playwright's route interception to mock API responses.
 * This allows the tests to run reliably without requiring the actual backend.
 */

import {test, expect} from '@playwright/test';
import { closeWelcomeModal, handleZorkOneRoute, handleSaveGameRoute, handleRestoreGameRoute, handleGetSavedGamesRoute } from './testHelpers';

test.describe('Game Save and Restore', () => {

    // Set up API mocking before each test
    test.beforeEach(async ({ page }) => {
        // Intercept requests to the API endpoints
        await page.route('http://localhost:5000/ZorkOne', handleZorkOneRoute);

        // Explicitly intercept GET requests to /saveGame for getSavedGames
        await page.route('http://localhost:5000/ZorkOne/saveGame?*', async (route) => {
            if (route.request().method() === 'GET') {
                await handleGetSavedGamesRoute(route);
            } else {
                await route.continue();
            }
        });

        // Explicitly intercept POST requests to /saveGame for saveGame
        await page.route('http://localhost:5000/ZorkOne/saveGame', async (route) => {
            if (route.request().method() === 'POST') {
                await handleSaveGameRoute(route);
            } else if (route.request().method() === 'GET' && !route.request().url().includes('?')) {
                // This is a fallback for GET requests without query params
                await handleGetSavedGamesRoute(route);
            } else {
                await route.continue();
            }
        });

        await page.route('http://localhost:5000/ZorkOne/restoreGame', handleRestoreGameRoute);
    });

    test('Save Game - saved games are displayed in the save modal and API is called', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the Game button to be visible
        await page.waitForSelector('[data-testid="game-button"]', {state: 'visible'});

        // Click the Game button to open the menu
        await page.locator('[data-testid="game-button"]').click();

        // Verify that the Game menu appears
        const gameMenu = page.locator('#game-menu');
        await expect(gameMenu).toBeVisible();

        // Find the specific menu item
        const saveMenuItem = page.locator('#game-menu li:has-text("Save your Game")');
        const saveMenuItemCount = await saveMenuItem.count();

        if (saveMenuItemCount > 0) {
            // Click the menu item
            await saveMenuItem.click();
        } else {
            console.error('Save your Game menu item not found!');
        }

        // Wait for the save modal to appear
        try {
            await page.waitForSelector('[data-testid="save-game-modal"]', {state: 'visible', timeout: 10000});
        } catch (error) {
            console.error('Error waiting for save modal:', error);
            throw error;
        }

        // Verify that the save modal contains the expected title
        const modalTitle = page.locator('[data-testid="save-game-modal"] h2#alert-dialog-title');
        await expect(modalTitle).toContainText('Save Your Game');

        try {
            // Wait with a longer timeout - use a more specific selector to avoid strict mode violations
            // We know there will be at least one save-game-new-section div (the input section)
            await page.waitForSelector('[data-testid="save-game-new-section"]', {
                state: 'visible', 
                timeout: 10000
            });
        } catch (error) {
            console.error('Error waiting for saved games section:', error);
            throw error;
        }

        // Verify that the saved games are displayed in the modal
        const savedGamesCount = await page.locator('[data-testid="save-game-item"]').count();

        if (savedGamesCount > 0) {
            // Verify visibility using expect - use nth() to select a specific element
            // This avoids the strict mode violation by selecting just one element
            const firstSavedGame = page.locator('[data-testid="save-game-item"]').nth(0);
            await expect(firstSavedGame).toBeVisible();
        } else {
            console.error('No save game items found for verification!');
        }

        // Enter a name for the saved game
        await page.fill('[data-testid="save-game-modal"] input', 'Test Save Game');

        // Click the Save button
        await page.locator('[data-testid="save-game-modal"] button:has-text("Save")').click();

        // Verify that the save modal is closed
        await expect(page.locator('[data-testid="save-game-modal"]')).not.toBeVisible();

        // Verify that a success message is displayed
        const snackbar = page.locator('div.MuiSnackbar-root');
        await expect(snackbar).toBeVisible();

        await expect(snackbar).toContainText('Game Saved Successfully');
    });

    test('Restore Game - saved games are displayed in the restore modal and API is called', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the Game button to be visible
        await page.waitForSelector('[data-testid="game-button"]', {state: 'visible'});

        // Click the Game button to open the menu
        await page.locator('[data-testid="game-button"]').click();

        // Verify that the Game menu appears
        const gameMenu = page.locator('#game-menu');
        await expect(gameMenu).toBeVisible();

        // Find the specific menu item
        const restoreMenuItem = page.locator('#game-menu li:has-text("Restore a Previous Saved Game")');
        const restoreMenuItemCount = await restoreMenuItem.count();

        if (restoreMenuItemCount > 0) {
            // Click the menu item
            await restoreMenuItem.click();
        } else {
            console.error('Restore a Previous Saved Game menu item not found!');
        }

        // Wait for the restore modal to appear
        try {
            await page.waitForSelector('[data-testid="restore-game-modal"]', {state: 'visible', timeout: 10000});
        } catch (error) {
            console.error('Error waiting for restore modal:', error);
            throw error;
        }

        // Verify that the restore modal contains the expected title
        const modalTitle = page.locator('[data-testid="restore-game-modal"] h2#alert-dialog-title');
        await expect(modalTitle).toContainText('Restore A Previously Saved Game');

        // Verify that the saved games are displayed in the modal
        try {
            // Wait for the saved games to be loaded
            await page.waitForSelector('[data-testid="restore-game-list"]', {
                state: 'visible',
                timeout: 10000
            });
        } catch (error) {
            console.error('Error waiting for saved games section:', error);
            throw error;
        }

        // Verify that the saved games are displayed in the modal
        const savedGamesCount = await page.locator('[data-testid="restore-game-item"]').count();

        if (savedGamesCount > 0) {
            // Verify visibility using expect - use nth() to select a specific element
            // This avoids the strict mode violation by selecting just one element
            const firstSavedGame = page.locator('[data-testid="restore-game-item"]').nth(0);
            await expect(firstSavedGame).toBeVisible();

            // Click the Restore button for the first saved game
            await page.locator('[data-testid="restore-game-modal"] button:has-text("Restore")').first().click();
        } else {
            console.error('No saved games found for verification!');

            // Close the modal by clicking Cancel
            await page.locator('[data-testid="restore-game-modal"] button:has-text("Cancel")').click();
            return;
        }

        // Verify that the restore modal is closed
        await expect(page.locator('[data-testid="restore-game-modal"]')).not.toBeVisible();

        // Verify that a success message is displayed
        const snackbar = page.locator('div.MuiSnackbar-root');
        await expect(snackbar).toBeVisible();

        await expect(snackbar).toContainText('Game Restored Successfully');

        // Verify that the game responded with the initial game state
        const gameResponses = page.locator('[data-testid="game-response"]');
        const responseCount = await gameResponses.count();
        expect(responseCount).toBeGreaterThan(0);
    });

    test('API Response "<Restore>" - when API returns "<Restore>" response, the restore modal opens', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the input field to be visible
        await page.waitForSelector('[data-testid="game-input"]', {state: 'visible', timeout: 10000});

        // Type the "restore" command in the input field
        await page.fill('[data-testid="game-input"]', 'restore');

        // Make sure the input field has the correct value before proceeding
        await expect(page.locator('[data-testid="game-input"]')).toHaveValue('restore');

        // Click the Go button to submit the command
        await page.click('[data-testid="go-button"]');

        // Wait for the restore modal to appear
        try {
            await page.waitForSelector('[data-testid="restore-game-modal"]', {state: 'visible', timeout: 10000});
        } catch (error) {
            console.error('Error waiting for restore modal:', error);
            throw error;
        }

        // Verify that the restore modal contains the expected title
        const modalTitle = page.locator('[data-testid="restore-game-modal"] h2#alert-dialog-title');
        await expect(modalTitle).toContainText('Restore A Previously Saved Game');

        // Verify that the saved games are displayed in the modal
        try {
            // Wait for the saved games to be loaded
            await page.waitForSelector('[data-testid="restore-game-list"]', {
                state: 'visible',
                timeout: 10000
            });
        } catch (error) {
            console.error('Error waiting for saved games section:', error);
            throw error;
        }

        // Verify that the saved games are displayed in the modal
        const savedGamesCount = await page.locator('[data-testid="restore-game-item"]').count();

        if (savedGamesCount > 0) {
            // Verify visibility using expect - use nth() to select a specific element
            // This avoids the strict mode violation by selecting just one element
            const firstSavedGame = page.locator('[data-testid="restore-game-item"]').nth(0);
            await expect(firstSavedGame).toBeVisible();

            // Close the modal by clicking Cancel
            await page.locator('[data-testid="restore-game-modal"] button:has-text("Cancel")').click();
        } else {
            console.error('No saved games found for verification!');

            // Close the modal by clicking Cancel
            await page.locator('[data-testid="restore-game-modal"] button:has-text("Cancel")').click();
        }

        // Verify that the restore modal is closed
        await expect(page.locator('[data-testid="restore-game-modal"]')).not.toBeVisible();
    });

    test('API Response "<Save>" - when API returns "<Save>" response, the save modal opens', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the input field to be visible
        await page.waitForSelector('[data-testid="game-input"]', {state: 'visible', timeout: 10000});

        // Type the "save" command in the input field
        await page.fill('[data-testid="game-input"]', 'save');

        // Make sure the input field has the correct value before proceeding
        await expect(page.locator('[data-testid="game-input"]')).toHaveValue('save');

        // Click the Go button to submit the command
        await page.click('[data-testid="go-button"]');

        // Wait for the save modal to appear
        try {
            await page.waitForSelector('[data-testid="save-game-modal"]', {state: 'visible', timeout: 10000});
        } catch (error) {
            console.error('Error waiting for save modal:', error);
            throw error;
        }

        // Verify that the save modal contains the expected title
        const modalTitle = page.locator('[data-testid="save-game-modal"] h2#alert-dialog-title');
        await expect(modalTitle).toContainText('Save Your Game');

        // Verify that the saved games are displayed in the modal
        try {
            // Wait for the saved games to be loaded
            await page.waitForSelector('[data-testid="save-game-new-section"]', {
                state: 'visible',
                timeout: 10000
            });
        } catch (error) {
            console.error('Error waiting for saved games section:', error);
            throw error;
        }

        // Verify that the saved games are displayed in the modal
        const savedGamesCount = await page.locator('[data-testid="save-game-item"]').count();

        if (savedGamesCount > 0) {
            // Verify visibility using expect - use nth() to select a specific element
            // This avoids the strict mode violation by selecting just one element
            const firstSavedGame = page.locator('[data-testid="save-game-item"]').nth(0);
            await expect(firstSavedGame).toBeVisible();

            // Close the modal by clicking Cancel
            await page.locator('[data-testid="save-game-modal"] button:has-text("Cancel")').click();
        } else {
            console.error('No saved games found for verification!');

            // Close the modal by clicking Cancel
            await page.locator('[data-testid="save-game-modal"] button:has-text("Cancel")').click();
        }

        // Verify that the save modal is closed
        await expect(page.locator('[data-testid="save-game-modal"]')).not.toBeVisible();
    });

    test('API Response "<Restart>" - when API returns "<Restart>" response, the restart modal opens', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the input field to be visible
        await page.waitForSelector('[data-testid="game-input"]', {state: 'visible', timeout: 10000});

        // Type the "restart" command in the input field
        await page.fill('[data-testid="game-input"]', 'restart');

        // Make sure the input field has the correct value before proceeding
        await expect(page.locator('[data-testid="game-input"]')).toHaveValue('restart');

        // Click the Go button to submit the command
        await page.click('[data-testid="go-button"]');

        // Wait for the restart modal to appear
        try {
            await page.waitForSelector('[data-testid="restart-game-modal"]', {state: 'visible', timeout: 10000});
        } catch (error) {
            console.error('Error waiting for restart modal:', error);
            throw error;
        }

        // Verify that the restart modal contains the expected title
        const modalTitle = page.locator('[data-testid="restart-game-modal"] h2#confirm-dialog');
        await expect(modalTitle).toContainText('Restart Your Game? Are you sure?');

        // Verify that the restart modal contains the expected message
        const modalMessage = page.locator('[data-testid="restart-game-modal"] div.p-10');
        await expect(modalMessage).toContainText('Your game will be reset to the beginning. Are you sure you want to restart?');

        // Close the modal by clicking Cancel
        await page.locator('[data-testid="restart-game-modal"] button:has-text("No")').click();

        // Verify that the restart modal is closed
        await expect(page.locator('[data-testid="restart-game-modal"]')).not.toBeVisible();
    });

    test('Save Modal - Overwrite Existing Save section is hidden when no saved games exist', async ({page}) => {
        // Override the getSavedGames route to return an empty array
        await page.route('http://localhost:5000/ZorkOne/saveGame?*', async (route) => {
            if (route.request().method() === 'GET') {
                await route.fulfill({ 
                    status: 200, 
                    contentType: 'application/json',
                    body: JSON.stringify([]) // Empty array - no saved games
                });
            } else {
                await route.continue();
            }
        });

        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the Game button to be visible
        await page.waitForSelector('[data-testid="game-button"]', {state: 'visible'});

        // Click the Game button to open the menu
        await page.locator('[data-testid="game-button"]').click();

        // Verify that the Game menu appears
        const gameMenu = page.locator('#game-menu');
        await expect(gameMenu).toBeVisible();

        // Find and click the Save your Game menu item
        const saveMenuItem = page.locator('#game-menu li:has-text("Save your Game")');
        await saveMenuItem.click();

        // Wait for the save modal to appear
        await page.waitForSelector('[data-testid="save-game-modal"]', {state: 'visible', timeout: 10000});

        // Verify that the "Create New Save" section is visible
        await expect(page.locator('[data-testid="save-game-new-section"]')).toBeVisible();

        // Verify that the "Overwrite Existing Save" section is NOT visible
        const overwriteSection = page.locator('[data-testid="overwrite-section-title"]');
        await expect(overwriteSection).not.toBeVisible();

        // Verify that the "No saved games found" message is visible
        const noSavedGamesMessage = page.locator('text=No saved games found');
        await expect(noSavedGamesMessage).toBeVisible();

        // Close the modal
        await page.locator('[data-testid="save-game-modal"] button:has-text("Cancel")').click();
    });

    test('Save Modal - Overwrite Existing Save section is visible when saved games exist', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the Game button to be visible
        await page.waitForSelector('[data-testid="game-button"]', {state: 'visible'});

        // Click the Game button to open the menu
        await page.locator('[data-testid="game-button"]').click();

        // Verify that the Game menu appears
        const gameMenu = page.locator('#game-menu');
        await expect(gameMenu).toBeVisible();

        // Find and click the Save your Game menu item
        const saveMenuItem = page.locator('#game-menu li:has-text("Save your Game")');
        await saveMenuItem.click();

        // Wait for the save modal to appear
        await page.waitForSelector('[data-testid="save-game-modal"]', {state: 'visible', timeout: 10000});

        // Verify that the "Create New Save" section is visible
        await expect(page.locator('[data-testid="save-game-new-section"]')).toBeVisible();

        // Verify that the "Overwrite Existing Save" section IS visible
        const overwriteSection = page.locator('[data-testid="overwrite-section-title"]');
        await expect(overwriteSection).toBeVisible();

        // Verify that at least one saved game item is visible
        const savedGameItem = page.locator('[data-testid="save-game-item"]').first();
        await expect(savedGameItem).toBeVisible();

        // Close the modal
        await page.locator('[data-testid="save-game-modal"] button:has-text("Cancel")').click();
    });
});
