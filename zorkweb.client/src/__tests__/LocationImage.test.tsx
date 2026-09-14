import React from 'react';
import {act, render, screen} from '@testing-library/react';
import fs from 'fs';
import path from 'path';
import {LocationImage, createLocationImageSet} from '@zork-ai/shared-types';
import {ZORK_LOCATION_IMAGES} from '../locationImages';

/**
 * jsdom never fetches an <img>, so the component's preloader would hang forever. This
 * stand-in records every instance and lets each test decide whether that file loads or
 * 404s, which is the distinction the component turns on.
 */
class FakeImage {
    static instances: FakeImage[] = [];
    onload: (() => void) | null = null;
    onerror: (() => void) | null = null;
    private _src = '';

    constructor() {
        FakeImage.instances.push(this);
    }

    set src(value: string) {
        this._src = value;
    }

    get src(): string {
        return this._src;
    }

    static reset() {
        FakeImage.instances = [];
    }

    static get last(): FakeImage {
        return FakeImage.instances[FakeImage.instances.length - 1];
    }
}

const FADE_IN = 100;
const HOLD = 1000;
const FADE_OUT = 200;

const IMAGES = createLocationImageSet('https://example.test/locations/', {
    WestOfHouse: 'WestOfHouse.webp',
    Kitchen: 'Kitchen.webp',
    // Rooms that are the same place to the player and share one picture, as the fifteen
    // maze rooms and the two Caves do.
    MazeOne: 'Maze.webp',
    MazeTwo: 'Maze.webp',
});

const PAN = 4000;
/** Past the fade-up, the whole drift and the fade-out - the picture is gone after this. */
const PLATE_LIFE = 20 + PAN + FADE_OUT;

const plate = (
    locationKey: string,
    {enabled = true, isDark = false, animate = true, locationName = locationKey, resetOn = 0} = {},
) => (
    <LocationImage
        locationKey={locationKey}
        locationName={locationName}
        images={IMAGES}
        enabled={enabled}
        isDark={isDark}
        animate={animate}
        resetOn={resetOn}
        panMs={PAN}
        holdMs={HOLD}
        fadeInMs={FADE_IN}
        fadeOutMs={FADE_OUT}
    />
);

const renderPlate = (locationKey: string, enabled = true) => render(plate(locationKey, {enabled}));

/** The component waits a tick before fading up, so it can transition from transparent. */
const settle = () => act(() => void jest.advanceTimersByTime(20 + FADE_IN));

const loadTheImage = () => act(() => FakeImage.last.onload?.());

