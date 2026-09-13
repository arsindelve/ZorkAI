/**
 * Zork's base transcript font size, in pixels.
 *
 * This is the "Medium" rung of the transcript font size preference - every other rung
 * is a multiple of it (see TRANSCRIPT_FONT_SCALE in the shared package). Changing this
 * value reshapes the whole scale, so it is the one place to adjust the game's default
 * text size. Planetfall keeps its own base.
 */
export const TRANSCRIPT_BASE_FONT_SIZE_PX = 15;
