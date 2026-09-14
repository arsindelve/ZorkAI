import {useEffect, useRef, useState} from 'react';
import {LocationImageSet, locationImageUrl} from '../locationImages/locationImages';

/** Fade up, drift down the picture, dissolve away. */
export const DEFAULT_LOCATION_IMAGE_FADE_IN_MS = 600;
export const DEFAULT_LOCATION_IMAGE_FADE_OUT_MS = 1200;

/**
 * How long the drift takes to cross the whole picture, and with it how long the picture
 * stays up: the fade does not begin until the pan has arrived at the bottom. Cutting away
 * mid-drift reads as an interruption, and the bottom of the frame is the half the letterbox
 * crop was hiding in the first place - the half worth waiting for.
 */
export const DEFAULT_LOCATION_IMAGE_PAN_MS = 9600;

/**
 * How long a still picture stays up instead. Only used when the player has animations off:
 * there is no pan to wait for then, and holding a motionless frame for the length of one
 * would just be a long pause.
 */
export const DEFAULT_LOCATION_IMAGE_HOLD_MS = 3400;

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
    /**
     * The room's own identity (`GameResponse.locationKey`), which is what the artwork is
     * keyed by. Not the display name: Zork I has two rooms called "Clearing" and only one
     * of them has the grating under the leaves.
     */
    locationKey: string | undefined;
    /** The room's display name, for the picture's alt text. */
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
    fadeOutMs?: number;
    /** How long the drift takes, and so how long the picture stays up. Longer is slower. */
    panMs?: number;
    /** How long a still picture stays up instead, when animations are off. */
    holdMs?: number;
    /**
     * A value that changes whenever the player starts over - a restart, or a restored save.
     * It clears the record of which rooms have already been seen, so the pictures play
     * again for a fresh run through the game.
     */
    resetOn?: string | number;
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
 * It fills the transcript panel and drifts slowly down the picture, dissolving away only
 * once the drift has reached the bottom.
 * Its own bottom edge is masked to nothing, so the room description the player just walked
 * into reads through the frame the whole time it is on screen. Because that bottom stretch
 * shows real, usable transcript, the plate does not take pointer events: a click there hits
 * the word underneath and does what the player expected, and dismisses the picture on its
 * way past. Any keystroke dismisses it too.
 *
 * "First time" is per picture and per playthrough. Per picture because several rooms can
 * share one - the maze is fifteen rooms and a single establishing shot - and it should
 * play in whichever of them the player reaches first, not in all of them. Per playthrough
 * because a restart or a restored save is a fresh run, and the pictures are the point of
 * the feature rather than an achievement to be spent; a plain reload replays them too.
 *
 * A room the player cannot see does not count as an arrival. In the dark nothing is shown
 * and nothing is fetched; the picture waits until they light a lamp or come back carrying
 * one. Showing it anyway would hand the player a look at a room the game is deliberately
 * hiding from them - the same leak issue #238 closed for exits and action chips.
 */
