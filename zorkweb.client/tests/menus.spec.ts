/**
 * Zork Game Menu Tests
 * 
 * These tests verify the functionality of various menus in the game.
 * 
 * NOTE: API Mocking
 * To avoid dependency on the backend API (which may not always be running),
 * these tests use Playwright's route interception to mock API responses.
 * This allows the tests to run reliably without requiring the actual backend.
 */

import {test, expect} from '@playwright/test';
import { closeWelcomeModal, waitForGameResponse, handleZorkOneRoute, handleSaveGameRoute, handleRestoreGameRoute, handleGetSavedGamesRoute } from './testHelpers';

test.describe('Game Menus', () => {

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

    test('About menu - when About button is clicked, the about menu appears', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the About button to be visible
        await page.waitForSelector('[data-testid="about-button"]', {state: 'visible'});

        // Click the About button
        await page.locator('[data-testid="about-button"]').click();

        // Verify that the About menu appears
        const aboutMenu = page.locator('#basic-menu');
        await expect(aboutMenu).toBeVisible();

        // Verify that the menu contains expected items
        const menuItems = page.locator('#basic-menu li');
        await expect(menuItems).toHaveCount(10); // There are 10 menu items in the AboutMenu component

        // Verify a specific menu item is present
        const whatIsThisGameItem = page.locator('#basic-menu li:has-text("What is this game?")');
        await expect(whatIsThisGameItem).toBeVisible();
    });

    test('Game menu - when Game button is clicked, the game menu appears', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the Game button to be visible
        await page.waitForSelector('[data-testid="game-button"]', {state: 'visible'});

        // Click the Game button
        await page.locator('[data-testid="game-button"]').click();

        // Verify that the Game menu appears
        const gameMenu = page.locator('#game-menu');
        await expect(gameMenu).toBeVisible();

        // Verify that the menu contains expected items
        const menuItems = page.locator('#game-menu li');
        await expect(menuItems).toHaveCount(4); // There are 4 menu items in the FunctionsMenu component

        // Verify a specific menu item is present
        const restartGameItem = page.locator('#game-menu li:has-text("Restart Your Game")');
        await expect(restartGameItem).toBeVisible();
    });

    test('Verbs menu - player can select a verb from the menu', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the Verbs button to be visible
        await page.waitForSelector('button:has-text("Verbs")', {state: 'visible'});

        // Click the Verbs button to open the menu
        await page.click('button:has-text("Verbs")');

        // Wait for the menu to appear
        await page.waitForSelector('ul[role="menu"]', {state: 'visible'});

        // Click a verb from the menu (e.g., "examine")
        await page.click('li:has-text("examine")');

        // Verify that the verb is added to the input field
        const inputValue = await page.inputValue('[data-testid="game-input"]');
        expect(inputValue).toBe('examine ');

        // Verify that the menu is closed
        const menu = page.locator('ul[role="menu"]');
        await expect(menu).not.toBeVisible();
    });

    test('Commands menu - player can select and execute a command', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the Commands button to be visible
        await page.waitForSelector('button:has-text("Commands")', {state: 'visible'});

        // Click the Commands button to open the menu
        await page.click('button:has-text("Commands")');

        // Wait for the menu to appear
        await page.waitForSelector('ul[role="menu"]', {state: 'visible'});

        // Click a command from the menu (e.g., "look")
        await page.click('li:has-text("look")');

        // Wait for any response to appear after submitting the command
        // This is more reliable than waiting for a specific text
        await waitForGameResponse(page);

        // Now check if our specific command was echoed
        const commandEcho = page.locator('p.text-lime-600').filter({ hasText: /look/i });
        await expect(commandEcho).toBeVisible({ timeout: 5000 });

        // Verify that the game responded to the command
        const gameResponses = page.locator('[data-testid="game-response"]');

        // Get the count of responses (should be more than the initial loading text)
        const responseCount = await gameResponses.count();
        expect(responseCount).toBeGreaterThan(1);

        // Verify that the menu is closed
        const menu = page.locator('ul[role="menu"]');
        await expect(menu).not.toBeVisible();

        // Verify that the input field is cleared after submission
        const inputValue = await page.inputValue('[data-testid="game-input"]');
        expect(inputValue).toBe('');
    });

    test('Inventory menu - player can select an item from the inventory menu', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the input field to be visible
        await page.waitForSelector('[data-testid="game-input"]', {state: 'visible', timeout: 10000});

        // Type the "inventory" command in the input field to trigger the API response with inventory items
        await page.fill('[data-testid="game-input"]', 'inventory');

        // Make sure the input field has the correct value before proceeding
        await expect(page.locator('[data-testid="game-input"]')).toHaveValue('inventory');

        // Click the Go button to submit the command
        await page.click('[data-testid="go-button"]');

        // Wait for the game response to appear
        await waitForGameResponse(page);

        // Verify that the Inventory button is visible
        const inventoryButton = page.locator('button:has-text("Inventory")');
        await expect(inventoryButton).toBeVisible();

        // Click the Inventory button to open the menu
        await inventoryButton.click();

        // Wait for the menu to appear
        await page.waitForSelector('ul[role="menu"]', {state: 'visible'});

        // Click an item from the menu (e.g., "leaflet")
        await page.click('li:has-text("leaflet")');

        // Verify that the item is added to the input field
        const inputValue = await page.inputValue('[data-testid="game-input"]');
        expect(inputValue).toContain('leaflet');

        // Verify that the menu is closed
        const menu = page.locator('ul[role="menu"]');
        await expect(menu).not.toBeVisible();
    });

    test('Inventory - when API returns items in inventory array, they are listed in the Inventory menu', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the input field to be visible
        await page.waitForSelector('[data-testid="game-input"]', {state: 'visible', timeout: 10000});

        // Type the "inventory" command in the input field
        await page.fill('[data-testid="game-input"]', 'inventory');

        // Make sure the input field has the correct value before proceeding
        await expect(page.locator('[data-testid="game-input"]')).toHaveValue('inventory');

        // Click the Go button to submit the command
        await page.click('[data-testid="go-button"]');

        // Wait for the game response to appear
        await waitForGameResponse(page);

        // Verify that the Inventory button is visible
        const inventoryButton = page.locator('button:has-text("Inventory")');
        await expect(inventoryButton).toBeVisible();

        // Click the Inventory button
        await inventoryButton.click();

        // Wait for the menu to appear
        await page.waitForSelector('ul[role="menu"]', {state: 'visible'});

        // Verify that the menu contains the expected items
        const menuItems = page.locator('ul[role="menu"] li');

        // Check the count of menu items
        await expect(menuItems).toHaveCount(3); // There should be 3 items in the inventory

        // Verify specific menu items are present
        await expect(page.locator('li:has-text("leaflet")')).toBeVisible();
        await expect(page.locator('li:has-text("brass lantern")')).toBeVisible();
        await expect(page.locator('li:has-text("sword")')).toBeVisible();

        // Click outside to close the menu
        await page.click('body', { position: { x: 0, y: 0 } });

        // Verify that the menu is closed
        await expect(page.locator('ul[role="menu"]')).not.toBeVisible();
    });

    test('Inventory - when API returns empty inventory array, the Inventory button is not visible', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the input field to be visible
        await page.waitForSelector('[data-testid="game-input"]', {state: 'visible', timeout: 10000});

        // Type the "drop all" command in the input field
        await page.fill('[data-testid="game-input"]', 'drop all');

        // Make sure the input field has the correct value before proceeding
        await expect(page.locator('[data-testid="game-input"]')).toHaveValue('drop all');

        // Click the Go button to submit the command
        await page.click('[data-testid="go-button"]');

        // Wait for the game response to appear
        await waitForGameResponse(page);

        // Verify that the Inventory button is not visible
        const inventoryButton = page.locator('button:has-text("Inventory")');
        await expect(inventoryButton).not.toBeVisible();
    });

});