describe('LocationImage', () => {
    let originalImage: typeof Image;

    beforeEach(() => {
        jest.useFakeTimers();
        originalImage = global.Image;
        // @ts-expect-error - a deliberately minimal stand-in for the DOM Image
        global.Image = FakeImage;
        FakeImage.reset();
    });

    afterEach(() => {
        global.Image = originalImage;
        jest.useRealTimers();
    });

    test('shows the plate once the art for the room has loaded', () => {
        render(plate('WestOfHouse', {locationName: 'West Of House'}));

        expect(FakeImage.last.src).toBe('https://example.test/locations/WestOfHouse.webp');
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();

        loadTheImage();
        settle();

        expect(screen.getByTestId('location-image')).toHaveStyle({opacity: '1'});
        expect(screen.getByTestId('location-image-picture')).toHaveAttribute(
            'src',
            'https://example.test/locations/WestOfHouse.webp',
        );
        expect(screen.getByTestId('location-image-picture')).toHaveAttribute(
            'alt',
            'West Of House',
        );
    });

    test('requests nothing for a room with no art', () => {
        renderPlate('TrollRoom');

        expect(FakeImage.instances).toHaveLength(0);
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('shows nothing when the file is missing rather than a broken image', () => {
        renderPlate('Kitchen');

        act(() => FakeImage.last.onerror?.());
        settle();

        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('drifts from the top of the picture to the bottom while it is on screen', () => {
        // The art is square and the panel is a short letterbox, so a fixed crop would show
        // only a band of the middle.
        render(plate('Kitchen'));
        loadTheImage();

        const picture = () => screen.getByTestId('location-image-picture');
        expect(picture()).toHaveStyle({objectPosition: 'center top'});

        settle();
        expect(picture()).toHaveStyle({objectPosition: 'center bottom'});
        // Its own duration, deliberately longer than the plate is ever up: the drift is a
        // slow reveal that never has to arrive anywhere.
        expect(picture().style.transition).toBe(`object-position ${PAN}ms linear`);
    });

    test('starts a room that arrives mid-pan over from the top', () => {
        // React reconciles the two rooms onto the same <img>, and a reused node keeps the
        // transition it is in the middle of - so the new picture inherited the old one's
        // slide and picked up wherever it had got to. A fresh node has nothing to inherit.
        const {rerender} = render(plate('Kitchen'));
        loadTheImage();
        settle();

        const midPan = screen.getByTestId('location-image-picture');
        expect(midPan).toHaveStyle({objectPosition: 'center bottom'});

        rerender(plate('WestOfHouse'));
        loadTheImage();

        const replacement = screen.getByTestId('location-image-picture');
        expect(replacement).not.toBe(midPan);
        expect(replacement).toHaveStyle({objectPosition: 'center top'});

        settle();
        expect(screen.getByTestId('location-image-picture')).toHaveStyle({
            objectPosition: 'center bottom',
        });
    });

    test('holds a centre crop instead of panning when animations are off', () => {
        // The global reduce-motion rule zeroes every duration, which would slam the pan to
        // the bottom of the frame in one frame rather than suppress it.
        render(plate('Kitchen', {animate: false}));
        loadTheImage();
        settle();

        const picture = screen.getByTestId('location-image-picture');
        expect(picture).toHaveStyle({objectPosition: 'center center'});
        expect(picture.style.transition).toBe('');
    });

    test('masks its own bottom edge so the transcript reads through it', () => {
        render(plate('Kitchen'));
        loadTheImage();
        settle();

        expect(screen.getByTestId('location-image').style.maskImage).toContain(
            'rgba(0, 0, 0, 0) 72%',
        );
    });

    test('waits for the drift to reach the bottom before it fades', () => {
        renderPlate('Kitchen');
        loadTheImage();
        settle();

        // Still up well past the point the old fixed hold would have cut it away.
        act(() => void jest.advanceTimersByTime(PAN - FADE_IN - 100));
        expect(screen.getByTestId('location-image')).toHaveStyle({opacity: '1'});

        act(() => void jest.advanceTimersByTime(200));
        expect(screen.getByTestId('location-image')).toHaveStyle({opacity: '0'});

        act(() => void jest.advanceTimersByTime(FADE_OUT));
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('a still picture uses the shorter hold, with no pan to wait for', () => {
        render(plate('Kitchen', {animate: false}));
        loadTheImage();
        settle();

        act(() => void jest.advanceTimersByTime(HOLD));
        expect(screen.getByTestId('location-image')).toHaveStyle({opacity: '0'});
    });

    test('a click anywhere dismisses it early', () => {
        renderPlate('Kitchen');
        loadTheImage();
        settle();

        // Not on the plate: it takes no pointer events, so the click lands on whatever is
        // underneath and is heard on the window on its way past.
        act(() => document.body.click());
        expect(screen.getByTestId('location-image')).toHaveStyle({opacity: '0'});

        act(() => void jest.advanceTimersByTime(FADE_OUT));
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('lets clicks through to the transcript underneath', () => {
        // The masked bottom of the frame shows real, readable text. A plate that took
        // pointer events would swallow every click aimed at it for ten seconds while
        // looking exactly like plain transcript.
        renderPlate('Kitchen');
        loadTheImage();
        settle();

        expect(screen.getByTestId('location-image')).toHaveStyle({pointerEvents: 'none'});
    });

    test('a keystroke dismisses it early', () => {
        renderPlate('Kitchen');
        loadTheImage();
        settle();

        act(() => {
            window.dispatchEvent(new KeyboardEvent('keydown', {key: 'Escape'}));
        });
        act(() => void jest.advanceTimersByTime(FADE_OUT));

        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('comes down at once when the Artwork preference is turned off', () => {
        const {rerender} = render(plate('Kitchen'));
        loadTheImage();
        settle();
        expect(screen.getByTestId('location-image')).toBeInTheDocument();

        rerender(plate('Kitchen', {enabled: false}));

        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('comes down at once when the room goes dark', () => {
        // A lit picture of a room the server has started refusing to describe is the leak
        // the darkness flag exists to close.
        const {rerender} = render(plate('Kitchen'));
        loadTheImage();
        settle();

        rerender(plate('Kitchen', {isDark: true}));

        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('does not linger over the next room when that room has no art', () => {
        const {rerender} = render(plate('Kitchen'));
        loadTheImage();
        settle();

        rerender(plate('TrollRoom'));
        act(() => void jest.advanceTimersByTime(FADE_OUT));

        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('retries a picture once after a failed fetch', () => {
        // A tunnel or a dropped connection should not spend the room's picture for the
        // whole run; a file that is genuinely missing still stops asking.
        const {rerender} = render(plate('Kitchen'));
        act(() => FakeImage.last.onerror?.());
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();

        rerender(plate('TrollRoom'));
        rerender(plate('Kitchen'));
        expect(FakeImage.instances).toHaveLength(2);

        // Second failure sticks.
        act(() => FakeImage.last.onerror?.());
        rerender(plate('TrollRoom'));
        rerender(plate('Kitchen'));
        expect(FakeImage.instances).toHaveLength(2);
    });

    test('shows the art for a room only on the first arrival', () => {
        const {rerender} = renderPlate('Kitchen');
        loadTheImage();
        settle();
        act(() => void jest.advanceTimersByTime(PLATE_LIFE));
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();

        const goTo = (locationName: string) => rerender(plate(locationName));

        goTo('WestOfHouse');
        expect(FakeImage.instances).toHaveLength(2);

        goTo('Kitchen');
        expect(FakeImage.instances).toHaveLength(2);
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('a picture shared by several rooms plays only in the first of them', () => {
        // Wandering the maze is fifteen rooms with one picture between them. Counting the
        // room rather than the picture meant it played again in every one.
        const {rerender} = render(plate('MazeOne'));
        loadTheImage();
        settle();
        act(() => void jest.advanceTimersByTime(PLATE_LIFE));
        expect(FakeImage.instances).toHaveLength(1);

        rerender(plate('MazeTwo'));

        expect(FakeImage.instances).toHaveLength(1);
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('plays the rooms again after the player starts over', () => {
        const {rerender} = render(plate('Kitchen'));
        loadTheImage();
        settle();
        act(() => void jest.advanceTimersByTime(PLATE_LIFE));
        expect(FakeImage.instances).toHaveLength(1);

        // Restarting usually lands the player back where they began, so the room key has
        // not changed - only the playthrough has.
        rerender(plate('Kitchen', {resetOn: 1}));
        loadTheImage();
        settle();

        expect(FakeImage.instances).toHaveLength(2);
        expect(screen.getByTestId('location-image')).toBeInTheDocument();
    });

    test('shows nothing in the dark, and does not spend the room on the visit', () => {
        // A picture of a room the player cannot see is the leak issue #238 closed for
        // exits and action chips.
        const {rerender} = render(plate('Kitchen', {isDark: true}));

        expect(FakeImage.instances).toHaveLength(0);
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();

        // Striking a light in the same room is the arrival the player earned.
        rerender(plate('Kitchen', {isDark: false}));
        loadTheImage();
        settle();

        expect(screen.getByTestId('location-image')).toBeInTheDocument();
    });

    test('shows on arrival when the player walks in already carrying a light', () => {
        render(plate('Kitchen', {isDark: false}));
        loadTheImage();
        settle();

        expect(screen.getByTestId('location-image')).toBeInTheDocument();
    });

    test('does not replay a room that was already shown lit', () => {
        const {rerender} = render(plate('Kitchen'));
        loadTheImage();
        settle();
        act(() => void jest.advanceTimersByTime(PLATE_LIFE));

        // The lamp going out and coming back on is not a fresh arrival.
        rerender(plate('Kitchen', {isDark: true}));
        rerender(plate('Kitchen', {isDark: false}));

        expect(FakeImage.instances).toHaveLength(1);
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('fetches nothing while the Artwork preference is off', () => {
        renderPlate('Kitchen', false);

        expect(FakeImage.instances).toHaveLength(0);
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });
});

/**
 * The engine's own room list, read from the C# it is generated against.
 *
 * The registry keys are location class names, so this is the only thing that can tell us
 * a key still refers to a real room. Class declarations, not file names: MazeBase.cs alone
 * holds sixteen of them.
 */
function readZorkRooms(): {displayNames: Map<string, string>; abstractBases: Set<string>} | null {
    const root = path.resolve(__dirname, '../../../ZorkOne/Location');
    // The engine is a sibling project, not a dependency. A client-only checkout or a build
    // context that copies just this package has nothing to read, and these checks sit out
    // rather than erroring on setup and hiding whatever else the suite would have said.
    if (!fs.existsSync(root)) return null;
    const declaration =
        /^\s*(?:public|internal)\s+(abstract\s+|sealed\s+)?class\s+(\w+)\s*:\s*([\w<>, ]+)/gm;
    const bases = new Map<string, string>();
    const ownName = new Map<string, string>();
    const abstractBases = new Set<string>();
    const all = new Set<string>();

    const walk = (dir: string) => {
        for (const entry of fs.readdirSync(dir, {withFileTypes: true})) {
            const full = path.join(dir, entry.name);
            if (entry.isDirectory()) {
                walk(full);
                continue;
            }
            if (!entry.name.endsWith('.cs')) continue;

            const source = fs.readFileSync(full, 'utf8');
            const found = [...source.matchAll(declaration)];
            found.forEach((match, index) => {
                const [, modifier, name, base] = match;
                const body = source.slice(
                    match.index! + match[0].length,
                    found[index + 1]?.index ?? source.length,
                );
                all.add(name);
                bases.set(name, base.split(',')[0].trim());
                if (modifier?.includes('abstract')) abstractBases.add(name);
                // Only a plain string literal counts. A computed Name (`=> IsDrained ? "X"
                // : "Y"`) would otherwise fall through to the base class's name and quietly
                // make the shared-picture assertion pass or fail for the wrong reason, so
                // it is recorded as its own unknown value instead.
                const declaresName = /override string Name\s*=>/.test(body);
                const declared = body.match(/override string Name\s*=>\s*"([^"]+)"\s*;/);
                if (declared) ownName.set(name, declared[1].trim());
                else if (declaresName) ownName.set(name, `<computed:${name}>`);
            });
        }
    };
    walk(root);

    // A room with no Name of its own takes its base class's - every Maze room this way.
    const resolve = (name: string, seen = new Set<string>()): string | undefined => {
        if (ownName.has(name)) return ownName.get(name);
        const base = bases.get(name);
        if (!base || seen.has(name)) return undefined;
        return resolve(base, new Set([...seen, name]));
    };

    const displayNames = new Map<string, string>();
    for (const name of all) {
        if (abstractBases.has(name)) continue;
        const resolved = resolve(name);
        if (resolved) displayNames.set(name, resolved.trim());
    }
    return {displayNames, abstractBases};
}

describe('Zork location artwork', () => {
    const entries = Object.entries(ZORK_LOCATION_IMAGES);
    const url = (locationKey: string) => ZORK_LOCATION_IMAGES[locationKey];
    const BASE = 'https://zorkai-assets.s3.amazonaws.com/locations/';

    test('the two Clearings get different pictures', () => {
        // They are both called "Clearing" to the player, so keying on the display name put
        // the grating-under-the-leaves picture in the clearing east of the house as well.
        expect(url('Clearing')).toBe(`${BASE}Clearing.webp`);
        expect(url('ClearingBehindHouse')).toBe(`${BASE}ClearingBehindHouse.webp`);
    });

    test('rooms whose file name is not simply their own name still resolve', () => {
        expect(url('TrollRoom')).toBe(`${BASE}TrollRoom.webp`);
        expect(url('MazeOne')).toBe(`${BASE}Maze.webp`);
        expect(url('LadderBottom')).toBe(`${BASE}LadderBottom.webp`);
        expect(url('UpATree')).toBe(`${BASE}UpATree.webp`);
    });

    test('every room resolves to a .webp under the bucket', () => {
        expect(entries.length).toBeGreaterThan(0);
        for (const [, value] of entries) {
            expect(value).toMatch(
                /^https:\/\/zorkai-assets\.s3\.amazonaws\.com\/locations\/[A-Za-z]+\.webp$/,
            );
        }
    });

    test('every key is a room the player can actually stand in', () => {
        // Keys for an abstract base (MazeBase, MirrorRoom) match nothing at runtime, which
        // is silent: the room just never shows its picture. Both mistakes happened.
        const rooms = readZorkRooms();
        if (!rooms) return;
        const {displayNames, abstractBases} = rooms;

        expect(displayNames.size).toBeGreaterThan(100);
        expect(abstractBases.has('MazeBase')).toBe(true);

        const keys = entries.map(([key]) => key);
        expect(keys.filter((key) => !displayNames.has(key))).toEqual([]);
        expect(keys.filter((key) => abstractBases.has(key))).toEqual([]);
    });

    test('a picture is only shared by rooms that are the same place to the player', () => {
        // The maze is fifteen rooms and one picture; so are both Caves, both Mirror Rooms
        // and both ends of White Cliffs Beach. Sharing beyond that would be a mix-up.
        const rooms = readZorkRooms();
        if (!rooms) return;
        const {displayNames} = rooms;
        const byFile = new Map<string, string[]>();
        for (const [key, value] of entries) {
            byFile.set(value, [...(byFile.get(value) ?? []), key]);
        }

        for (const [file, keys] of byFile) {
            const names = new Set(keys.map((key) => displayNames.get(key)));
            expect({file, names: [...names]}).toEqual({file, names: [names.values().next().value]});
        }
    });
});
