/**
 * Compass size preference. Whether the compass is shown at all is a plain on/off
 * toggle - see showCompassPreference in ./toggles.
 *
 * Stored per-browser in localStorage - see ./storage. No backend.
 */

import {PREFERENCE_KEY_PREFIX, readStoredPreference, writeStoredPreference} from './storage';

/* ----------------------------------------------------------------------- size */

export type CompassSize = 'xsmall' | 'small' | 'medium' | 'large';

/** Menu order, smallest first. */
export const COMPASS_SIZES: readonly CompassSize[] = ['xsmall', 'small', 'medium', 'large'];

export const COMPASS_SIZE_LABELS: Record<CompassSize, string> = {
    xsmall: 'Smallest',
    small: 'Smaller',
    medium: 'Default',
    large: 'Larger',
};

/**
 * Applied as a CSS transform scale on the compass container, which resizes the rose,
 * its up/down controls and its padding together. The compass sizes itself internally
 * with utility classes, so scaling the container is the only way to move all of it in
 * step without duplicating that scale in every client.
 */
export const COMPASS_SIZE_SCALE: Record<CompassSize, number> = {
    xsmall: 0.7,
    small: 0.85,
    medium: 1,
    large: 1.18,
};

export const DEFAULT_COMPASS_SIZE: CompassSize = 'medium';

export const COMPASS_SIZE_STORAGE_KEY = `${PREFERENCE_KEY_PREFIX}compassSize`;

export function isCompassSize(value: unknown): value is CompassSize {
    return typeof value === 'string' && (COMPASS_SIZES as readonly string[]).includes(value);
}

export function loadCompassSize(): CompassSize {
    const stored = readStoredPreference(COMPASS_SIZE_STORAGE_KEY);
    return isCompassSize(stored) ? stored : DEFAULT_COMPASS_SIZE;
}

export function saveCompassSize(size: CompassSize): void {
    writeStoredPreference(COMPASS_SIZE_STORAGE_KEY, size);
}

export function compassScale(size: CompassSize): number {
    return COMPASS_SIZE_SCALE[size];
}
