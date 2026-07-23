import {HintExchange} from "../HintExchange";
import {Mixpanel} from "./Mixpanel";

/**
 * Ask the game's stateless /hint endpoint for a hint.
 *
 * The endpoint is read-only — asking consumes no game turn and mutates no state. The prior
 * conversation is supplied BY THE CLIENT (that is what makes the endpoint stateless), so pass the
 * running history and, on success, append the new exchange yourself (HintPanel does this for you).
 *
 * Only the most recent exchanges are sent: disclosure pacing needs the recent thread, not the
 * whole transcript, and this bounds the token cost of long hint sessions.
 */
/** A reply from the hint endpoint. isHint=false marks a refusal/system message: show it, never record it. */
export interface HintAnswer {
    text: string;
    isHint?: boolean;
}

export async function askForHint(
    baseUrl: string,
    sessionId: string,
    question: string,
    history: HintExchange[]
): Promise<HintAnswer> {
    const started = Date.now();

    const response = await fetch(`${baseUrl}/hint`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        },
        body: JSON.stringify({
            sessionId,
            question,
            history: history.slice(-10)
        })
    });

    if (!response.ok)
        throw new Error(`Hint request failed: ${response.status}`);

    const data: HintAnswer = await response.json();

    Mixpanel.track("Asked For Hint", {
        question,
        historyLength: history.length,
        latencyMs: Date.now() - started,
        isHint: data.isHint !== false
    });

    return data;
}
