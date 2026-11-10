/**
 * Zork Preferences Tests
 *
 * These tests cover the half of the preferences feature that unit tests structurally
 * cannot reach: that a choice made in the dialog actually changes the running game,
 * and that it survives a reload through localStorage.
 *
 * The jest suites assert that picking an option calls its setter. Nothing below the
 * context - Game.tsx applying fontSize/fontFamily/lineHeight, the {showVerbsMenu && ...}
 * guards, the compass transform - is visible to them.
 *
 * NOTE: API Mocking
 * As with the other specs, API responses are intercepted so these tests do not need a
 * running backend.
 */

import {test, expect, Page} from '@playwright/test';
import {closeWelcomeModal, handleZorkOneRoute} from './testHelpers';

/** The transcript panel, whose computed style is what every reading preference drives. */
const TRANSCRIPT = '[data-testid="game-responses-container"]';

/**
 * Options are real radio/checkbox inputs hidden behind styled labels, so a click has to
 * land on the label - which is also what a player actually clicks.
 */
async function choose(page: Page, inputId: string) {
    await page.locator(`label[for="${inputId}"]`).click();
}

async function openPreferences(page: Page) {
    await page.locator('[data-testid="game-button"]').click();
    await page.locator('#game-menu li:has-text("Preferences")').click();
    await expect(page.locator('[data-testid="preferences-dialog"]')).toBeVisible();
}

const transcriptStyle = (page: Page, property: string) =>
    page
        .locator(TRANSCRIPT)
        .evaluate((element, prop) => getComputedStyle(element).getPropertyValue(prop), property);

test.describe('Preferences', () => {
    test.beforeEach(async ({page}) => {
        await page.route('http://localhost:5000/ZorkOne', handleZorkOneRoute);
    });

    test('the Game menu opens the Preferences dialog', async ({page}) => {
        await closeWelcomeModal(page);
        await openPreferences(page);

        await expect(page.locator('[data-testid="font-size-medium"]')).toBeChecked();
    });

    test('choosing a font size resizes the transcript', async ({page}) => {
        await closeWelcomeModal(page);
        const before = await transcriptStyle(page, 'font-size');

        await openPreferences(page);
        await choose(page, 'transcript-font-size-xlarge');

        const after = await transcriptStyle(page, 'font-size');
        expect(parseFloat(after)).toBeGreaterThan(parseFloat(before));
    });

    test('choosing a font changes the transcript typeface', async ({page}) => {
        await closeWelcomeModal(page);
        await openPreferences(page);

        await choose(page, 'transcript-font-storybook');

        expect(await transcriptStyle(page, 'font-family')).toContain('Georgia');
    });

    test('choosing line spacing changes the transcript leading', async ({page}) => {
        await closeWelcomeModal(page);
        const before = await transcriptStyle(page, 'line-height');

        await openPreferences(page);
        await choose(page, 'line-spacing-roomy');

        const after = await transcriptStyle(page, 'line-height');
        expect(parseFloat(after)).toBeGreaterThan(parseFloat(before));
    });

    test('a choice survives a reload', async ({page}) => {
        // The reason the preference is stored at all - and the one thing a component
        // test cannot prove, since it never leaves the render.
        await closeWelcomeModal(page);
        await openPreferences(page);
        await choose(page, 'transcript-font-typewriter');
        await page.locator('[data-testid="preferences-done"]').click();

        await page.reload();
        await expect(page.locator(TRANSCRIPT)).toBeVisible();

        expect(await transcriptStyle(page, 'font-family')).toContain('Courier');
    });

    test('turning off the Verbs menu removes the button from the page', async ({page}) => {
        await closeWelcomeModal(page);
        await expect(page.locator('button:has-text("Verbs")')).toBeVisible();

        await openPreferences(page);
        await choose(page, 'pref-verbs');
        await page.locator('[data-testid="preferences-done"]').click();

        await expect(page.locator('button:has-text("Verbs")')).toHaveCount(0);
    });

    test('turning off the compass hides it and disables its size control', async ({page}) => {
        test.skip(
            (page.viewportSize()?.width ?? 0) < 768,
            'Compass is only visible on desktop viewports',
        );

        await closeWelcomeModal(page);
        await openPreferences(page);

        await choose(page, 'pref-compass');

        // Sizing a hidden compass does nothing, so the control must not look live.
        await expect(page.locator('[data-testid="compass-size-large"]')).toBeDisabled();

        await page.locator('[data-testid="preferences-done"]').click();
        await expect(page.locator('svg#Layer_1')).toHaveCount(0);
    });

    test('turning animations off flags the document for reduced motion', async ({page}) => {
        await closeWelcomeModal(page);
        await openPreferences(page);

        await choose(page, 'pref-animations');

        // Set on <html> rather than an app wrapper so it also covers portalled dialogs.
        await expect(page.locator('html')).toHaveAttribute('data-reduce-motion', 'true');
    });
});
