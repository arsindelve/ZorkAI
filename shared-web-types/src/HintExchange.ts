/**
 * One turn of the hint conversation — what the player asked and what the narrator revealed.
 *
 * The /hint endpoint is STATELESS: the client owns the conversation and replays it with every
 * request (see HintServer.askForHint). Progressive disclosure is paced from this history, so the
 * client must append each (question, revealed) pair after a successful ask — and must NOT append
 * failed/unavailable responses, or the pacing gets poisoned.
 */
export interface HintExchange {
    question: string;
    revealed: string;
}
