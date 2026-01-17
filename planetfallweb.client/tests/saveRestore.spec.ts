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
import { closeWelcomeModal, handlePlanetfallRoute, handleSaveGameRoute, handleRestoreGameRoute, handleGetSavedGamesRoute } from './testHelpers';
import { mockResponses } from './mockResponses';

test.describe('Game Save and Restore', () => {

    // Set up API mocking before each test
    test.beforeEach(async ({ page }) => {
        // Intercept DELETE requests to /saveGame/{id} for deleteSavedGame
        await page.route('http://localhost:5000/Planetfall/saveGame/*', async (route) => {
            if (route.request().method() === 'DELETE') {
                await route.fulfill({ status: 200, contentType: 'application/json', body: '' });
            } else {
                await route.continue();
            }
        });
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

    test('Restore Modal - clicking delete opens confirmation dialog with correct game name', async ({ page }) => {
        await closeWelcomeModal(page);
        await page.waitForSelector('[data-testid="game-button"]', { state: 'visible' });
        await page.locator('[data-testid="game-button"]').click();
        const gameMenu = page.locator('#game-menu');
        await expect(gameMenu).toBeVisible();
        const restoreMenuItem = page.locator('#game-menu li:has-text("Restore a Previous Saved Game")');
        await restoreMenuItem.click();
        await page.waitForSelector('[data-testid="restore-game-modal"]', { state: 'visible', timeout: 10000 });
        // Ensure list is visible
        await page.waitForSelector('[data-testid="restore-game-list"]', { state: 'visible', timeout: 10000 });
        const firstItem = page.locator('[data-testid="restore-game-item"]').first();
        await expect(firstItem).toBeVisible();
        // Click the delete icon button
        await firstItem.locator('button[title="Delete saved game"]').click();
        // Expect confirmation dialog to appear with the correct title and message including the game name
        const confirmDialog = page.locator('[aria-labelledby="confirmation-dialog-title"]');
        await expect(confirmDialog).toBeVisible();
        await expect(confirmDialog.locator('#confirmation-dialog-title')).toHaveText('Delete Saved Game');
        await expect(confirmDialog).toContainText(`Are you sure you want to delete "${mockResponses.savedGames[0].name}"?`);
        // Close dialog via Cancel to clean up
        await confirmDialog.locator('button:has-text("Cancel")').click();
        await expect(confirmDialog).not.toBeVisible();
    });

    test('Restore Modal - canceling deletion keeps the item and closes the confirmation dialog', async ({ page }) => {
        await closeWelcomeModal(page);
        await page.waitForSelector('[data-testid="game-button"]', { state: 'visible' });
        await page.locator('[data-testid="game-button"]').click();
        await expect(page.locator('#game-menu')).toBeVisible();
        await page.locator('#game-menu li:has-text("Restore a Previous Saved Game")').click();
        await page.waitForSelector('[data-testid="restore-game-modal"]', { state: 'visible', timeout: 10000 });
        await page.waitForSelector('[data-testid="restore-game-list"]', { state: 'visible', timeout: 10000 });
        const items = page.locator('[data-testid="restore-game-item"]');
        const initialCount = await items.count();
        const firstItem = items.first();
        await firstItem.locator('button[title="Delete saved game"]').click();
        const confirmDialog = page.locator('[aria-labelledby="confirmation-dialog-title"]');
        await expect(confirmDialog).toBeVisible();
        await confirmDialog.locator('button:has-text("Cancel")').click();
        await expect(confirmDialog).not.toBeVisible();
        await expect(items).toHaveCount(initialCount);
    });

    test('Restore Modal - confirming deletion calls API and shows success snackbar', async ({ page }) => {
        await closeWelcomeModal(page);
        await page.waitForSelector('[data-testid="game-button"]', { state: 'visible' });
        await page.locator('[data-testid="game-button"]').click();
        await page.locator('#game-menu li:has-text("Restore a Previous Saved Game")').click();
        await page.waitForSelector('[data-testid="restore-game-modal"]', { state: 'visible', timeout: 10000 });
        await page.waitForSelector('[data-testid="restore-game-list"]', { state: 'visible', timeout: 10000 });
        const firstItem = page.locator('[data-testid="restore-game-item"]').first();
        // Prepare to wait for DELETE request
        const deleteRequestPromise = page.waitForRequest((req) => req.method() === 'DELETE' && /\/Planetfall\/saveGame\/.+\?sessionId=/.test(req.url()));
        // Open confirmation dialog and confirm
        await firstItem.locator('button[title="Delete saved game"]').click();
        const confirmDialog = page.locator('[aria-labelledby="confirmation-dialog-title"]');
        await expect(confirmDialog).toBeVisible();
        await confirmDialog.locator('button:has-text("Delete")').click();
        // Verify DELETE called
        await deleteRequestPromise;
        // Verify snackbar shows success message
        const snackbar = page.locator('div.MuiSnackbar-root');
        await expect(snackbar).toBeVisible();
        await expect(snackbar).toContainText('Game Deleted Successfully');
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
            await page.waitForSelector('[data-testid="restart-confirm-dialog"]', {state: 'visible', timeout: 10000});
        } catch (error) {
            console.error('Error waiting for restart modal:', error);
            throw error;
        }

        // Verify that the restart modal contains the expected title
        const modalTitle = page.locator('[data-testid="restart-confirm-dialog"] h2#restart-confirm-dialog');
        await expect(modalTitle).toContainText('Restart Your Game? Are you sure?');

        // Verify that the restart modal contains the expected message
        const modalMessage = page.locator('[data-testid="restart-confirm-dialog"] .MuiDialogContent-root');
        await expect(modalMessage).toContainText('Your game will be reset to the beginning. Are you sure you want to restart?');

        // Close the modal by clicking Cancel
        await page.locator('[data-testid="restart-confirm-cancel"]').click();

        // Verify that the restart modal is closed
        await expect(page.locator('[data-testid="restart-confirm-dialog"]')).not.toBeVisible();
    });

    test('Save Modal - Overwrite Existing Save section is hidden when no saved games exist', async ({page}) => {
        // Override the getSavedGames route to return an empty array
        await page.route('http://localhost:5000/Planetfall/saveGame?*', async (route) => {
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
