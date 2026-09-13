import {
    DEFAULT_COMPASS_SIZE,
    DEFAULT_TRANSCRIPT_FONT_SIZE,
    COMPASS_SIZES,
    COMPASS_SIZE_STORAGE_KEY,
    TRANSCRIPT_FONT_SIZES,
    TRANSCRIPT_FONT_SIZE_STORAGE_KEY,
    compassScale,
    createBooleanPreference,
    isCompassSize,
    isTranscriptFontSize,
    loadCompassSize,
    loadTranscriptFontSize,
    saveCompassSize,
    saveTranscriptFontSize,
    showCompassPreference,
    showCommandsMenuPreference,
    showInventoryButtonPreference,
    showLocationButtonPreference,
    showVerbsMenuPreference,
    transcriptFontSizePx,
    DEFAULT_TRANSCRIPT_FONT,
    TRANSCRIPT_FONTS,
    TRANSCRIPT_FONT_IS_MONOSPACE,
    TRANSCRIPT_FONT_STACKS,
    TRANSCRIPT_FONT_STORAGE_KEY,
    isTranscriptFont,
    loadTranscriptFont,
    saveTranscriptFont,
    transcriptFontStack,
    DEFAULT_TRANSCRIPT_LINE_SPACING,
    TRANSCRIPT_LINE_SPACINGS,
    TRANSCRIPT_LINE_SPACING_STORAGE_KEY,
    isTranscriptLineSpacing,
    loadTranscriptLineSpacing,
    saveTranscriptLineSpacing,
    transcriptLineHeight,
    DEFAULT_TRANSCRIPT_MARKER,
    TRANSCRIPT_MARKERS,
    TRANSCRIPT_MARKER_STORAGE_KEY,
    formatTranscriptMarker,
    isTranscriptMarker,
    loadTranscriptMarker,
    saveTranscriptMarker,
    animationsPreference,
} from '@zork-ai/shared-types';
import {TRANSCRIPT_BASE_FONT_SIZE_PX} from '../transcriptFontSize';

