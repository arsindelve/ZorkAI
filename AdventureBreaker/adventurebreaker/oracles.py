"""Deterministic oracles -- the free, no-LLM bug detectors.

L0  contract     : transport/HTTP/JSON/score-bounds/leaked-internals checks on the raw result.
L1  consistency  : the narrator prose vs the structured state envelope.
L2  anchors      : known static strings + narrator-leak heuristics.

Each oracle returns OracleHit objects. They never raise; a noisy oracle is
tuned down to low/info, not removed, so the interactive driver (Claude) makes
the final call on whether a hit is a real bug.
"""
from __future__ import annotations

import re
from dataclasses import dataclass, field
from typing import List, Optional

from .client import ApiResult
from .models import GameResponse

SEVERITIES = ["info", "low", "medium", "high", "critical"]

# Substrings that should never appear in player-facing prose: leaked engine
# internals / .NET or Python stack traces.
INTERNALS_MARKERS = [
    "   at ", "System.", "GameEngine.", "Microsoft.AspNetCore",
    "StackTrace", "Traceback (most recent call last)", "NullReferenceException",
    "Unhandled exception", "InvalidOperationException", "at lambda_method",
]
# Markers that suggest the AI narrator broke character / leaked its prompt.
NARRATOR_LEAK_MARKERS = [
    "as an ai", "language model", "system prompt", "i cannot fulfill",
    "i'm sorry, but i can", "openai", "gpt-", "as a narrator, i",
    "you are a parser", "you are the narrator of", "i am an ai",
]


@dataclass
class OracleHit:
    oracle: str
    layer: str          # L0 / L1 / L2
    severity: str       # info..critical
    category: str       # contract / consistency / narrator / scoring / parser
    title: str
    detail: str
    evidence: str = ""

    def __post_init__(self):
        if self.severity not in SEVERITIES:
            self.severity = "medium"


_MOVE_WORDS = set(d.lower() for d in
                  ["n", "s", "e", "w", "ne", "nw", "sw", "se", "u", "d",
                   "up", "down", "in", "out", "north", "south", "east", "west",
                   "northeast", "northwest", "southeast", "southwest", "enter",
                   "exit", "go", "climb"])
_FAIL_MOVE = re.compile(r"can(?:'|no)t go that way|cannot go|you can't go", re.I)


def _clip(s: str, n: int = 280) -> str:
    s = (s or "").strip().replace("\n", " ")
    return s if len(s) <= n else s[:n] + " ..."


# --------------------------------------------------------------------------
# L0 -- contract
# --------------------------------------------------------------------------
def check_contract(result: ApiResult, max_score: int) -> List[OracleHit]:
    hits: List[OracleHit] = []
    if result.transport_error and result.status >= 500:
        hits.append(OracleHit("transport", "L0", "critical", "contract",
                              "Server error / transport failure",
                              f"status={result.status} {result.transport_error}",
                              _clip(result.raw_text)))
    elif result.transport_error:
        hits.append(OracleHit("transport", "L0", "high", "contract",
                              "Transport failure",
                              f"status={result.status} {result.transport_error}",
                              _clip(result.raw_text)))
    elif not result.ok:
        hits.append(OracleHit("http_status", "L0", "high", "contract",
                              f"Non-2xx HTTP status {result.status}",
                              f"status={result.status}", _clip(result.raw_text)))

    if result.raw_text and result.json is None and result.status < 500:
        hits.append(OracleHit("malformed_json", "L0", "high", "contract",
                              "Response body is not valid JSON object",
                              result.parse_error or "no json", _clip(result.raw_text)))

    text = result.raw_text or ""
    low = text.lower()
    for m in INTERNALS_MARKERS:
        if m.lower() in low:
            hits.append(OracleHit("leaked_internals", "L0", "critical", "contract",
                                  "Engine internals / stack trace leaked to player",
                                  f"marker={m!r}", _clip(text)))
            break

    g = result.game()
    if g is not None:
        if g.score < 0 or g.score > max_score:
            hits.append(OracleHit("score_bounds", "L0", "high", "scoring",
                                  f"Score {g.score} outside [0, {max_score}]",
                                  f"score={g.score} max={max_score}"))
        if g.moves < 0:
            hits.append(OracleHit("moves_negative", "L0", "medium", "contract",
                                  f"Negative move count {g.moves}", ""))
        if result.method == "POST" and (g.response or "").strip() == "":
            hits.append(OracleHit("empty_response", "L0", "medium", "narrator",
                                  "Empty narrator response to a command", ""))
        if (g.response or "").strip() in ("The narrator is silent.",
                                          "Your companion says nothing."):
            hits.append(OracleHit("silent_narrator", "L0", "medium", "narrator",
                                  "Narrator fell back to silent placeholder",
                                  "", _clip(g.response)))
    return hits


