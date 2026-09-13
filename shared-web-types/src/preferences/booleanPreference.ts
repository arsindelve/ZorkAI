import {PREFERENCE_KEY_PREFIX, readStoredPreference, writeStoredPreference} from './storage';

export interface BooleanPreference {
    key: string;
    defaultValue: boolean;
    load(): boolean;
    save(value: boolean): void;
}

/**
 * A single on/off preference backed by localStorage.
 *
 * Only the exact strings 'true' and 'false' are honoured; anything else - absent,
 * blank, or hand-edited junk - falls back to `defaultValue`. Reading it as
 * `stored !== 'false'` would be subtly wrong for any preference that defaults to off,
 * so the comparison is explicit in both directions.
 */
export function createBooleanPreference(name: string, defaultValue: boolean): BooleanPreference {
    const key = `${PREFERENCE_KEY_PREFIX}${name}`;
    return {
        key,
        defaultValue,
        load(): boolean {
            const stored = readStoredPreference(key);
            if (stored === 'true') return true;
            if (stored === 'false') return false;
            return defaultValue;
        },
        save(value: boolean): void {
            writeStoredPreference(key, value ? 'true' : 'false');
        },
    };
}
