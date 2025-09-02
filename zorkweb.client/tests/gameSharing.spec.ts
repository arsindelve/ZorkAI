/**
 * Zork Game Sharing Tests
 * 
 * These tests verify the functionality of the new game sharing features:
 * - Share This Game (shows session ID for sharing)
 * - Load Shared Game (load games from another session)
 * 
 * NOTE: API Mocking
 * To avoid dependency on the backend API (which may not always be running),
 * these tests use Playwright's route interception to mock API responses.
 * This allows the tests to run reliably without requiring the actual backend.
 */

import { test, expect } from '@playwright/test';
import { closeWelcomeModal, handleZorkOneRoute, handleGetSavedGamesRoute } from './testHelpers';

test.describe('Game Sharing Features', () => {
    
    // Set up API mocking before each test
    test.beforeEach(async ({ page }) => {
        // Intercept requests to the API endpoints
        await page.route('http://localhost:5000/ZorkOne', handleZorkOneRoute);
        
        // Mock saved games endpoint
        await page.route('http://localhost:5000/ZorkOne/saveGame?*', async (route) => {
            if (route.request().method() === 'GET') {
                await handleGetSavedGamesRoute(route);
            } else {
                await route.continue();
            }
        });
        
        // Mock shared games endpoints
        await page.route('http://localhost:5000/ZorkOne/shareGame/*', async (route) => {
            const method = route.request().method();
            const url = route.request().url();
            
            if (method === 'GET') {
                // Mock getSharedGames response
                await route.fulfill({
                    status: 200,
                    contentType: 'application/json',
                    body: JSON.stringify([
                        {
                            id: "shared-game-1",
                            name: "Underground Adventure",
                            savedOn: "2023-12-01T10:30:00Z"
                        },
                        {
                            id: "shared-game-2", 
                            name: "Treasure Hunt Save",
                            savedOn: "2023-12-01T15:45:00Z"
                        }
                    ])
                });
            } else if (method === 'POST') {
                // Mock copySharedGame response
                await route.fulfill({
                    status: 200,
                    contentType: 'application/json',
                    body: JSON.stringify({ success: true })
                });
            } else {
                await route.continue();
            }
        });
    });

    test('Share This Game - opens share modal and displays session ID', async ({ page }) => {
        // Close the welcome modal
        await closeWelcomeModal(page);

        // Wait for the Game button to be visible
        await page.waitForSelector('[data-testid="game-button"]', { state: 'visible' });

        // Click the Game button to open the menu
        await page.locator('[data-testid="game-button"]').click();

        // Verify the game menu is open
        await expect(page.locator('#game-menu')).toBeVisible();

        // Click "Share This Game" menu item
        await page.locator('#game-menu li:has-text("Share This Game")').click();

        // Verify the share modal opens
        await expect(page.locator('[data-testid="share-game-modal"]')).toBeVisible();

        // Verify modal title
        const modalTitle = page.locator('[data-testid="share-game-modal"] h2:has-text("Share Your Game")');
        await expect(modalTitle).toBeVisible();

        // Verify session ID is displayed
        const sessionIdDisplay = page.locator('[data-testid="session-id-display"]');
        await expect(sessionIdDisplay).toBeVisible();
        
        // Verify session ID format (15 alphanumeric characters)
        const sessionIdText = await sessionIdDisplay.textContent();
        expect(sessionIdText).toMatch(/^[A-Za-z0-9]{15}$/);

        // Verify copy button is present
        const copyButton = page.locator('[data-testid="copy-session-id-button"]');
        await expect(copyButton).toBeVisible();

        // Test copy functionality (mock clipboard)
        await page.evaluate(() => {
            navigator.clipboard = {
                writeText: async (text: string) => Promise.resolve()
            };
        });

        // Click copy button
        await copyButton.click();

        // Verify success message appears
        await expect(page.locator('div[role="alert"]:has-text("Session ID copied to clipboard!")')).toBeVisible();

        // Close the modal
        await page.locator('[data-testid="share-game-modal"] button:has-text("Close")').click();

        // Verify modal is closed
        await expect(page.locator('[data-testid="share-game-modal"]')).not.toBeVisible();
    });

    test('Load Shared Game - opens modal and allows browsing shared games', async ({ page }) => {
        // Close the welcome modal
        await closeWelcomeModal(page);

        // Wait for the Game button to be visible
        await page.waitForSelector('[data-testid="game-button"]', { state: 'visible' });

        // Click the Game button to open the menu
        await page.locator('[data-testid="game-button"]').click();

        // Verify the game menu is open
        await expect(page.locator('#game-menu')).toBeVisible();

        // Click "Load Shared Game" menu item
        await page.locator('#game-menu li:has-text("Load Shared Game")').click();

        // Verify the load shared modal opens
        await expect(page.locator('[data-testid="load-shared-game-modal"]')).toBeVisible();

        // Verify modal title
        const modalTitle = page.locator('[data-testid="load-shared-game-modal"] h2:has-text("Load Shared Game")');
        await expect(modalTitle).toBeVisible();

        // Verify session ID input field is present
        const sessionIdInput = page.locator('[data-testid="session-id-input"]');
        await expect(sessionIdInput).toBeVisible();

        // Test entering a valid session ID
        await sessionIdInput.fill('ABC123DEF456789');

        // Verify load games button is enabled
        const loadGamesButton = page.locator('[data-testid="load-shared-games-button"]');
        await expect(loadGamesButton).toBeEnabled();

        // Click load games button
        await loadGamesButton.click();

        // Wait for shared games to load and display
        await expect(page.locator('[data-testid="shared-game-item"]')).toHaveCount(2);

        // Verify first shared game is displayed correctly
        const firstGame = page.locator('[data-testid="shared-game-item"]').first();
        await expect(firstGame).toContainText('Underground Adventure');
        await expect(firstGame).toContainText('December 1st, 10:30 am');

        // Verify second shared game is displayed correctly
        const secondGame = page.locator('[data-testid="shared-game-item"]').nth(1);
        await expect(secondGame).toContainText('Treasure Hunt Save');
        await expect(secondGame).toContainText('December 1st, 3:45 pm');

        // Test copying a shared game
        const copyGameButton = firstGame.locator('button:has-text("Copy Game")');
        await expect(copyGameButton).toBeVisible();
        await copyGameButton.click();

        // The modal should close after successful copy (this would normally reload saved games)
        await expect(page.locator('[data-testid="load-shared-game-modal"]')).not.toBeVisible();
    });

    test('Load Shared Game - validates session ID format', async ({ page }) => {
        // Close the welcome modal
        await closeWelcomeModal(page);

        // Open the Load Shared Game modal
        await page.locator('[data-testid="game-button"]').click();
        await page.locator('#game-menu li:has-text("Load Shared Game")').click();

        const sessionIdInput = page.locator('[data-testid="session-id-input"]');
        const loadGamesButton = page.locator('[data-testid="load-shared-games-button"]');

        // Test empty input
        await expect(loadGamesButton).toBeDisabled();

        // Test invalid format - too short
        await sessionIdInput.fill('ABC123');
        await expect(page.locator('p:has-text("Session ID must be exactly 15 alphanumeric characters")')).toBeVisible();
        await expect(loadGamesButton).toBeDisabled();

        // Test invalid format - too long
        await sessionIdInput.fill('ABC123DEF456789XX');
        await expect(page.locator('p:has-text("Session ID must be exactly 15 alphanumeric characters")')).toBeVisible();
        await expect(loadGamesButton).toBeDisabled();

        // Test invalid characters
        await sessionIdInput.fill('ABC-123-DEF-456');
        await expect(page.locator('p:has-text("Session ID must be exactly 15 alphanumeric characters")')).toBeVisible();
        await expect(loadGamesButton).toBeDisabled();

        // Test valid format
        await sessionIdInput.fill('ABC123DEF456789');
        await expect(page.locator('p:has-text("Session ID must be exactly 15 alphanumeric characters")')).not.toBeVisible();
        await expect(loadGamesButton).toBeEnabled();

        // Close the modal
        await page.locator('[data-testid="load-shared-game-modal"] button:has-text("Cancel")').click();
        await expect(page.locator('[data-testid="load-shared-game-modal"]')).not.toBeVisible();
    });

    test('Load Shared Game - handles no games found scenario', async ({ page }) => {
        // Close the welcome modal
        await closeWelcomeModal(page);

        // Mock empty shared games response for this test
        await page.route('http://localhost:5000/ZorkOne/shareGame/*', async (route) => {
            if (route.request().method() === 'GET') {
                await route.fulfill({
                    status: 200,
                    contentType: 'application/json',
                    body: JSON.stringify([]) // Empty array
                });
            } else {
                await route.continue();
            }
        });

        // Open the Load Shared Game modal
        await page.locator('[data-testid="game-button"]').click();
        await page.locator('#game-menu li:has-text("Load Shared Game")').click();

        // Enter a valid session ID
        const sessionIdInput = page.locator('[data-testid="session-id-input"]');
        await sessionIdInput.fill('ABC123DEF456789');

        // Click load games button
        const loadGamesButton = page.locator('[data-testid="load-shared-games-button"]');
        await loadGamesButton.click();

        // Verify error message is displayed
        await expect(page.locator('[role="alert"]:has-text("No saved games found for this Session ID")')).toBeVisible();

        // Verify no game items are displayed
        await expect(page.locator('[data-testid="shared-game-item"]')).toHaveCount(0);

        // Close the modal
        await page.locator('[data-testid="load-shared-game-modal"] button:has-text("Cancel")').click();
    });

    test('Load Shared Game - handles API error scenario', async ({ page }) => {
        // Close the welcome modal
        await closeWelcomeModal(page);

        // Mock API error for this test
        await page.route('http://localhost:5000/ZorkOne/shareGame/*', async (route) => {
            if (route.request().method() === 'GET') {
                await route.fulfill({
                    status: 404,
                    contentType: 'application/json',
                    body: JSON.stringify({ error: 'Session not found' })
                });
            } else {
                await route.continue();
            }
        });

        // Open the Load Shared Game modal
        await page.locator('[data-testid="game-button"]').click();
        await page.locator('#game-menu li:has-text("Load Shared Game")').click();

        // Enter a valid session ID
        const sessionIdInput = page.locator('[data-testid="session-id-input"]');
        await sessionIdInput.fill('ABC123DEF456789');

        // Click load games button
        const loadGamesButton = page.locator('[data-testid="load-shared-games-button"]');
        await loadGamesButton.click();

        // Verify error message is displayed
        await expect(page.locator('[role="alert"]:has-text("Failed to load shared games. Please check the Session ID and try again.")')).toBeVisible();

        // Close the modal
        await page.locator('[data-testid="load-shared-game-modal"] button:has-text("Cancel")').click();
    });

    test('Load Shared Game - supports keyboard navigation', async ({ page }) => {
        // Close the welcome modal
        await closeWelcomeModal(page);

        // Open the Load Shared Game modal
        await page.locator('[data-testid="game-button"]').click();
        await page.locator('#game-menu li:has-text("Load Shared Game")').click();

        // Enter a valid session ID
        const sessionIdInput = page.locator('[data-testid="session-id-input"]');
        await sessionIdInput.fill('ABC123DEF456789');

        // Test pressing Enter to load games
        await sessionIdInput.press('Enter');

        // Wait for shared games to load
        await expect(page.locator('[data-testid="shared-game-item"]')).toHaveCount(2);

        // Verify games are displayed
        await expect(page.locator('[data-testid="shared-game-item"]').first()).toContainText('Underground Adventure');
    });

    test('Game menu shows correct count of items including sharing options', async ({ page }) => {
        // Close the welcome modal
        await closeWelcomeModal(page);

        // Click the Game button
        await page.locator('[data-testid="game-button"]').click();

        // Verify the game menu has 6 items (including the new sharing options)
        const menuItems = page.locator('#game-menu li');
        await expect(menuItems).toHaveCount(6);

        // Verify all menu items are present
        await expect(page.locator('#game-menu li:has-text("Restart Your Game")')).toBeVisible();
        await expect(page.locator('#game-menu li:has-text("Restore a Previous Saved Game")')).toBeVisible();
        await expect(page.locator('#game-menu li:has-text("Save your Game")')).toBeVisible();
        await expect(page.locator('#game-menu li:has-text("Share This Game")')).toBeVisible();
        await expect(page.locator('#game-menu li:has-text("Load Shared Game")')).toBeVisible();
        await expect(page.locator('#game-menu li:has-text("Copy Game Transcript")')).toBeVisible();
    });
});