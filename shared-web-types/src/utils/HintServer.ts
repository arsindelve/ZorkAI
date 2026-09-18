import {HintExchange} from '../HintExchange';
import {Mixpanel} from './Mixpanel';

/**
 * Ask the game's stateless /hint endpoint for a hint.
 *
 * The endpoint is read-only — asking consumes no game turn and mutates no state. The prior
 * conversation is supplied BY THE CLIENT (that is what makes the endpoint stateless), so pass the
 * running history and, on success, append the new exchange yourself (HintPanel does this for you).
 *
 * Only the most recent exchanges are sent: disclosure pacing needs the recent thread, not the
 * whole transcript, and this bounds the token cost of long hint sessions. The window is wide enough
 * that a puzzle's earlier rungs survive a detour through other questions; drop below it and a
 * ladder restarts from its first rung.
 */
const HISTORY_WINDOW = 20;

/**
 * A reply from the hint endpoint. `isHint === false` marks a refusal/system message: show it, never
 * record it. `kind`, `topic` and `rung` must be echoed back on the recorded exchange (see
 * HintExchange); `totalRungs` lets the panel show "hint 2 of 3"; `softLock` (None, Warning,
 * BestEndingOnly, Hard) says whether a caveat rides on the text.
 */
export interface HintAnswer {
    text: string;
    isHint?: boolean;
    kind?: string;
    topic?: string | null;
    rung?: number;
    totalRungs?: number;
    softLock?: string;
}

/** Records an answer the way the endpoint wants it replayed: with the kind/topic/rung it returned. */
export function toExchange(question: string, answer: HintAnswer): HintExchange {
    const exchange: HintExchange = {question, revealed: answer.text};
    if (answer.kind !== undefined) exchange.kind = answer.kind;
    if (answer.topic !== undefined && answer.topic !== null) exchange.topic = answer.topic;
    if (answer.rung !== undefined) exchange.rung = answer.rung;
    return exchange;
}

export async function askForHint(
    baseUrl: string,
    sessionId: string,
    question: string,
    history: HintExchange[],
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
        }),
    });

    if (!response.ok) throw new Error(`Hint request failed: ${response.status}`);

    const data: HintAnswer = await response.json();

    Mixpanel.track('Asked For Hint', {
        question,
        historyLength: history.length,
        latencyMs: Date.now() - started,
        isHint: data.isHint !== false,
        kind: data.kind,
        topic: data.topic,
        rung: data.rung,
    });

    return data;
}
