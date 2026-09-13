/**
 * Transcript line spacing preference.
 *
 * Stored per-browser in localStorage - see ./storage. No backend.
 */

import {PREFERENCE_KEY_PREFIX, readStoredPreference, writeStoredPreference} from './storage';

export type TranscriptLineSpacing = 'compact' | 'normal' | 'roomy';

/** Menu order, tightest first. */
export const TRANSCRIPT_LINE_SPACINGS: readonly TranscriptLineSpacing[] = [
    'compact',
    'normal',
    'roomy',
];

export const TRANSCRIPT_LINE_SPACING_LABELS: Record<TranscriptLineSpacing, string> = {
    compact: 'Compact',
    normal: 'Normal',
    roomy: 'Roomy',
};

/**
 * Unitless multipliers of the font size, so spacing tracks the transcript font size
 * preference instead of fighting it. `normal` is 1.5 - exactly what the transcript has
 * always inherited - so the default changes nothing.
 */
export const TRANSCRIPT_LINE_SPACING_VALUES: Record<TranscriptLineSpacing, number> = {
    compact: 1.3,
    normal: 1.5,
    roomy: 1.8,
};

export const DEFAULT_TRANSCRIPT_LINE_SPACING: TranscriptLineSpacing = 'normal';

export const TRANSCRIPT_LINE_SPACING_STORAGE_KEY = `${PREFERENCE_KEY_PREFIX}transcriptLineSpacing`;

export function isTranscriptLineSpacing(value: unknown): value is TranscriptLineSpacing {
    return (
        typeof value === 'string' &&
        (TRANSCRIPT_LINE_SPACINGS as readonly string[]).includes(value)
    );
}

export function transcriptLineHeight(spacing: TranscriptLineSpacing): number {
    return TRANSCRIPT_LINE_SPACING_VALUES[spacing];
}

export function loadTranscriptLineSpacing(): TranscriptLineSpacing {
    const stored = readStoredPreference(TRANSCRIPT_LINE_SPACING_STORAGE_KEY);
    return isTranscriptLineSpacing(stored) ? stored : DEFAULT_TRANSCRIPT_LINE_SPACING;
}

export function saveTranscriptLineSpacing(spacing: TranscriptLineSpacing): void {
    writeStoredPreference(TRANSCRIPT_LINE_SPACING_STORAGE_KEY, spacing);
}
