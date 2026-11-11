import {SessionHandler} from '../../../shared-web-types/src/utils/SessionHandler';

describe('Unopened Worlds session handoff', () => {
    beforeEach(() => {
        localStorage.clear();
        window.history.replaceState(null, '', '/');
    });

    test('adopts a valid guest session from the URL fragment and removes it from the address bar', () => {
        window.history.replaceState(null, '', '/#session=AbCdEf123456789');

        const [sessionId, firstTime] = new SessionHandler().getSessionId();

        expect(sessionId).toBe('AbCdEf123456789');
        expect(firstTime).toBe(false);
        expect(localStorage.getItem('SessionId')).toBe('AbCdEf123456789');
        expect(window.location.hash).toBe('');
    });

    test('ignores malformed handoff fragments', () => {
        window.history.replaceState(null, '', '/#session=not-valid!');

        const [sessionId, firstTime] = new SessionHandler().getSessionId();

        expect(sessionId).toHaveLength(15);
        expect(sessionId).not.toBe('not-valid!');
        expect(firstTime).toBe(true);
    });
});
