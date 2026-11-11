/**
 * Optional marker printed beside each command you type, to make a long transcript
 * easier to scan back through.
 *
 * Stored per-browser in localStorage - see ./storage. No backend.
 */

import {PREFERENCE_KEY_PREFIX, readStoredPreference, writeStoredPreference} from './storage';

export type TranscriptMarker = 'none' | 'time' | 'number';

/** Menu order. `none` is how the transcript has always looked and stays the default. */
export const TRANSCRIPT_MARKERS: readonly TranscriptMarker[] = ['none', 'time', 'number'];

export const TRANSCRIPT_MARKER_LABELS: Record<TranscriptMarker, string> = {
    none: 'None',
    time: 'Time of day',
    number: 'Command number',
};

export const TRANSCRIPT_MARKER_DESCRIPTIONS: Record<TranscriptMarker, string> = {
    none: 'Just the command, as now',
    time: 'When you typed it, e.g. 3:42 PM',
    number: 'How many commands in, e.g. #42',
};

export const DEFAULT_TRANSCRIPT_MARKER: TranscriptMarker = 'none';

export const TRANSCRIPT_MARKER_STORAGE_KEY = `${PREFERENCE_KEY_PREFIX}transcriptMarker`;

export function isTranscriptMarker(value: unknown): value is TranscriptMarker {
    return typeof value === 'string' && (TRANSCRIPT_MARKERS as readonly string[]).includes(value);
}

/**
 * Render the marker for one command.
 *
 * `commandNumber` counts the commands the player has typed this session. It is
 * deliberately NOT the game's move counter: `look` and `inventory` are free actions
 * that never advance moves, so numbering by moves would repeat the same label on
 * consecutive lines and be useless for scanning.
 *
 * `now` is injected so the caller - and the tests - control the clock.
 */
export function formatTranscriptMarker(
    marker: TranscriptMarker,
    commandNumber: number,
    now: Date = new Date(),
): string {
    switch (marker) {
        case 'time':
            return now.toLocaleTimeString([], {hour: 'numeric', minute: '2-digit'});
        case 'number':
            return `#${commandNumber}`;
        case 'none':
        default:
            return '';
    }
}

export function loadTranscriptMarker(): TranscriptMarker {
    const stored = readStoredPreference(TRANSCRIPT_MARKER_STORAGE_KEY);
    return isTranscriptMarker(stored) ? stored : DEFAULT_TRANSCRIPT_MARKER;
}

export function saveTranscriptMarker(marker: TranscriptMarker): void {
    writeStoredPreference(TRANSCRIPT_MARKER_STORAGE_KEY, marker);
}
