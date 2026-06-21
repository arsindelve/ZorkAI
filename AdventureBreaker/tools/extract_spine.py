#!/usr/bin/env python3
"""
Extract a "spine" (ordered walkthrough) from the ZorkAI NUnit [TestCase] fixtures.

The ZorkAI walkthroughs live as C# attributes of the shape:

    [TestCase("command", null|"SetupMethod", "expected substr", "expected substr", ...)]

We parse the *first* string literal as the player command, the *second*
positional argument as the god-mode setup method (or null), and every
remaining string literal as an expected-output substring. The expected
substrings turn the walkthrough into a golden transcript we can assert
against (a free L2 regression oracle).

Steps whose setup is non-null require reflection-based god-mode state
manipulation that cannot be replayed over the production HTTP API; we keep
them but flag `http_replayable = (setup is None)`.

This script does NOT copy any game source -- it extracts only the sequence
of *player inputs* (and the test's own expected-output fragments), which are
facts about how the game is played, not game implementation.

Usage:
    python3 tools/extract_spine.py \
        --src /home/user/ZorkAI/ZorkOne.Tests/Walkthrough/WalkthroughTestOne.cs \
        --game zork --out adventurebreaker/spine/zork1.json
"""
from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

TESTCASE_RE = re.compile(r"\[\s*TestCase\s*\(")


def _read_args(text: str, start: int):
    """Given index just after '[TestCase(', return (args_str, end_index)
    capturing through the matching ')'. String- and paren-aware."""
    depth = 1
    i = start
    in_str = False
    escape = False
    while i < len(text):
        c = text[i]
        if in_str:
            if escape:
                escape = False
            elif c == "\\":
                escape = True
            elif c == '"':
                in_str = False
        else:
            if c == '"':
                in_str = True
            elif c == "(":
                depth += 1
            elif c == ")":
                depth -= 1
                if depth == 0:
                    return text[start:i], i + 1
        i += 1
    raise ValueError("Unterminated TestCase(...) starting at %d" % start)


def _split_top_level(args: str):
    """Split an argument list on top-level commas (string/paren aware)."""
    out, buf, depth = [], [], 0
    in_str = False
    escape = False
    for c in args:
        if in_str:
            buf.append(c)
            if escape:
                escape = False
            elif c == "\\":
                escape = True
            elif c == '"':
                in_str = False
            continue
        if c == '"':
            in_str = True
            buf.append(c)
        elif c in "([{":
            depth += 1
            buf.append(c)
        elif c in ")]}":
            depth -= 1
            buf.append(c)
        elif c == "," and depth == 0:
            out.append("".join(buf).strip())
            buf = []
        else:
            buf.append(c)
    if buf:
        out.append("".join(buf).strip())
    return out


def _unquote(token: str):
    """Return the decoded string if token is a C# string literal, else None."""
    token = token.strip()
    if len(token) >= 2 and token[0] == '"' and token[-1] == '"':
        body = token[1:-1]
        # Decode the handful of escapes that show up in these fixtures.
        body = (body.replace('\\"', '"').replace("\\\\", "\\")
                .replace("\\n", "\n").replace("\\t", "\t").replace("\\r", "\r"))
        return body
    return None


def extract(text: str):
    steps = []
    for m in TESTCASE_RE.finditer(text):
        args_str, _ = _read_args(text, m.end())
        parts = _split_top_level(args_str)
        if not parts:
            continue
        command = _unquote(parts[0])
        if command is None:
            # First arg is not a string literal -> not a command-style TestCase.
            continue
        setup = _unquote(parts[1]) if len(parts) > 1 else None
        # null / nameof(...) / identifiers all decode to None via _unquote.
        expected = [s for s in (_unquote(p) for p in parts[2:]) if s is not None]
        steps.append({
            "cmd": command,
            "setup": setup,
            "expect": expected,
            "http_replayable": setup is None,
        })
    return steps


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--src", required=True, action="append",
                    help="Walkthrough .cs file (repeatable; concatenated in order)")
    ap.add_argument("--game", required=True)
    ap.add_argument("--out", required=True)
    args = ap.parse_args()

    all_steps = []
    for src in args.src:
        text = Path(src).read_text(encoding="utf-8")
        all_steps.extend(extract(text))

    out = {
        "game": args.game,
        "source": args.src,
        "count": len(all_steps),
        "replayable_count": sum(1 for s in all_steps if s["http_replayable"]),
        "steps": all_steps,
    }
    Path(args.out).parent.mkdir(parents=True, exist_ok=True)
    Path(args.out).write_text(json.dumps(out, indent=2), encoding="utf-8")
    print(f"[{args.game}] {out['count']} steps "
          f"({out['replayable_count']} http-replayable) -> {args.out}")
    # Show a small sample for sanity.
    for s in all_steps[:5]:
        print("   ", repr(s["cmd"]), "| expect:", s["expect"][:1])


if __name__ == "__main__":
    main()
