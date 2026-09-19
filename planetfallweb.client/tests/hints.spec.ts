/**
 * Hint panel tests
 *
 * Verifies the hint side panel: toggling it from the button row, asking the narrator a question,
 * the stateless client-owned history (replayed with each request), and error handling.
 *
 * NOTE: API Mocking
 * Like the other specs, these tests mock the API with Playwright route interception — including
 * the read-only /hint endpoint — so no backend (or OpenAI call) is needed.
 */

import {test, expect, Route} from '@playwright/test';
import {closeWelcomeModal, handlePlanetfallRoute, visitGame} from './testHelpers';

const HINT_URL = 'http://localhost:5000/Planetfall/hint';

async function fulfillHint(route: Route, text: string) {
    await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({text}),
    });
}

test.describe('Hint Panel', () => {
    test.beforeEach(async ({page}) => {
        await page.route('http://localhost:5000/Planetfall', handlePlanetfallRoute);
    });

    test('Hints button opens and closes the panel', async ({page}) => {
        await closeWelcomeModal(page, '/?hints=1');

        const hintsButton = page.getByRole('button', {name: 'Hints'});
        await expect(hintsButton).toBeEnabled();
        await hintsButton.click();

        await expect(page.getByTestId('hint-panel')).toBeVisible();
        await expect(page.getByRole('button', {name: 'New chat'})).toBeVisible();
        // The compass floats where the panel docks — it must yield while hints are open.
        await expect(page.locator('.compass-ring')).toHaveCount(0);

        await page.getByRole('button', {name: 'Close hints'}).click();
        await expect(page.getByTestId('hint-panel')).not.toBeVisible();
        // ...and return when the panel closes.
        await expect(page.locator('.compass-ring')).toBeVisible();
    });

    test('asking a question shows the narrator answer and replays history on the follow-up', async ({
        page,
    }) => {
        const requests: {question: string; history: {question: string; revealed: string}[]}[] = [];

        await page.route(HINT_URL, async (route) => {
            requests.push(route.request().postDataJSON());
            await fulfillHint(
                route,
                requests.length === 1
                    ? 'Try waiting. The ship has plans for you.'
                    : 'Fine: wait until the explosion, then head for the pod.',
            );
        });

        await closeWelcomeModal(page, '/?hints=1');
        await page.getByRole('button', {name: 'Hints'}).click();

        await page.getByPlaceholder('Ask for a hint…').fill('what should I do?');
        await page.getByRole('button', {name: 'Send hint question'}).click();
        await expect(page.getByText('Try waiting. The ship has plans for you.')).toBeVisible();

        await page.getByPlaceholder('Ask for a hint…').fill('more help');
        await page.getByRole('button', {name: 'Send hint question'}).click();
        await expect(page.getByText(/head for the pod/)).toBeVisible();

        // The endpoint is stateless: the client replays the running conversation with each ask.
        expect(requests[0].question).toBe('what should I do?');
        expect(requests[0].history).toEqual([]);
        expect(requests[1].question).toBe('more help');
        expect(requests[1].history).toEqual([
            {question: 'what should I do?', revealed: 'Try waiting. The ship has plans for you.'},
        ]);
    });

    test('a failed hint request shows an in-voice error and keeps the question', async ({page}) => {
        await page.route(HINT_URL, (route) => route.fulfill({status: 500, body: 'boom'}));

        await closeWelcomeModal(page, '/?hints=1');
        await page.getByRole('button', {name: 'Hints'}).click();

        await page.getByPlaceholder('Ask for a hint…').fill('help me');
        await page.getByRole('button', {name: 'Send hint question'}).click();

        await expect(page.getByTestId('hint-error')).toBeVisible();
        // The question is restored so the player can simply resend.
        await expect(page.getByTestId('hint-input')).toHaveValue('help me');
    });

    // Both clients ship with hints disabled. ?hints=0 / ?hints=1 is the per-browser override,
    // persisted in localStorage, used to test the real pipeline before launch.
    test('feature flag: ?hints=0 hides hints, persists, and ?hints=1 restores them', async ({
        page,
    }) => {
        await closeWelcomeModal(page, '/?hints=0');
        await expect(page.getByRole('button', {name: 'Hints'})).toHaveCount(0);
        await expect(page.getByTestId('hint-panel')).toHaveCount(0);
        // The rest of the game UI is unaffected.
        await expect(page.getByTestId('game-input')).toBeVisible();

        // The override persists across a plain reload (no query param). Use visitGame: the welcome
        // modal already fired on this context's first visit and will not appear again.
        await visitGame(page);
        await expect(page.getByRole('button', {name: 'Hints'})).toHaveCount(0);

        // ?hints=1 flips it back on for this browser.
        await visitGame(page, '/?hints=1');
        await page.getByRole('button', {name: 'Hints'}).click();
        await expect(page.getByTestId('hint-panel')).toBeVisible();
    });
});
