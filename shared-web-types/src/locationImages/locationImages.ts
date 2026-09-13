/**
 * Artwork for the handful of rooms that have a picture drawn for them.
 *
 * A registry rather than a URL derived from the room name on the fly: most rooms have
 * no art, and guessing a URL for them would fire a 404 on every first visit. Only the
 * rooms listed here are ever requested.
 */

/** Normalised room name -> absolute image URL. */
export interface LocationImageSet {
    readonly [normalisedLocationName: string]: string;
}

/**
 * The key a room name is looked up under.
 *
 * Lower-cased, trimmed and with runs of whitespace collapsed, because the C# location
 * names are not consistently written: "West Of House" sits next to "North of House",
 * and a couple ("Squeaky Room ", "Strange Passage ") carry a trailing space. Matching
 * on the raw string would silently drop the art for those rooms.
 */
export function normaliseLocationName(locationName: string): string {
    return locationName.trim().replace(/\s+/g, ' ').toLowerCase();
}

/**
 * Build a lookup from `room name -> file name`, resolved against `baseUrl`.
 *
 * The room names are written the way the game reports them, so the map reads like the
 * game; normalisation happens here, once.
 */
export function createLocationImageSet(
    baseUrl: string,
    files: Record<string, string>,
): LocationImageSet {
    const base = baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`;
    const set: Record<string, string> = {};
    for (const [locationName, fileName] of Object.entries(files)) {
        set[normaliseLocationName(locationName)] = `${base}${fileName}`;
    }
    return set;
}

/** The image for a room, or undefined when that room has no art. */
export function locationImageUrl(
    images: LocationImageSet,
    locationName: string | undefined | null,
): string | undefined {
    if (!locationName) return undefined;
    return images[normaliseLocationName(locationName)];
}
