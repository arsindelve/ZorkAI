/**
 * Artwork for the handful of rooms that have a picture drawn for them.
 *
 * Keyed by `GameResponse.locationKey` - the room's own identity - and not by its display
 * name, which is not unique: Zork I has two rooms called "Clearing", two called "Cave"
 * and four called "Forest". Only one of the Clearings has the grating hidden under the
 * leaves, so keying on the name would put its picture in both.
 *
 * A registry rather than a URL derived from the key on the fly: most rooms have no art,
 * and guessing a URL for them would fire a 404 on every first visit.
 */

/** Room key -> absolute image URL. */
export interface LocationImageSet {
    readonly [locationKey: string]: string;
}

/**
 * Build a lookup from `room key -> file name`, resolved against `baseUrl`.
 *
 * Two rooms may share a file - the north and south Caves are the same cave to a player -
 * so file names are not required to be unique.
 */
export function createLocationImageSet(
    baseUrl: string,
    files: Record<string, string>,
): LocationImageSet {
    const base = baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`;
    // No prototype: the keys are C# class names, and a room class called `Constructor` or
    // `ToString` would otherwise have the lookup hand back an inherited function. It is
    // truthy, so it would sail past the "no art for this room" check and be assigned to an
    // <img> src, which string-coerces it into a 404 against our own origin.
    const set: Record<string, string> = Object.create(null);
    for (const [locationKey, fileName] of Object.entries(files)) {
        set[locationKey] = `${base}${fileName}`;
    }
    return set;
}

/** The image for a room, or undefined when that room has no art. */
export function locationImageUrl(
    images: LocationImageSet,
    locationKey: string | undefined | null,
): string | undefined {
    if (!locationKey) return undefined;
    return images[locationKey];
}
