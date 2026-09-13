/**
 * The on/off preferences, in the order they appear in the Preferences dialog.
 *
 * Each one hides a piece of optional UI. They all default to ON: the preference exists
 * to let a player clear the screen, never to make them discover a feature that was
 * hidden from them on first run.
 */

import {createBooleanPreference} from './booleanPreference';

export const showCompassPreference = createBooleanPreference('showCompass', true);
export const showVerbsMenuPreference = createBooleanPreference('showVerbsMenu', true);
export const showCommandsMenuPreference = createBooleanPreference('showCommandsMenu', true);
export const showLocationButtonPreference = createBooleanPreference('showLocationButton', true);
export const showInventoryButtonPreference = createBooleanPreference('showInventoryButton', true);

/**
 * Animations: the compass pulse, the direction "ping" flashes and the transcript
 * fade-in. Defaults to on, exactly as the games have always behaved. Turning it off
 * suppresses motion everywhere via a root-level attribute (see GameProvider).
 */
export const animationsPreference = createBooleanPreference('animations', true);
