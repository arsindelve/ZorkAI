/**
 * One turn of the hint conversation — what the player asked and what the narrator revealed.
 *
 * The /hint endpoint is STATELESS: the client owns the conversation and replays it with every
 * request (see HintServer.askForHint). Progressive disclosure is paced from this history, so the
 * client must append each exchange after a successful ask — echoing the `kind`, `topic` and `rung`
 * the endpoint returned, which is how the next "more" knows which hint ladder it is climbing (and
 * which rung comes next) or that it is continuing a lore answer — and must NOT append
 * failed/unavailable responses (isHint === false), or the pacing gets poisoned.
 */
export interface HintExchange {
    question: string;
    revealed: string;
    /** The puzzle this exchange hinted, as returned by the endpoint. Absent for lore and fallback answers. */
    topic?: string;
    /** Which rung of that puzzle's ladder was revealed (0-based), as returned by the endpoint. */
    rung?: number;
    /** What kind of answer this was: Progress, Mechanic, Lore, SoftLock or Grounded. */
    kind?: string;
}
