import React from 'react';
import {act, render, screen} from '@testing-library/react';
import {LocationImage, createLocationImageSet, normaliseLocationName} from '@zork-ai/shared-types';
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
    'West Of House': 'WestOfHouse.webp',
    Kitchen: 'Kitchen.webp',
});

const plate = (locationName: string, {enabled = true, isDark = false, animate = true} = {}) => (
    <LocationImage
        locationName={locationName}
        images={IMAGES}
        enabled={enabled}
        isDark={isDark}
        animate={animate}
        fadeInMs={FADE_IN}
        holdMs={HOLD}
        fadeOutMs={FADE_OUT}
    />
);

const renderPlate = (locationName: string, enabled = true) =>
    render(plate(locationName, {enabled}));

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
        renderPlate('West Of House');

        expect(FakeImage.last.src).toBe('https://example.test/locations/WestOfHouse.webp');
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();

        loadTheImage();
        settle();

        const plate = screen.getByTestId('location-image');
        expect(plate).toHaveStyle({opacity: '1'});
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
        renderPlate('Troll Room');

        expect(FakeImage.instances).toHaveLength(0);
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('matches the room name however the engine cased or padded it', () => {
        renderPlate('  west of house ');

        expect(FakeImage.last.src).toBe('https://example.test/locations/WestOfHouse.webp');
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
        // One drift across the whole visible life, fades included - not just the hold.
        expect(picture().style.transition).toBe(
            `object-position ${FADE_IN + HOLD + FADE_OUT}ms linear`,
        );
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

    test('fades out after the hold and unmounts', () => {
        renderPlate('Kitchen');
        loadTheImage();
        settle();

        act(() => void jest.advanceTimersByTime(HOLD));
        expect(screen.getByTestId('location-image')).toHaveStyle({opacity: '0'});

        act(() => void jest.advanceTimersByTime(FADE_OUT));
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('a click dismisses it early', () => {
        renderPlate('Kitchen');
        loadTheImage();
        settle();

        act(() => screen.getByTestId('location-image').click());
        expect(screen.getByTestId('location-image')).toHaveStyle({opacity: '0'});

        act(() => void jest.advanceTimersByTime(FADE_OUT));
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
    });

    test('shows the art for a room only on the first arrival', () => {
        const {rerender} = renderPlate('Kitchen');
        loadTheImage();
        settle();
        act(() => void jest.advanceTimersByTime(HOLD + FADE_OUT));
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();

        const goTo = (locationName: string) => rerender(plate(locationName));

        goTo('West Of House');
        expect(FakeImage.instances).toHaveLength(2);

        goTo('Kitchen');
        expect(FakeImage.instances).toHaveLength(2);
        expect(screen.queryByTestId('location-image')).not.toBeInTheDocument();
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
        act(() => void jest.advanceTimersByTime(HOLD + FADE_OUT));

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

describe('Zork location artwork', () => {
    const entries = Object.entries(ZORK_LOCATION_IMAGES);

    test('the rooms named in this conversation point at their own file', () => {
        const url = (room: string) => ZORK_LOCATION_IMAGES[normaliseLocationName(room)];

        expect(url('West Of House')).toBe(
            'https://zorkai-assets.s3.amazonaws.com/locations/WestOfHouse.webp',
        );
        expect(url('North of House')).toBe(
            'https://zorkai-assets.s3.amazonaws.com/locations/NorthOfHouse.webp',
        );
        expect(url('South of House')).toBe(
            'https://zorkai-assets.s3.amazonaws.com/locations/SouthOfHouse.webp',
        );
        expect(url('Up A Tree')).toBe(
            'https://zorkai-assets.s3.amazonaws.com/locations/UpATree.webp',
        );
        expect(url('Kitchen')).toBe(
            'https://zorkai-assets.s3.amazonaws.com/locations/Kitchen.webp',
        );
        // The one room whose file name is not simply its name with the spaces taken out.
        expect(url('The Troll Room')).toBe(
            'https://zorkai-assets.s3.amazonaws.com/locations/TrollRoom.webp',
        );
    });

    test('every room resolves to a .webp under the bucket', () => {
        expect(entries.length).toBeGreaterThan(0);
        for (const [, url] of entries) {
            expect(url).toMatch(
                /^https:\/\/zorkai-assets\.s3\.amazonaws\.com\/locations\/[A-Za-z]+\.webp$/,
            );
        }
    });

    test('no two rooms collide on the same lookup key', () => {
        // The set is keyed by the normalised name, so a duplicate would not error - the
        // later room would silently take the earlier one's picture.
        expect(new Set(entries.map(([key]) => key)).size).toBe(entries.length);
    });

    test('every room has its own file', () => {
        const files = entries.map(([, url]) => url);
        expect(new Set(files).size).toBe(files.length);
    });
});