export default function LocationImage({
    locationKey,
    locationName,
    images,
    isDark = false,
    enabled = true,
    fadeInMs = DEFAULT_LOCATION_IMAGE_FADE_IN_MS,
    holdMs = DEFAULT_LOCATION_IMAGE_HOLD_MS,
    fadeOutMs = DEFAULT_LOCATION_IMAGE_FADE_OUT_MS,
    panMs = DEFAULT_LOCATION_IMAGE_PAN_MS,
    resetOn,
    animate = true,
    className,
}: LocationImageProps) {
    const alreadyRequested = useRef<Set<string>>(new Set());
    const failures = useRef<Map<string, number>>(new Map());
    const lastReset = useRef(resetOn);
    const timers = useRef<number[]>([]);
    const [plate, setPlate] = useState<{url: string; name: string; id: number} | null>(null);
    const plateCount = useRef<number>(0);
    const [opaque, setOpaque] = useState<boolean>(false);
    const [panned, setPanned] = useState<boolean>(false);

    // The image loads asynchronously, and the player can change a preference or walk on
    // while it is in flight. The callback reads these through a ref so it acts on what is
    // true when the picture arrives, not on what was true when it was asked for.
    const latest = useRef({animate, panMs, holdMs, fadeOutMs, locationName});
    latest.current = {animate, panMs, holdMs, fadeOutMs, locationName};

    const showing = useRef<boolean>(false);
    showing.current = plate !== null;

    const clearTimers = () => {
        timers.current.forEach((id) => window.clearTimeout(id));
        timers.current = [];
    };

    const after = (ms: number, run: () => void) => {
        timers.current.push(window.setTimeout(run, ms));
    };

    /** Take it down gently. */
    const dismiss = () => {
        if (!showing.current) return;
        clearTimers();
        setOpaque(false);
        after(latest.current.fadeOutMs, () => setPlate(null));
    };

    /** Take it down at once, for the cases where a fade would itself be the bug. */
    const hideNow = () => {
        if (!showing.current) return;
        clearTimers();
        setPlate(null);
    };

    useEffect(() => clearTimers, []);

    // Dismiss on any interaction. Listened for on the window rather than handled on the
    // plate, because the plate must not intercept the click - see pointerEvents below.
    useEffect(() => {
        if (!plate) return;
        const onInteract = () => dismiss();
        window.addEventListener('click', onInteract);
        window.addEventListener('keydown', onInteract);
        return () => {
            window.removeEventListener('click', onInteract);
            window.removeEventListener('keydown', onInteract);
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [plate]);

    useEffect(() => {
        // Whatever is on screen belongs to the room the player has just left, or to a
        // setting they have just turned off. It comes down either way; a picture for the
        // room they are in now replaces it once it has loaded. Turning artwork off and
        // walking into the dark are not gentle exits - a picture of a room the game is
        // refusing to describe is the leak `isDark` exists to close, so it goes at once.
        if (!enabled || isDark) {
            hideNow();
            return;
        }
        dismiss();

        // Checked here rather than in an effect of its own, so the clear is guaranteed to
        // happen before the lookup below. A restart usually lands the player back in the
        // room they started in, so the key alone would not have changed and an effect that
        // only watched the key would never re-run.
        if (lastReset.current !== resetOn) {
            lastReset.current = resetOn;
            alreadyRequested.current.clear();
            failures.current.clear();
        }

        if (!locationKey) return;

        const url = locationImageUrl(images, locationKey);
        if (!url) return;

        // Tracked by the picture rather than the room, because a picture can belong to
        // several: wandering the maze is fifteen rooms sharing one establishing shot, and
        // counting rooms replayed it in every one of them.
        if (alreadyRequested.current.has(url)) return;

        // Marked before the image has loaded, not after. A room whose art has not been
        // drawn yet 404s, and without this it would 404 again on every single visit.
        alreadyRequested.current.add(url);

        let cancelled = false;
        const loader = new Image();

        loader.onload = () => {
            if (cancelled) return;
            failures.current.delete(url);

            // Mount transparent and fade up on the next tick: an element that mounts at
            // its final opacity has nothing to transition from and would simply appear.
            const {animate: panning, panMs: pan, holdMs: hold, fadeOutMs: out} = latest.current;
            clearTimers();
            setPlate({url, name: latest.current.locationName, id: ++plateCount.current});
            setOpaque(false);
            setPanned(false);

            // The pan is the clock. It starts as the picture fades up and the fade out waits
            // for it to finish, so the drift always reaches the bottom of the frame.
            const onScreenMs = panning ? pan : hold;
            after(20, () => {
                setOpaque(true);
                setPanned(true);
            });
            after(20 + onScreenMs, () => setOpaque(false));
            after(20 + onScreenMs + out, () => setPlate(null));
        };

        // A file that is simply not there yet 404s, and staying marked is what stops it
        // being asked for on every visit. A tunnel or a dropped connection is a different
        // thing, and spending the room's picture for the whole run over one bad moment is
        // too harsh - so the first failure is forgiven and only the second sticks.
        loader.onerror = () => {
            const soFar = (failures.current.get(url) ?? 0) + 1;
            failures.current.set(url, soFar);
            if (soFar < 2) alreadyRequested.current.delete(url);
        };

        loader.src = url;

        return () => {
            cancelled = true;
        };
        // isDark is a dependency, not just a guard: when the player lights a lamp the room
        // has not changed, and only re-running on that flip plays the picture they earned.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [locationKey, enabled, isDark, resetOn]);

    if (!plate) return null;

    return (
        <div
            data-testid="location-image"
            role="presentation"
            className={`absolute inset-0 z-20 overflow-hidden rounded-t-lg ${className ?? ''}`}
            style={{
                // The masked bottom of the frame is readable transcript, and it has to stay
                // clickable, scrollable and selectable. Taking pointer events would have the
                // plate quietly eat every one of those for the ten seconds it is up, while
                // looking for all the world like plain text.
                pointerEvents: 'none',
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
                // Keyed, so a room that arrives while the last one is still panning gets a
                // brand new element. React would otherwise reconcile the two onto one <img>,
                // and a reused node carries its in-flight transition across - the incoming
                // picture would glide back up to the top instead of starting there.
                key={plate.id}
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
