/**
 * Transcript font size preference.
 *
 * Stored per-browser in localStorage - there is deliberately no backend. A display
 * preference belongs to the device you are reading on, not to the saved game, so it
 * must never travel with a session or need a round trip to render.
 */

import {PREFERENCE_KEY_PREFIX, readStoredPreference, writeStoredPreference} from './storage';

export type TranscriptFontSize = 'small' | 'medium' | 'large' | 'xlarge';

/** Menu order, smallest first. */
export const TRANSCRIPT_FONT_SIZES: readonly TranscriptFontSize[] = [
    'small',
    'medium',
    'large',
    'xlarge',
];

export const TRANSCRIPT_FONT_SIZE_LABELS: Record<TranscriptFontSize, string> = {
    small: 'Small',
    medium: 'Medium',
    large: 'Large',
    xlarge: 'Extra Large',
};

/**
 * Multipliers applied to each game's *own* base transcript size rather than absolute
 * pixel values. That keeps "medium" identical to whatever the game already shipped
 * with, so turning the preference on changes nothing until the player asks it to -
 * and Zork (15px) and Planetfall (16px) can keep their different baselines.
 */
export const TRANSCRIPT_FONT_SCALE: Record<TranscriptFontSize, number> = {
    small: 0.87,
    medium: 1,
    large: 1.15,
    xlarge: 1.3,
};

export const DEFAULT_TRANSCRIPT_FONT_SIZE: TranscriptFontSize = 'medium';

export const TRANSCRIPT_FONT_SIZE_STORAGE_KEY = `${PREFERENCE_KEY_PREFIX}transcriptFontSize`;

export function isTranscriptFontSize(value: unknown): value is TranscriptFontSize {
    return (
        typeof value === 'string' &&
        (TRANSCRIPT_FONT_SIZES as readonly string[]).includes(value)
    );
}

/**
 * Resolve the size to render at. Rounded so the browser never lands on a half pixel,
 * which renders blurry in a monospace transcript.
 */
export function transcriptFontSizePx(basePx: number, size: TranscriptFontSize): number {
    return Math.round(basePx * TRANSCRIPT_FONT_SCALE[size]);
}

/**
 * Read the stored preference, falling back to the default. A stale or hand-edited
 * value must not be trusted to still be one of the four sizes, hence the guard.
 */
export function loadTranscriptFontSize(): TranscriptFontSize {
    const stored = readStoredPreference(TRANSCRIPT_FONT_SIZE_STORAGE_KEY);
    return isTranscriptFontSize(stored) ? stored : DEFAULT_TRANSCRIPT_FONT_SIZE;
}

export function saveTranscriptFontSize(size: TranscriptFontSize): void {
    writeStoredPreference(TRANSCRIPT_FONT_SIZE_STORAGE_KEY, size);
}
