import {useState} from 'react';
import {BooleanPreference} from './booleanPreference';

/**
 * State for one on/off preference, seeded from localStorage and written back on every
 * change.
 *
 * The stored value is read through useState's lazy initialiser rather than an effect,
 * so the very first paint is already correct - loading it in an effect would render
 * the default for a frame and visibly flash hidden UI on before hiding it again.
 */
export function useBooleanPreference(
    preference: BooleanPreference,
): [boolean, (value: boolean) => void] {
    const [value, setValue] = useState<boolean>(preference.load);

    const set = (next: boolean) => {
        setValue(next);
        preference.save(next);
    };

    return [value, set];
}