describe('User preferences', () => {
    beforeEach(() => {
        localStorage.clear();
        jest.restoreAllMocks();
    });

    describe('Transcript font size', () => {
        test('defaults to medium when nothing is stored', () => {
            expect(loadTranscriptFontSize()).toBe('medium');
            expect(DEFAULT_TRANSCRIPT_FONT_SIZE).toBe('medium');
        });

        test('round-trips a saved choice through localStorage', () => {
            saveTranscriptFontSize('large');

            expect(localStorage.getItem(TRANSCRIPT_FONT_SIZE_STORAGE_KEY)).toBe('large');
            expect(loadTranscriptFontSize()).toBe('large');
        });

        test('falls back to the default when the stored value is not a known size', () => {
            localStorage.setItem(TRANSCRIPT_FONT_SIZE_STORAGE_KEY, 'gigantic');

            expect(loadTranscriptFontSize()).toBe('medium');
        });

        test('medium renders at exactly the game’s existing size', () => {
            // The whole point of scaling from a per-game base: turning the preference
            // on must not change how the game already looks.
            expect(transcriptFontSizePx(TRANSCRIPT_BASE_FONT_SIZE_PX, 'medium')).toBe(
                TRANSCRIPT_BASE_FONT_SIZE_PX,
            );
        });

        test('every rung is strictly larger than the one below it, in whole pixels', () => {
            const px = TRANSCRIPT_FONT_SIZES.map((size) =>
                transcriptFontSizePx(TRANSCRIPT_BASE_FONT_SIZE_PX, size),
            );

            px.forEach((value, index) => {
                expect(Number.isInteger(value)).toBe(true);
                // Rungs a shared pixel apart would be indistinguishable in the menu.
                if (index > 0) expect(value).toBeGreaterThan(px[index - 1]);
            });
        });

        test('offers five sizes', () => {
            expect(TRANSCRIPT_FONT_SIZES).toHaveLength(5);
        });

        test('scales from each game’s own base, so the games can differ', () => {
            // Zork's base is 15, Planetfall's is 16; one shared scale, two results.
            expect(transcriptFontSizePx(15, 'large')).not.toBe(transcriptFontSizePx(16, 'large'));
        });

        test('isTranscriptFontSize accepts only the five sizes', () => {
            expect(isTranscriptFontSize('small')).toBe(true);
            expect(isTranscriptFontSize('xsmall')).toBe(true);
            expect(isTranscriptFontSize('xlarge')).toBe(true);
            expect(isTranscriptFontSize('huge')).toBe(false);
            expect(isTranscriptFontSize(null)).toBe(false);
            expect(isTranscriptFontSize(12)).toBe(false);
        });
    });

    describe('Transcript font', () => {
        test('defaults to the terminal font the game already used', () => {
            expect(loadTranscriptFont()).toBe('terminal');
            expect(DEFAULT_TRANSCRIPT_FONT).toBe('terminal');
        });

        test('offers exactly five choices', () => {
            expect(TRANSCRIPT_FONTS).toHaveLength(5);
        });

        test('the default stack still leads with Roboto Mono', () => {
            // Whatever else changed, the default must render as it always has.
            expect(transcriptFontStack('terminal')).toContain('Roboto Mono');
        });

        test('the default stack falls back to monospace, never serif', () => {
            // The old font-mono utility fell back to `serif`, which silently turned a
            // monospace transcript proportional on machines without Roboto Mono.
            expect(transcriptFontStack('terminal')).toContain('monospace');
            expect(transcriptFontStack('terminal')).not.toMatch(/(^|[\s,])serif/);
        });

        test('every font has a non-empty stack ending in a generic family', () => {
            TRANSCRIPT_FONTS.forEach((font) => {
                const stack = transcriptFontStack(font);
                expect(stack.length).toBeGreaterThan(0);
                expect(stack).toMatch(/(monospace|serif|sans-serif)$/);
            });
        });

        test('exactly two options preserve column alignment', () => {
            const monospace = TRANSCRIPT_FONTS.filter((f) => TRANSCRIPT_FONT_IS_MONOSPACE[f]);

            expect(monospace).toEqual(['terminal', 'typewriter']);
        });

        test('round-trips a saved choice', () => {
            saveTranscriptFont('storybook');

            expect(localStorage.getItem(TRANSCRIPT_FONT_STORAGE_KEY)).toBe('storybook');
            expect(loadTranscriptFont()).toBe('storybook');
        });

        test('falls back to the default on a corrupt stored value', () => {
            localStorage.setItem(TRANSCRIPT_FONT_STORAGE_KEY, 'comic-sans');

            expect(loadTranscriptFont()).toBe('terminal');
        });

        test('isTranscriptFont accepts only the five fonts', () => {
            expect(isTranscriptFont('typewriter')).toBe(true);
            expect(isTranscriptFont('papyrus')).toBe(false);
            expect(isTranscriptFont(null)).toBe(false);
        });

        test('each font maps to a distinct stack', () => {
            const stacks = TRANSCRIPT_FONTS.map((f) => TRANSCRIPT_FONT_STACKS[f]);

            expect(new Set(stacks).size).toBe(stacks.length);
        });
    });

    describe('Line spacing', () => {
        test('defaults to normal, which is the 1.5 the transcript already used', () => {
            expect(loadTranscriptLineSpacing()).toBe('normal');
            expect(DEFAULT_TRANSCRIPT_LINE_SPACING).toBe('normal');
            expect(transcriptLineHeight('normal')).toBe(1.5);
        });

        test('offers exactly one tighter and one looser option', () => {
            expect(TRANSCRIPT_LINE_SPACINGS).toHaveLength(3);
            expect(transcriptLineHeight('compact')).toBeLessThan(1.5);
            expect(transcriptLineHeight('roomy')).toBeGreaterThan(1.5);
        });

        test('is unitless, so it scales with the font size preference', () => {
            // A px line-height would stay fixed while the text grew, and Extra Large
            // text would collide with the line above it.
            TRANSCRIPT_LINE_SPACINGS.forEach((spacing) => {
                const value = transcriptLineHeight(spacing);
                expect(typeof value).toBe('number');
                expect(value).toBeLessThan(5);
            });
        });

        test('round-trips a saved choice', () => {
            saveTranscriptLineSpacing('compact');

            expect(localStorage.getItem(TRANSCRIPT_LINE_SPACING_STORAGE_KEY)).toBe('compact');
            expect(loadTranscriptLineSpacing()).toBe('compact');
        });

        test('falls back to the default on a corrupt stored value', () => {
            localStorage.setItem(TRANSCRIPT_LINE_SPACING_STORAGE_KEY, 'airy');

            expect(loadTranscriptLineSpacing()).toBe('normal');
        });

        test('isTranscriptLineSpacing accepts only the three options', () => {
            expect(isTranscriptLineSpacing('roomy')).toBe(true);
            expect(isTranscriptLineSpacing('huge')).toBe(false);
        });
    });

    describe('Command marker', () => {
        const at = (hours: number, minutes: number) =>
            new Date(2026, 0, 1, hours, minutes, 0);

        test('defaults to none, so the transcript looks exactly as it does today', () => {
            expect(loadTranscriptMarker()).toBe('none');
            expect(DEFAULT_TRANSCRIPT_MARKER).toBe('none');
            expect(formatTranscriptMarker('none', 7, at(15, 42))).toBe('');
        });

        test('offers three options', () => {
            expect(TRANSCRIPT_MARKERS).toHaveLength(3);
        });

        test('numbers commands sequentially', () => {
            expect(formatTranscriptMarker('number', 1, at(15, 42))).toBe('#1');
            expect(formatTranscriptMarker('number', 42, at(15, 42))).toBe('#42');
        });

        test('formats the time from the clock it is given', () => {
            // The clock is injected so this does not depend on when the suite runs.
            const formatted = formatTranscriptMarker('time', 1, at(15, 42));

            expect(formatted).toMatch(/\b42\b/);
            expect(formatted).not.toBe('');
        });

        test('an unknown marker renders nothing rather than throwing', () => {
            expect(
                formatTranscriptMarker('nonsense' as never, 3, at(9, 5)),
            ).toBe('');
        });

        test('round-trips a saved choice', () => {
            saveTranscriptMarker('time');

            expect(localStorage.getItem(TRANSCRIPT_MARKER_STORAGE_KEY)).toBe('time');
            expect(loadTranscriptMarker()).toBe('time');
        });

        test('falls back to the default on a corrupt stored value', () => {
            localStorage.setItem(TRANSCRIPT_MARKER_STORAGE_KEY, 'emoji');

            expect(loadTranscriptMarker()).toBe('none');
        });

        test('isTranscriptMarker accepts only the three options', () => {
            expect(isTranscriptMarker('number')).toBe(true);
            expect(isTranscriptMarker('turn')).toBe(false);
        });
    });

    describe('Compass size', () => {
        test('defaults to medium, which is scale 1 - the current size', () => {
            expect(loadCompassSize()).toBe('medium');
            expect(DEFAULT_COMPASS_SIZE).toBe('medium');
            expect(compassScale('medium')).toBe(1);
        });

        test('offers two steps down and one up, each strictly ordered', () => {
            const scales = COMPASS_SIZES.map(compassScale);

            expect(scales).toHaveLength(4);
            scales.forEach((value, index) => {
                if (index > 0) expect(value).toBeGreaterThan(scales[index - 1]);
            });
            expect(compassScale('xsmall')).toBeLessThan(compassScale('small'));
            expect(compassScale('large')).toBeGreaterThan(1);
        });

        test('round-trips a saved choice', () => {
            saveCompassSize('large');

            expect(localStorage.getItem(COMPASS_SIZE_STORAGE_KEY)).toBe('large');
            expect(loadCompassSize()).toBe('large');
        });

        test('falls back to the default on a corrupt stored value', () => {
            localStorage.setItem(COMPASS_SIZE_STORAGE_KEY, 'enormous');

            expect(loadCompassSize()).toBe('medium');
        });

        test('isCompassSize accepts only its own sizes', () => {
            expect(isCompassSize('small')).toBe(true);
            expect(isCompassSize('xsmall')).toBe(true);
            expect(isCompassSize('xlarge')).toBe(false);
            expect(isCompassSize(undefined)).toBe(false);
        });
    });

    describe('On/off toggles', () => {
        const toggles = [
            ['compass', showCompassPreference],
            ['verbs menu', showVerbsMenuPreference],
            ['commands menu', showCommandsMenuPreference],
            ['location button', showLocationButtonPreference],
            ['inventory button', showInventoryButtonPreference],
            ['animations', animationsPreference],
        ] as const;

        test.each(toggles)('%s defaults to on', (_name, preference) => {
            expect(preference.load()).toBe(true);
        });

        test.each(toggles)('%s round-trips off and back on', (_name, preference) => {
            preference.save(false);
            expect(preference.load()).toBe(false);

            preference.save(true);
            expect(preference.load()).toBe(true);
        });

        test('each toggle uses its own distinct storage key', () => {
            const keys = toggles.map(([, preference]) => preference.key);

            expect(new Set(keys).size).toBe(keys.length);
        });

        test('a corrupt stored value falls back to the default', () => {
            localStorage.setItem(showCompassPreference.key, 'yes-please');

            expect(showCompassPreference.load()).toBe(true);
        });

        test('a preference defaulting to off is not flipped on by junk', () => {
            // Guards the tempting `stored !== 'false'` shortcut, which would read any
            // unrecognised value as true and silently invert a default-off preference.
            const offByDefault = createBooleanPreference('testOnlyDefaultOff', false);
            localStorage.setItem(offByDefault.key, 'whatever');

            expect(offByDefault.load()).toBe(false);
        });
    });

    describe('when localStorage is unavailable', () => {
        // Browsers set to block site data *throw* on access rather than returning
        // null, and a cosmetic preference must never take the game down.
        test('reads fall back to defaults instead of throwing', () => {
            jest.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
                throw new Error('Access denied');
            });

            expect(loadTranscriptFontSize()).toBe('medium');
            expect(loadTranscriptFont()).toBe('terminal');
            expect(loadCompassSize()).toBe('medium');
            expect(showCompassPreference.load()).toBe(true);
        });

        test('writes are swallowed instead of throwing', () => {
            jest.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
                throw new Error('Quota exceeded');
            });

            expect(() => saveTranscriptFontSize('large')).not.toThrow();
            expect(() => saveCompassSize('small')).not.toThrow();
            expect(() => showCompassPreference.save(false)).not.toThrow();
        });
    });
});
