import {recentTranscript} from '@zork-ai/shared-types';

describe('recentTranscript', () => {
    test('turns the on-screen HTML chunks into the plain text the player read', () => {
        const text = recentTranscript([
            '<p class="font-extrabold mt-3 mb-1">&gt; open desk</p>',
            'Opening the desk reveals a kitchen access card.<br/><br/>Floyd bounces &amp; giggles.',
        ]);

        expect(text).toBe(
            '> open desk\n\nOpening the desk reveals a kitchen access card.\n\nFloyd bounces & giggles.',
        );
    });

    test('keeps only the recent end of a long game', () => {
        const text = recentTranscript([`${'a'.repeat(20000)}`, 'THE LATEST THING']);

        expect(text.length).toBeLessThanOrEqual(9000);
        expect(text.endsWith('THE LATEST THING')).toBe(true);
    });
});
