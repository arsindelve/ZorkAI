/**
 * localStorage access for user preferences. There is deliberately no backend: a
 * preference describes the device you are reading on, not the saved game.
 *
 * Both helpers swallow failures because localStorage is not merely "sometimes empty" -
 * it *throws* on access in a browser configured to block site data, and a throw here
 * would take the whole game down over a cosmetic setting.
 */

export const PREFERENCE_KEY_PREFIX = 'preferences.';

export function readStoredPreference(key: string): string | null {
    try {
        return localStorage.getItem(key);
    } catch {
        return null;
    }
}

export function writeStoredPreference(key: string, value: string): void {
    try {
        localStorage.setItem(key, value);
    } catch {
        // Storage blocked or full - the choice still applies for this session.
    }
}
