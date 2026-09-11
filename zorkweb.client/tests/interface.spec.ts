/**
 * Zork Game Interface Tests
 *
 * These tests verify the basic game interface functionality.
 *
 * NOTE: API Mocking
 * To avoid dependency on the backend API (which may not always be running),
 * these tests use Playwright's route interception to mock API responses.
 * This allows the tests to run reliably without requiring the actual backend.
 */

import {test, expect} from '@playwright/test';
import {
    closeWelcomeModal,
    handleZorkOneRoute,
    handleSaveGameRoute,
    handleRestoreGameRoute,
    handleGetSavedGamesRoute,
} from './testHelpers';

test.describe('Game Interface', () => {
    // Set up API mocking before each test
    test.beforeEach(async ({page}) => {
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

    test('should load the game interface', async ({page}) => {
        // Close the welcome modal using the helper function
        await closeWelcomeModal(page);

        // Wait for initial game content to load
        await page.waitForSelector('.bg-stone-900', {state: 'visible'});

        // Check that the game container is visible
        const gameContainer = page.locator('.bg-stone-900');
        await expect(gameContainer).toBeVisible();

        // Check that the input field is visible
        const inputField = page.locator('[data-testid="game-input"]');
        await expect(inputField).toBeVisible();

        // Check that the initial game text is displayed
        const gameText = page.locator('[data-testid="game-response"]');
        const textContent = await gameText.first().textContent();

        // Verify that some text is displayed
        expect(textContent).toBeTruthy();
        expect(textContent?.length).toBeGreaterThan(0);
    });

    // Regression guard for the "scrolling from the bottom" glitch. The decorative
    // scanline overlay used to be attached to the scrolling transcript itself. It is
    // animated with translateY, and a transformed descendant contributes its
    // *transformed* bounds to the scroll container's scrollable overflow — so
    // scrollHeight crept up by the panel's height every 8s and snapped back on each
    // loop. Auto-scroll then aimed at a phantom bottom (dead space under the newest
    // line), the view lurched, and the inflated gap falsely tripped the "New messages"
    // pill. Decorative overlays must live outside the scroll container.
    test('a displaced decorative overlay cannot extend the transcript scroll area', async ({
        page,
    }) => {
        // The session-restore GET carries a ?sessionId= query, which the exact-URL route
        // in beforeEach doesn't cover. This test asserts on the transcript's geometry, so
        // the opening text has to be mocked too rather than left to a live backend.
        await page.route('http://localhost:5000/ZorkOne?*', handleZorkOneRoute);

        // A normal desktop window height. At the default 1280x720 the panel is short
        // enough that the swept overlay stays tucked under the real content, which is
        // the one geometry where the defect is invisible.
        await page.setViewportSize({width: 1280, height: 900});
        await closeWelcomeModal(page);
        await page.waitForSelector('[data-testid="game-responses-container"]', {state: 'visible'});
        await page.waitForSelector('[data-testid="game-input"]', {state: 'visible'});

        // Wait for the initial response to land first — it clears the input box, and
        // would otherwise wipe the command out from under us.
        const responses = page.locator('[data-testid="game-response"]');
        await expect(responses).toHaveCount(2);

        await page.fill('[data-testid="game-input"]', 'look around');
        await expect(page.locator('[data-testid="game-input"]')).toHaveValue('look around');
        await page.click('[data-testid="go-button"]');
        await expect(responses).toHaveCount(3);

        const measure = () =>
            page.$eval('[data-testid="game-responses-container"]', (el) => ({
                scrollHeight: Math.round(el.scrollHeight),
                distanceFromBottom: Math.round(el.scrollHeight - el.scrollTop - el.clientHeight),
            }));

        const before = await measure();

        // Pin the scanline sweep at the end of its cycle instead of waiting out the 8s
        // loop. Freezing it is what makes this deterministic: a compositor-driven
        // transform animation doesn't necessarily republish scrollable overflow every
        // frame, so sampling the live animation can miss the displacement entirely.
        await page.addStyleTag({
            content:
                '.scanline-effect::after { animation: none !important; transform: translateY(100%) !important; }',
        });
        await page.waitForTimeout(200);
        const after = await measure();

        // Nothing was appended, so the scrollable area must not have grown...
        expect(after.scrollHeight).toBe(before.scrollHeight);

        // ...and the newest text must still be sitting at the true bottom of the panel.
        expect(after.distanceFromBottom).toBeLessThanOrEqual(2);

        // The sweep must not have been merely relocated onto an ancestor either. An
        // ancestor with phantom scrollable overflow is silently scrollable, and any
        // reveal-scroll (scrollIntoView, find-in-page) would slide the whole panel
        // inside its clip with no way for the player to put it back.
        const ancestorOverflow = await page.$eval(
            '[data-testid="game-responses-container"]',
            (el) => {
                let node = el.parentElement;
                let worst = 0;
                while (node && node !== document.body) {
                    worst = Math.max(worst, Math.round(node.scrollHeight - node.clientHeight));
                    node = node.parentElement;
                }
                return worst;
            },
        );
        expect(ancestorOverflow).toBe(0);
    });
});