# --------------------------------------------------------------------------
# L1 -- prose vs state
# --------------------------------------------------------------------------
def check_consistency(command: str, prev: Optional[GameResponse],
                      cur: Optional[GameResponse]) -> List[OracleHit]:
    hits: List[OracleHit] = []
    if cur is None:
        return hits
    resp = cur.response or ""
    low = resp.lower()
    cmd = (command or "").strip().lower()
    first = cmd.split()[0] if cmd else ""

    if prev is not None:
        # Moves should never go backwards.
        if cur.moves < prev.moves:
            hits.append(OracleHit("moves_regress", "L1", "medium", "contract",
                                  "Move counter went backwards",
                                  f"{prev.moves} -> {cur.moves}"))
        # Score regressions are rare-but-legal (e.g. broken egg); flag low.
        if cur.score < prev.score:
            hits.append(OracleHit("score_regress", "L1", "low", "scoring",
                                  "Score decreased -- verify it's intended",
                                  f"{prev.score} -> {cur.score}", _clip(resp)))

        # Movement consistency.
        is_move = first in _MOVE_WORDS or cmd in _MOVE_WORDS
        moved = cur.location_name != prev.location_name
        if is_move and not moved and not _FAIL_MOVE.search(resp) and cur.moves > prev.moves:
            # Some legit non-moves (blocked by gate w/ custom msg) exist -> low.
            hits.append(OracleHit("move_no_change", "L1", "low", "consistency",
                                  "Movement command consumed a turn but location "
                                  "did not change and no 'can't go' message shown",
                                  f"loc={cur.location_name!r}", _clip(resp)))
        if moved and cur.previous_location_name not in (None, prev.location_name):
            hits.append(OracleHit("prevloc_mismatch", "L1", "low", "consistency",
                                  "previousLocationName disagrees with actual prior location",
                                  f"prev={prev.location_name!r} "
                                  f"reported={cur.previous_location_name!r}"))

        # Take / drop vs inventory size.
        inv_delta = len(cur.inventory) - len(prev.inventory)
        if re.search(r"\btaken\b", low) and inv_delta <= 0 and "drop" not in cmd:
            hits.append(OracleHit("take_no_inv", "L1", "medium", "consistency",
                                  "Prose says 'Taken' but inventory did not grow",
                                  f"inv {len(prev.inventory)} -> {len(cur.inventory)}",
                                  _clip(resp)))
        if re.search(r"\bdropped\b", low) and inv_delta >= 0 and first not in ("take", "get"):
            hits.append(OracleHit("drop_no_inv", "L1", "medium", "consistency",
                                  "Prose says 'Dropped' but inventory did not shrink",
                                  f"inv {len(prev.inventory)} -> {len(cur.inventory)}",
                                  _clip(resp)))
    return hits


# --------------------------------------------------------------------------
# L2 -- anchors + narrator-leak heuristics
# --------------------------------------------------------------------------
def check_anchors(result: ApiResult) -> List[OracleHit]:
    hits: List[OracleHit] = []
    g = result.game()
    if g is None:
        return hits
    low = (g.response or "").lower()
    for m in NARRATOR_LEAK_MARKERS:
        if m in low:
            hits.append(OracleHit("narrator_leak", "L2", "high", "narrator",
                                  "Narrator may have broken character / leaked meta",
                                  f"marker={m!r}", _clip(g.response)))
            break
    return hits


def run_all(command: str, result: ApiResult, prev: Optional[GameResponse],
            max_score: int) -> List[OracleHit]:
    hits = []
    hits += check_contract(result, max_score)
    hits += check_consistency(command, prev, result.game())
    hits += check_anchors(result)
    return hits
