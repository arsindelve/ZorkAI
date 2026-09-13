/**
 * Transcript font family preference.
 *
 * Every option is a stack of fonts already present on the system - nothing is
 * downloaded, so switching is instant, works offline and cannot flash unstyled text.
 *
 * Stored per-browser in localStorage - see ./storage. No backend.
 */

import {PREFERENCE_KEY_PREFIX, readStoredPreference, writeStoredPreference} from './storage';

export type TranscriptFont = 'terminal' | 'typewriter' | 'storybook' | 'modern' | 'legible';

/** Menu order. `terminal` is what the game has always used and stays the default. */
export const TRANSCRIPT_FONTS: readonly TranscriptFont[] = [
    'terminal',
    'typewriter',
    'storybook',
    'modern',
    'legible',
];

export const TRANSCRIPT_FONT_LABELS: Record<TranscriptFont, string> = {
    terminal: 'Terminal',
    typewriter: 'Typewriter',
    storybook: 'Storybook',
    modern: 'Modern',
    legible: 'Easy to read',
};

export const TRANSCRIPT_FONT_DESCRIPTIONS: Record<TranscriptFont, string> = {
    terminal: 'Roboto Mono - the classic screen look',
    typewriter: 'Courier - like the original 1980s printouts',
    storybook: 'Georgia - a serif, for reading it as prose',
    modern: 'A clean interface sans-serif',
    legible: 'Wide, open letterforms for easier reading',
};

/**
 * `terminal` leads with Roboto Mono exactly as the transcript always has. The rest of
 * its stack is real monospace rather than the bare `serif` the utility class fell back
 * to - a monospace transcript dropping to a serif was never intentional.
 */
export const TRANSCRIPT_FONT_STACKS: Record<TranscriptFont, string> = {
    terminal: "'Roboto Mono', ui-monospace, SFMono-Regular, Menlo, Consolas, monospace",
    typewriter: "'Courier New', Courier, 'Nimbus Mono PS', monospace",
    storybook: "Georgia, 'Iowan Old Style', 'Times New Roman', Times, serif",
    modern: "system-ui, -apple-system, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif",
    legible: "Verdana, Tahoma, 'DejaVu Sans', Geneva, sans-serif",
};

/**
 * The three proportional options trade column alignment for readability. Anything the
 * games line up in columns (inventory lists, score tables) only stays aligned in the
 * two monospace options, which is why `terminal` remains the default.
 */
export const TRANSCRIPT_FONT_IS_MONOSPACE: Record<TranscriptFont, boolean> = {
    terminal: true,
    typewriter: true,
    storybook: false,
    modern: false,
    legible: false,
};

export const DEFAULT_TRANSCRIPT_FONT: TranscriptFont = 'terminal';

export const TRANSCRIPT_FONT_STORAGE_KEY = `${PREFERENCE_KEY_PREFIX}transcriptFont`;

export function isTranscriptFont(value: unknown): value is TranscriptFont {
    return typeof value === 'string' && (TRANSCRIPT_FONTS as readonly string[]).includes(value);
}

export function transcriptFontStack(font: TranscriptFont): string {
    return TRANSCRIPT_FONT_STACKS[font];
}

export function loadTranscriptFont(): TranscriptFont {
    const stored = readStoredPreference(TRANSCRIPT_FONT_STORAGE_KEY);
    return isTranscriptFont(stored) ? stored : DEFAULT_TRANSCRIPT_FONT;
}

export function saveTranscriptFont(font: TranscriptFont): void {
    writeStoredPreference(TRANSCRIPT_FONT_STORAGE_KEY, font);
}
