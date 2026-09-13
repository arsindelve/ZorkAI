import {useEffect, useRef, useState} from 'react';
import {
    LocationImageSet,
    locationImageUrl,
    normaliseLocationName,
} from '../locationImages/locationImages';

/** Fade up, hold, dissolve away. Tuned so the plate reads as a beat, not an interruption. */
export const DEFAULT_LOCATION_IMAGE_FADE_IN_MS = 600;
export const DEFAULT_LOCATION_IMAGE_HOLD_MS = 3400;
export const DEFAULT_LOCATION_IMAGE_FADE_OUT_MS = 1200;

/**
 * Opaque across the top, gone well before the bottom of the frame.
 *
 * The ramp drops off its own steepest stretch early rather than sliding evenly to the
 * floor: a half-lit picture behind pale text is the one state that reads as neither, and
 * the transcript is bottom-anchored, so its newest lines - the room description the player
 * just walked into - land in the bottom third and need it genuinely clear, not merely dim.
 */
const BOTTOM_FADE =
    'linear-gradient(to bottom, rgba(0, 0, 0, 1) 0%, rgba(0, 0, 0, 1) 30%, ' +
    'rgba(0, 0, 0, 0.35) 58%, rgba(0, 0, 0, 0) 72%)';

type LocationImageProps = {
    /** The room the player is in now, exactly as the server reports it. */
    locationName: string;
    images: LocationImageSet;
    /**
     * True when the player is somewhere unlit (`GameResponse.itIsDarkHere`). Nothing is
     * shown and nothing is fetched, and the room is not counted as seen - so the picture
     * is still waiting when the player strikes a light.
     */
    isDark?: boolean;
    /** The player's Artwork preference. Off means nothing is shown and nothing is fetched. */
    enabled?: boolean;
    fadeInMs?: number;
    holdMs?: number;
    fadeOutMs?: number;
    /**
     * The player's Animations preference. Off means no pan - the picture sits on its
     * centre crop instead. The global reduce-motion CSS would otherwise collapse the
     * pan's duration to nothing and slam it straight to the bottom of the frame.
     */
    animate?: boolean;
    className?: string;
};

/**
 * An establishing shot for a room, shown the first time the player arrives there.
 *
 * It fills the transcript panel and drifts slowly down the picture before dissolving away.
 * Its own bottom edge is masked to nothing, so the room description the player just walked
 * into reads through the frame the whole time it is on screen. A click dismisses it early.
 *
 * "First time" is per browser session, held in a ref rather than localStorage: a player
 * who reloads or restores a save is arriving somewhere fresh again, and the pictures are
 * the point of the feature, not an achievement to be spent.
 *
 * A room the player cannot see does not count as an arrival. In the dark nothing is shown
 * and nothing is fetched; the picture waits until they light a lamp or come back carrying
 * one. Showing it anyway would hand the player a look at a room the game is deliberately
 * hiding from them - the same leak issue #238 closed for exits and action chips.
 */
export default function LocationImage({
    locationName,
    images,
    isDark = false,
    enabled = true,
    fadeInMs = DEFAULT_LOCATION_IMAGE_FADE_IN_MS,
    holdMs = DEFAULT_LOCATION_IMAGE_HOLD_MS,
    fadeOutMs = DEFAULT_LOCATION_IMAGE_FADE_OUT_MS,
    animate = true,
    className,
}: LocationImageProps) {
    const alreadyRequested = useRef<Set<string>>(new Set());
    const timers = useRef<number[]>([]);
    const [plate, setPlate] = useState<{url: string; name: string} | null>(null);
    const [opaque, setOpaque] = useState<boolean>(false);
    const [panned, setPanned] = useState<boolean>(false);

    const clearTimers = () => {
        timers.current.forEach((id) => window.clearTimeout(id));
        timers.current = [];
    };

    const after = (ms: number, run: () => void) => {
        timers.current.push(window.setTimeout(run, ms));
    };

    useEffect(() => clearTimers, []);

    useEffect(() => {
        if (!enabled || isDark) return;

        const url = locationImageUrl(images, locationName);
        if (!url) return;

        const key = normaliseLocationName(locationName);
        if (alreadyRequested.current.has(key)) return;

        // Marked before the image has loaded, not after. A room whose art has not been
        // drawn yet 404s, and without this it would 404 again on every single visit.
        alreadyRequested.current.add(key);

        let cancelled = false;
        const loader = new Image();

        loader.onload = () => {
            if (cancelled) return;
            // Mount transparent and fade up on the next tick: an element that mounts at
            // its final opacity has nothing to transition from and would simply appear.
            clearTimers();
            setPlate({url, name: locationName});
            setOpaque(false);
            setPanned(false);
            after(20, () => {
                setOpaque(true);
                setPanned(true);
            });
            after(20 + fadeInMs + holdMs, () => setOpaque(false));
            after(20 + fadeInMs + holdMs + fadeOutMs, () => setPlate(null));
        };

        // No art for this room after all - stay out of the way rather than flash a
        // broken image over the transcript.
        loader.onerror = () => {};

        loader.src = url;

        return () => {
            cancelled = true;
        };
        // isDark is a dependency, not just a guard: when the player lights a lamp the room
        // has not changed, and only re-running on that flip plays the picture they earned.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [locationName, enabled, isDark]);

    if (!plate) return null;

    // One continuous drift across the picture's whole life on screen, fades included.
    const panMs = fadeInMs + holdMs + fadeOutMs;

    const dismiss = () => {
        clearTimers();
        setOpaque(false);
        after(fadeOutMs, () => setPlate(null));
    };

    return (
        <div
            data-testid="location-image"
            onClick={dismiss}
            className={`absolute inset-0 z-20 overflow-hidden rounded-t-lg cursor-pointer ${className ?? ''}`}
            style={{
                opacity: opaque ? 1 : 0,
                transition: `opacity ${opaque ? fadeInMs : fadeOutMs}ms ease-in-out`,
                // The picture dissolves into the transcript rather than sitting on top of
                // it: the panel is bottom-anchored, so the room description the player just
                // walked into is right here at the bottom, readable through the fade.
                maskImage: BOTTOM_FADE,
                WebkitMaskImage: BOTTOM_FADE,
            }}
        >
            <img
                src={plate.url}
                alt={plate.name}
                data-testid="location-image-picture"
                className="h-full w-full object-cover"
                style={{
                    // The art is square and the panel is a short wide letterbox, so a fixed
                    // crop would throw most of the picture away. Drift from the top edge to
                    // the bottom instead - as percentages, object-position lands on both
                    // edges exactly, whatever the panel and the image actually measure.
                    objectPosition: !animate
                        ? 'center center'
                        : panned
                          ? 'center bottom'
                          : 'center top',
                    transition: animate ? `object-position ${panMs}ms linear` : undefined,
                }}
            />
        </div>
    );
}
