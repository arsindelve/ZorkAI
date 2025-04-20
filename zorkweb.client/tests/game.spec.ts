/**
 * Zork Game Tests - Main Entry Point
 * 
 * This file serves as the main entry point for the Zork game tests.
 * The tests have been organized into separate files by category for better maintainability.
 * 
 * Test Categories:
 * - interface.spec.ts: Tests for basic game interface functionality
 * - menus.spec.ts: Tests for menu functionality (About, Game, Verbs, Commands, Inventory)
 * - commands.spec.ts: Tests for game input and command functionality
 * - gameState.spec.ts: Tests for game state display (location, score, moves)
 * - saveRestore.spec.ts: Tests for save, restore, and restart functionality
 * - dialogs.spec.ts: Tests for dialog functionality (Welcome, Video, Release notes)
 * - features.spec.ts: Tests for miscellaneous features (copy game transcript)
 * 
 * NOTE: API Mocking
 * All test files use Playwright's route interception to mock API responses.
 * This allows the tests to run reliably without requiring the actual backend.
 */

import {test} from '@playwright/test';

test.describe('Zork Game', () => {
    // This is just a placeholder to indicate that the tests have been moved to separate files
    test('Tests have been moved to separate files by category', async () => {
        // This test does nothing, it's just a placeholder
        // The actual tests are in the files listed above
    });
});