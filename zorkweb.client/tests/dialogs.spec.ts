/**
 * Zork Game Dialog Tests
 * 
 * These tests verify the functionality of various dialogs in the game.
 * 
 * NOTE: API Mocking
 * To avoid dependency on the backend API (which may not always be running),
 * these tests use Playwright's route interception to mock API responses.
 * This allows the tests to run reliably without requiring the actual backend.
 */

import {test, expect} from '@playwright/test';
import { closeWelcomeModal, handleZorkOneRoute, handleSaveGameRoute, handleRestoreGameRoute, handleGetSavedGamesRoute } from './testHelpers';

test.describe('Game Dialogs', () => {

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

    test('Welcome dialog - welcome dialog appears on first visit and can be closed', async ({page}) => {
        // Navigate to the application
        await page.goto('/');

        // Wait for the welcome modal to be visible
        await page.waitForSelector('[data-testid="welcome-modal"]', {state: 'visible'});

        // Verify that the welcome modal contains the expected title
        const modalTitle = page.locator('[data-testid="welcome-modal"] #alert-dialog-title');
        await expect(modalTitle).toContainText('Welcome to Zork AI - A Modern Reimagining of the 1980s Classic!');

        // Verify that the welcome modal contains some expected content
        const modalContent = page.locator('[data-testid="welcome-modal"] #alert-dialog-description');
        await expect(modalContent).toContainText('This is a modern re-imagining of the iconic text adventure game Zork I.');
        await expect(modalContent).toContainText('Need inspiration? Try:');

        // Close the welcome modal
        await page.locator('[data-testid="welcome-modal-close-button"]').click();

        // Verify that the welcome modal is closed
        await expect(page.locator('[data-testid="welcome-modal"]')).not.toBeVisible();
    });

    test('Video dialog - video dialog can be opened from About menu and closed', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the About button to be visible
        await page.waitForSelector('[data-testid="about-button"]', {state: 'visible'});

        // Click the About button
        await page.locator('[data-testid="about-button"]').click();

        // Wait for the About menu to appear
        await page.waitForSelector('#basic-menu', {state: 'visible'});

        // Click the "Watch intro video" menu item
        await page.locator('#basic-menu li:has-text("Watch intro video")').click();

        // Wait for the video dialog to appear
        await page.waitForSelector('div[role="dialog"]', {state: 'visible'});

        // Verify that the video dialog contains the expected title
        const modalTitle = page.locator('div[role="dialog"] #video-dialog-title');
        await expect(modalTitle).toContainText('Welcome to Zork AI');

        // Verify that the video dialog contains a video element
        const videoElement = page.locator('div[role="dialog"] video');
        await expect(videoElement).toBeVisible();

        // Close the video dialog
        await page.locator('div[role="dialog"] button:has-text("Close")').click();

        // Verify that the video dialog is closed
        await expect(page.locator('div[role="dialog"]')).not.toBeVisible();
    });

    test('Release notes dialog - release notes dialog can be opened from About menu and closed', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for the About button to be visible
        await page.waitForSelector('[data-testid="about-button"]', {state: 'visible'});

        // Click the About button
        await page.locator('[data-testid="about-button"]').click();

        // Wait for the About menu to appear
        await page.waitForSelector('#basic-menu', {state: 'visible'});

        // Click the "Version" menu item (last item in the menu)
        await page.locator('#basic-menu li').last().click();

        // Wait for the release notes dialog to appear
        await page.waitForSelector('div[role="dialog"]', {state: 'visible'});

        // Verify that the release notes dialog contains the expected title
        const modalTitle = page.locator('div[role="dialog"] #release-notes-title');
        await expect(modalTitle).toContainText('Zork AI Release Notes');

        // Verify that the release notes dialog contains either loading text or release notes
        const modalContent = page.locator('div[role="dialog"] .MuiDialogContent-root');
        await expect(modalContent).toBeVisible();

        // Close the release notes dialog
        await page.locator('div[role="dialog"] button:has-text("Close")').click();

        // Verify that the release notes dialog is closed
        await expect(page.locator('div[role="dialog"]')).not.toBeVisible();
    });

});