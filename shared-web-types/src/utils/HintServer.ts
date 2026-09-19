import {HintExchange} from '../HintExchange';
import {Mixpanel} from './Mixpanel';

/**
 * Ask the game's stateless /hint endpoint for a hint.
 *
 * The endpoint is read-only — asking consumes no game turn and mutates no state. The prior
 * conversation is supplied BY THE CLIENT (that is what makes the endpoint stateless), so pass the
 * running history and, on success, append the new exchange yourself (HintPanel does this for you).
 *
 * Only the most recent exchanges are sent: pacing needs the recent thread, and this bounds the
 * token cost of long hint sessions.
 */
const HISTORY_WINDOW = 20;

/**
 * A reply from the hint endpoint. `isHint === false` marks a refusal/system message: show it, never
 * record it.
 */
export interface HintAnswer {
    text: string;
    isHint?: boolean;
}

/** Records an answer the way the endpoint wants it replayed. */
export function toExchange(question: string, answer: HintAnswer): HintExchange {
    return {question, revealed: answer.text};
}

/** How much of the recent game the narrator gets to read over the player's shoulder. */
const TRANSCRIPT_CHARS = 9000;

/**
 * The recent stretch of the game as plain text, from the transcript the page is showing (HTML
 * chunks). This is how the narrator knows what the player has actually seen — and so what would be
 * a spoiler.
 */
export function recentTranscript(gameText: string[]): string {
    const text = gameText
        .join('\n')
        .replace(/<br\s*\/?>(\s*)/gi, '\n')
        .replace(/<\/p>/gi, '\n')
        .replace(/<[^>]+>/g, '')
        .replace(/&nbsp;/g, ' ')
        .replace(/&gt;/g, '>')
        .replace(/&lt;/g, '<')
        .replace(/&amp;/g, '&')
        .replace(/\n{3,}/g, '\n\n')
        .trim();
    return text.length > TRANSCRIPT_CHARS ? text.slice(-TRANSCRIPT_CHARS) : text;
}

export async function askForHint(
    baseUrl: string,
    sessionId: string,
    question: string,
    history: HintExchange[],
    transcript?: string,
): Promise<HintAnswer> {
    const started = Date.now();

    const response = await fetch(`${baseUrl}/hint`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Accept: 'application/json',
        },
        body: JSON.stringify({
            sessionId,
            question,
            history: history.slice(-HISTORY_WINDOW),
            transcript,
        }),
    });

    if (!response.ok) throw new Error(`Hint request failed: ${response.status}`);

    const data: HintAnswer = await response.json();

    Mixpanel.track('Asked For Hint', {
        question,
        historyLength: history.length,
        latencyMs: Date.now() - started,
        isHint: data.isHint !== false,
    });

    return data;
}
