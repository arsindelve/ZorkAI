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
import {closeWelcomeModal, handlePlanetfallRoute} from './testHelpers';

const HINT_URL = 'http://localhost:5000/Planetfall/hint';

async function fulfillHint(route: Route, text: string) {
    await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({text})
    });
}

test.describe('Hint Panel', () => {

    test.beforeEach(async ({page}) => {
        await page.route('http://localhost:5000/Planetfall', handlePlanetfallRoute);
    });

    test('Hints button opens and closes the panel', async ({page}) => {
        await closeWelcomeModal(page);

        await page.waitForSelector('[data-testid="hints-button"]', {state: 'visible', timeout: 10000});
        await page.click('[data-testid="hints-button"]');

        await expect(page.getByTestId('hint-panel')).toBeVisible();
        await expect(page.getByText('Costs no turn')).toBeVisible();
        await expect(page.getByText(/I won't judge/)).toBeVisible();

        await page.click('[data-testid="hint-close"]');
        await expect(page.getByTestId('hint-panel')).not.toBeVisible();
    });

    test('asking a question shows the narrator answer and replays history on the follow-up', async ({page}) => {
        const requests: {question: string; history: {question: string; revealed: string}[]}[] = [];

        await page.route(HINT_URL, async route => {
            requests.push(route.request().postDataJSON());
            await fulfillHint(route, requests.length === 1
                ? 'Try waiting. The ship has plans for you.'
                : 'Fine: wait until the explosion, then head for the pod.');
        });

        await closeWelcomeModal(page);
        await page.click('[data-testid="hints-button"]');

        await page.fill('[data-testid="hint-input"]', 'what should I do?');
        await page.click('[data-testid="hint-send"]');
        await expect(page.getByText('Try waiting. The ship has plans for you.')).toBeVisible();

        await page.fill('[data-testid="hint-input"]', 'more help');
        await page.click('[data-testid="hint-send"]');
        await expect(page.getByText(/head for the pod/)).toBeVisible();

        // The endpoint is stateless: the client replays the running conversation with each ask.
        expect(requests[0].question).toBe('what should I do?');
        expect(requests[0].history).toEqual([]);
        expect(requests[1].question).toBe('more help');
        expect(requests[1].history).toEqual([
            {question: 'what should I do?', revealed: 'Try waiting. The ship has plans for you.'}
        ]);
    });

    test('a failed hint request shows an in-voice error and keeps the question', async ({page}) => {
        await page.route(HINT_URL, route => route.fulfill({status: 500, body: 'boom'}));

        await closeWelcomeModal(page);
        await page.click('[data-testid="hints-button"]');

        await page.fill('[data-testid="hint-input"]', 'help me');
        await page.click('[data-testid="hint-send"]');

        await expect(page.getByTestId('hint-error')).toBeVisible();
        // The question is restored so the player can simply resend.
        await expect(page.getByTestId('hint-input')).toHaveValue('help me');
    });

    test('quick-ask chips send a question directly', async ({page}) => {
        await page.route(HINT_URL, route => fulfillHint(route, 'A chip-sized nudge.'));

        await closeWelcomeModal(page);
        await page.click('[data-testid="hints-button"]');

        await page.locator('[data-testid="hint-chip"]').first().click();
        await expect(page.getByText('A chip-sized nudge.')).toBeVisible();
    });
});
