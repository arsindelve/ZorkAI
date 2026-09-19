/**
 * One turn of the hint conversation — what the player asked and what the narrator revealed.
 *
 * The /hint endpoint is STATELESS: the client owns the conversation and replays it with every
 * request (see HintServer.askForHint). The narrator reads it to see what it has already given away
 * and goes one step further next time, so the client must append each exchange after a successful
 * ask — and must NOT append failed/unavailable responses (isHint === false).
 */
export interface HintExchange {
    question: string;
    revealed: string;
}
