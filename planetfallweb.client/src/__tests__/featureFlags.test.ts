import {isFeatureEnabled} from '@zork-ai/shared-types';

describe('isFeatureEnabled', () => {
    const setUrl = (search: string) => {
        window.history.replaceState({}, '', `/${search}`);
    };

    beforeEach(() => {
        localStorage.clear();
        setUrl('');
    });

    test('returns the build default when nothing overrides it', () => {
        expect(isFeatureEnabled('hints', false)).toBe(false);
        expect(isFeatureEnabled('hints', true)).toBe(true);
    });

    test('?hints=1 forces the flag on over a false build default', () => {
        setUrl('?hints=1');
        expect(isFeatureEnabled('hints', false)).toBe(true);
    });

    test('?hints=0 forces the flag off over a true build default', () => {
        setUrl('?hints=0');
        expect(isFeatureEnabled('hints', true)).toBe(false);
    });

    test('the URL override persists after the query param is gone', () => {
        setUrl('?hints=1');
        isFeatureEnabled('hints', false);

        setUrl('');
        expect(isFeatureEnabled('hints', false)).toBe(true);
    });

    test('a later ?hints=0 flips a persisted on-override back off', () => {
        setUrl('?hints=1');
        isFeatureEnabled('hints', false);

        setUrl('?hints=0');
        expect(isFeatureEnabled('hints', false)).toBe(false);

        setUrl('');
        expect(isFeatureEnabled('hints', false)).toBe(false);
    });

    test('unrecognized values neither override nor persist', () => {
        setUrl('?hints=banana');
        expect(isFeatureEnabled('hints', false)).toBe(false);
        expect(localStorage.getItem('ff_hints')).toBeNull();
    });

    test('flags are isolated by name', () => {
        setUrl('?other=1');
        expect(isFeatureEnabled('hints', false)).toBe(false);
        expect(isFeatureEnabled('other', false)).toBe(true);
    });

    test('falls back to the build default when storage is unavailable', () => {
        const spy = jest.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
            throw new Error('denied');
        });
        try {
            expect(isFeatureEnabled('hints', true)).toBe(true);
            expect(isFeatureEnabled('hints', false)).toBe(false);
        } finally {
            spy.mockRestore();
        }
    });
});
