/**
 * Room artwork
 *
 * The jest suite drives the component with a stand-in for the DOM `Image`, so it proves
 * the state machine but never that a real <img> reaches the page over the transcript.
 * That is what these cover, with the S3 file itself mocked - the suite must not depend
 * on a bucket object existing.
 */

import {test, expect, Page} from '@playwright/test';
import {handleZorkOneRoute} from './testHelpers';

const ART = 'https://zorkai-assets.s3.amazonaws.com/locations/*';
const PLATE = '[data-testid="location-image"]';

/** A 1x1 transparent GIF - the smallest thing the browser will accept as an image. */
const PIXEL = Buffer.from('R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7', 'base64');

async function serveArtwork(page: Page) {
    await page.route(ART, (route) =>
        route.fulfill({status: 200, contentType: 'image/gif', body: PIXEL}),
    );
}

async function start(page: Page) {
    await page.goto('/');
    await page.waitForSelector('[data-testid="welcome-modal"]', {state: 'visible'});
    await page.locator('[data-testid="welcome-modal-close-button"]').click();
}

test.describe('Room artwork', () => {
    test.beforeEach(async ({page}) => {
        // A glob, not the bare URL the other specs use: the opening GET carries
        // ?sessionId=...&clientId=..., which an exact string does not match, and these
        // tests turn on the room name that opening response reports.
        await page.route('**/ZorkOne?*', handleZorkOneRoute);
        await page.route('http://localhost:5000/ZorkOne', handleZorkOneRoute);
    });

    test('the room art covers the transcript on arrival, then dissolves away', async ({page}) => {
        await serveArtwork(page);
        await start(page);

        // West of House is the mocked starting room, and it has art.
        await expect(page.locator(PLATE)).toBeVisible();
        await expect(page.locator(`${PLATE} img`)).toHaveAttribute(
            'src',
            'https://zorkai-assets.s3.amazonaws.com/locations/WestOfHouse.webp',
        );
        await expect(page.locator(`${PLATE} img`)).toHaveAttribute('alt', 'West of House');

        // It leaves on its own; the transcript underneath was never disturbed.
        await expect(page.locator(PLATE)).toHaveCount(0, {timeout: 15000});
        await expect(page.locator('[data-testid="game-responses-container"]')).toContainText(
            'small mailbox',
        );
    });

    test('a click dismisses it early', async ({page}) => {
        await serveArtwork(page);
        await start(page);

        await page.locator(PLATE).click();
        await expect(page.locator(PLATE)).toHaveCount(0, {timeout: 5000});
    });

    test('a dark room shows nothing until the player strikes a light', async ({page}) => {
        // The server withholds the room's exits and action chips in the dark (issue #238);
        // the artwork is the same kind of leak, so it waits for the lamp.
        const cellar = (dark: boolean, response: string) => ({
            score: 0,
            moves: 1,
            locationName: 'Kitchen',
            locationKey: 'Kitchen',
            response,
            inventory: ['lamp'],
            exits: [],
            itIsDarkHere: dark,
        });

        await serveArtwork(page);
        await page.unroute('**/ZorkOne?*');
        await page.route('**/ZorkOne?*', (route) =>
            route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify(
                    cellar(true, 'It is pitch black. You are likely to be eaten by a grue.'),
                ),
            }),
        );
        await page.unroute('http://localhost:5000/ZorkOne');
        await page.route('http://localhost:5000/ZorkOne', (route) =>
            route.fulfill({
                status: 200,
                contentType: 'application/json',
                body: JSON.stringify(cellar(false, 'The brass lantern is now on.')),
            }),
        );

        await start(page);
        await expect(page.locator('[data-testid="game-responses-container"]')).toContainText(
            'pitch black',
        );
        await expect(page.locator(PLATE)).toHaveCount(0);

        // Strike the light: same room, now lit, and the picture is waiting.
        await page
            .locator('[data-testid="command-input"] input, input')
            .first()
            .fill('turn on lamp');
        await page.keyboard.press('Enter');

        await expect(page.locator(PLATE)).toBeVisible();
    });

    test('a room whose art is missing shows nothing at all', async ({page}) => {
        await page.route(ART, (route) => route.abort());
        await start(page);

        await expect(page.locator('[data-testid="game-responses-container"]')).toContainText(
            'small mailbox',
        );
        await expect(page.locator(PLATE)).toHaveCount(0);
    });

    test('turning Artwork off in Preferences stops the art appearing', async ({page}) => {
        await serveArtwork(page);
        await start(page);

        await page.locator(PLATE).click();
        await expect(page.locator(PLATE)).toHaveCount(0, {timeout: 5000});

        await page.locator('[data-testid="game-button"]').click();
        await page.locator('#game-menu li:has-text("Preferences")').click();
        await page.locator('label[for="pref-location-images"]').click();
        await page.locator('[data-testid="preferences-done"]').click();

        // A reload is a fresh arrival in the starting room, so the art would show again
        // were the preference not honoured.
        await page.reload();
        await expect(page.locator('[data-testid="game-responses-container"]')).toContainText(
            'small mailbox',
        );
        await expect(page.locator(PLATE)).toHaveCount(0);
    });
});
