/**
 * Test Helper Functions
 * 
 * This file contains helper functions used across multiple test files.
 */

import { Page, Route } from '@playwright/test';
import { mockResponses } from './mockResponses';

/**
 * Helper function to close the welcome modal
 * This function navigates to the application, waits for the welcome modal to be visible,
 * and then closes it by clicking the close button.
 */
export async function closeWelcomeModal(page: Page) {
    // Navigate to the application
    await page.goto('/');

    // Wait for the welcome modal to be visible
    await page.waitForSelector('[data-testid="welcome-modal"]', {state: 'visible'});

    // Close the welcome modal
    await page.locator('[data-testid="welcome-modal-close-button"]').click();
}

/**
 * Helper function to wait for game responses
 * This function waits for more than one game response to appear,
 * indicating that a command has been processed.
 */
export async function waitForGameResponse(page: Page) {
    // Wait for any response to appear after submitting the command
    await page.waitForFunction(() => {
        const paragraphs = document.querySelectorAll('[data-testid="game-response"]');
        return paragraphs.length > 1; // More than just the initial loading text
    }, { timeout: 10000 });
}

/**
 * Helper function to handle the main ZorkOne endpoint route
 * This function intercepts requests to the ZorkOne endpoint and returns mock responses
 */
export async function handleZorkOneRoute(route: Route) {
    const method = route.request().method();

    if (method === 'GET') {
        // Return the mock init response
        await route.fulfill({ 
            status: 200, 
            contentType: 'application/json',
            body: JSON.stringify(mockResponses.init)
        });
    } else if (method === 'POST') {
        // Get the request body to determine which mock response to return
        const body = route.request().postDataJSON();
        let response: { score: number; moves: number; locationName: string; response: string; inventory: string[]; };

        // Select the appropriate mock response based on the input
        if (body.input === 'look around') {
            response = mockResponses.lookAround;
        } else if (body.input === 'look') {
            response = mockResponses.look;
        } else if (body.input === 'North') {
            response = mockResponses.north;
        } else if (body.input === 'restore') {
            response = mockResponses.restore;
        } else if (body.input === 'save') {
            response = mockResponses.save;
        } else if (body.input === 'restart') {
            response = mockResponses.restart;
        } else if (body.input === 'inventory') {
            response = mockResponses.withInventory;
        } else if (body.input === 'drop all') {
            response = mockResponses.emptyInventory;
        } else {
            // Default response for any other command
            response = {
                ...mockResponses.look,
                response: `You entered: ${body.input}`
            };
        }

        await route.fulfill({ 
            status: 200, 
            contentType: 'application/json',
            body: JSON.stringify(response)
        });
    }
}

/**
 * Helper function to handle the saveGame endpoint route
 * This function intercepts requests to the saveGame endpoint and returns a success message
 */
export async function handleSaveGameRoute(route: Route) {
    await route.fulfill({ 
        status: 200, 
        contentType: 'application/json',
        body: JSON.stringify("Game saved successfully")
    });
}

/**
 * Helper function to handle the restoreGame endpoint route
 * This function intercepts requests to the restoreGame endpoint and returns the initial game state
 */
export async function handleRestoreGameRoute(route: Route) {
    await route.fulfill({ 
        status: 200, 
        contentType: 'application/json',
        body: JSON.stringify(mockResponses.init)
    });
}

/**
 * Helper function to handle the getSavedGames endpoint route
 * This function intercepts requests to the getSavedGames endpoint and returns mock saved games
 */
export async function handleGetSavedGamesRoute(route: Route) {
    await route.fulfill({ 
        status: 200, 
        contentType: 'application/json',
        body: JSON.stringify(mockResponses.savedGames)
    });
}
