"""AdventureBreaker CLI -- the surface the interactive driver (Claude) drives.

Every `play` auto-runs the deterministic oracles, prints a verdict, and records
a transcript line. The human/LLM driver supplies the adversarial commands and
calls `finding` when the narrator (or engine) does something wrong.

Examples:
    python3 -m adventurebreaker.harness new --game zork --target prod
    python3 -m adventurebreaker.harness spine-run --count 12      # advance via walkthrough
    python3 -m adventurebreaker.harness play examine the troll    # probe (narrator on)
    python3 -m adventurebreaker.harness quiet open mailbox        # engine only (no narrator)
    python3 -m adventurebreaker.harness finding --severity high --category narrator \
        --title "Narrator describes dead troll as alive" --command "examine troll" \
        --detail "After killing the troll, examining it says it 'brandishes its axe'."
"""
from __future__ import annotations

import argparse
import json
import random
import string
import sys
from dataclasses import asdict
from pathlib import Path

from . import config, oracles
from .client import ApiClient
from .ledger import Run, RUNS_DIR
from .models import GameResponse

SEV_ICON = {"info": "·", "low": "▽", "medium": "△", "high": "✖", "critical": "‼"}
SPINE_DIR = Path(__file__).resolve().parent / "spine"


# -- helpers ----------------------------------------------------------------
def _gen_id(n=15):
    return "".join(random.choices(string.ascii_letters + string.digits, k=n))


def _load_run(name=None) -> Run:
    name = name or Run.current_name()
    if not name:
        sys.exit("No current run. Create one with: new --game <zork|planetfall>")
    return Run.load(name)


def _client(run: Run) -> ApiClient:
    c = config.resolve(run.game, run.target)
    return ApiClient(c["base_url"], c["endpoint"])


def _spine(game: str) -> dict:
    fname = config.GAME_BACKENDS[game]["spine"]
    return json.loads((SPINE_DIR / fname).read_text())


def _print_state(g: GameResponse):
    print(f"--- {g.short()}")
    if g.actions_from_location:
        print(f"    actions@loc: {list(g.actions_from_location.keys())}")


def _print_hits(hits):
    if not hits:
        print("--- oracles: clean")
        return
    print(f"--- oracles: {len(hits)} hit(s)")
    for h in hits:
        icon = SEV_ICON.get(h.severity, "?")
        line = f"  {icon} [{h.severity}/{h.category}/{h.layer}] {h.title}"
        if h.detail:
            line += f" — {h.detail}"
        print(line)
        if h.evidence:
            print(f"      > {h.evidence}")


def _record_and_judge(run: Run, command: str, narrator: bool, result, *,
                      spine_expect=None):
    cfg = config.resolve(run.game, run.target)
    prev = GameResponse.from_json(run.last_response) if run.last_response else None
    hits = oracles.run_all(command, result, prev, cfg["max_score"])
    g = result.game()

    # Spine golden-transcript check (only meaningful on successful replay).
    spine_ok = None
    if spine_expect:
        text = (g.response if g else "") or result.raw_text or ""
        missing = [e for e in spine_expect if e not in text]
        spine_ok = not missing
        if missing:
            hits.append(oracles.OracleHit(
                "spine_divergence", "L2", "low", "consistency",
                "Spine expected-output not found (bug OR live-RNG divergence)",
                f"missing={missing!r}", oracles._clip(text)))

    run.turn += 1
    entry = {
        "turn": run.turn, "command": command, "narrator": narrator,
        "status": result.status, "latency_ms": result.latency_ms,
        "transport_error": result.transport_error,
        "location": g.location_name if g else None,
        "score": g.score if g else None, "moves": g.moves if g else None,
        "time": g.time if g else None,
        "exits": g.exits if g else None,
        "inventory": g.inventory if g else None,
        "response": g.response if g else result.raw_text,
        "spine_expect": spine_expect, "spine_ok": spine_ok,
        "hits": [asdict(h) for h in hits],
    }
    run.append_transcript(entry)
    if g is not None:
        run.last_response = g.raw
    run.save()
    return g, hits


# -- commands ---------------------------------------------------------------
def cmd_new(args):
    cfg = config.resolve(args.game, args.target)
    name = args.name or args.run or f"{args.game}-{args.target}-{_gen_id(4)}"
    run = Run(name=name, game=args.game, target=args.target,
              session_id=_gen_id(), client_id=_gen_id())
    run.make_current()
    client = _client(run)
    res = client.init(run.session_id)
    print(f"== new run '{name}' :: {cfg['name']} @ {cfg['target']} ({cfg['url']})")
    print(f"== session={run.session_id} client={run.client_id}")
    g = res.game()
    if g:
        print(g.response)
        _print_state(g)
        run.last_response = g.raw
    else:
        print("!! init failed:", res.status, res.transport_error, res.raw_text[:300])
    run.save()


def cmd_use(args):
    run = Run.load(args.run)
    run.make_current()
    print(f"current run -> {run.name} ({run.game}@{run.target}) turn={run.turn} "
          f"spine_pos={run.spine_pos}")


def cmd_state(args):
    run = _load_run(args.run)
    res = _client(run).init(run.session_id)
    g = res.game()
    if g:
        # GET returns the whole session log in `response`; show only its tail.
        tail = (g.response or "").strip().splitlines()[-4:]
        print("\n".join(tail))
        _print_state(g)
    else:
        print("!! state failed:", res.status, res.transport_error)


def _play(args, narrator: bool):
    run = _load_run(args.run)
    command = " ".join(args.command).strip()
    res = _client(run).play(run.session_id, command, narrator=narrator)
    tag = "narrator" if narrator else "quiet"
    print(f"> {command}    ({tag}) [HTTP {res.status}, {res.latency_ms}ms]")
    g, hits = _record_and_judge(run, command, narrator, res)
    print((g.response if g else res.raw_text) or "(no body)")
    if g:
        _print_state(g)
    _print_hits(hits)


def cmd_play(args):
    _play(args, narrator=True)


def cmd_quiet(args):
    _play(args, narrator=False)


def cmd_spine(args):
    run = _load_run(args.run)
    spine = _spine(run.game)
    steps = spine["steps"]
    start = args.frm if args.frm is not None else run.spine_pos
    end = min(start + args.count, len(steps))
    print(f"spine '{run.game}': pos={run.spine_pos}/{len(steps)}  showing {start}..{end-1}")
    for i in range(start, end):
        s = steps[i]
        flag = "" if s["http_replayable"] else "  [GOD-MODE: not replayable]"
        print(f"  {i:3d}  {s['cmd']!r:30}  expect={s['expect'][:1]}{flag}")


def cmd_spine_run(args):
    run = _load_run(args.run)
    spine = _spine(run.game)
    steps = spine["steps"]
    client = _client(run)
    narrator = args.narrator
    n = 0
    while n < args.count and run.spine_pos < len(steps):
        s = steps[run.spine_pos]
        if not s["http_replayable"]:
            print(f"  [{run.spine_pos}] SKIP god-mode step {s['cmd']!r}")
            run.spine_pos += 1
            run.save()
            continue
        res = client.play(run.session_id, s["cmd"], narrator=narrator)
        g, hits = _record_and_judge(run, s["cmd"], narrator, res,
                                    spine_expect=s["expect"])
        loc = g.location_name if g else "?"
        sc = g.score if g else "?"
        ok = "ok" if not any(h.oracle == "spine_divergence" for h in hits) else "DIVERGE"
        sev = max((h.severity for h in hits), default="clean",
                  key=lambda x: oracles.SEVERITIES.index(x) if x in oracles.SEVERITIES else 0)
        print(f"  [{run.spine_pos:3d}] {s['cmd']!r:28} -> [{loc}] score={sc} "
              f"{ok}  oracles:{sev}")
        for h in hits:
            if h.severity in ("high", "critical") or (args.verbose and h.severity != "low"):
                print(f"        {SEV_ICON.get(h.severity,'?')} [{h.severity}/{h.category}] "
                      f"{h.title} — {h.detail}")
        run.spine_pos += 1
        run.save()
        n += 1
    print(f"spine_pos now {run.spine_pos}/{len(steps)}")


def cmd_save(args):
    run = _load_run(args.run)
    res = _client(run).save(run.session_id, run.client_id, args.name)
    print(f"save '{args.name}': HTTP {res.status} {res.latency_ms}ms")
    print(res.raw_text[:400])


def cmd_saves(args):
    run = _load_run(args.run)
    res = _client(run).list_saves(run.client_id)
    print(f"saves: HTTP {res.status}")
    try:
        for s in (json.loads(res.raw_text) or []):
            print(f"  {s.get('id')}  {s.get('name')!r}  {s.get('date')}")
    except Exception:
        print(res.raw_text[:400])


def cmd_restore(args):
    run = _load_run(args.run)
    res = _client(run).restore(run.session_id, run.client_id, args.id)
    g = res.game()
    print(f"restore {args.id}: HTTP {res.status}")
    if g:
        run.last_response = g.raw
        run.save()
        print(g.response)
        _print_state(g)
    else:
        print(res.raw_text[:400])


def cmd_finding(args):
    run = _load_run(args.run)
    g = GameResponse.from_json(run.last_response) if run.last_response else None
    finding = {
        "severity": args.severity, "category": args.category,
        "title": args.title, "detail": args.detail or "",
        "evidence": args.evidence or "", "command": args.command,
        "source": "driver", "location": g.location_name if g else None,
        "repro": args.repro or "",
    }
    run.add_finding(finding)
    print(f"recorded [{args.severity}] {args.title}")
    print(f"ledger: {run.dir / 'findings.md'}")


def cmd_report(args):
    run = _load_run(args.run)
    findings = run.load_findings()
    print(f"run {run.name}: {len(findings)} finding(s), turn={run.turn}, "
          f"spine_pos={run.spine_pos}")
    by = {}
    for f in findings:
        by[f.get("severity")] = by.get(f.get("severity"), 0) + 1
    for s in reversed(oracles.SEVERITIES):
        if s in by:
            print(f"  {s}: {by[s]}")
    print(f"ledger: {run.dir / 'findings.md'}")
    print(f"transcript: {run.dir / 'transcript.jsonl'}")


def build_parser():
    p = argparse.ArgumentParser(prog="adventurebreaker")
    p.add_argument("--run", help="run name (defaults to current)")
    sub = p.add_subparsers(dest="cmd", required=True)

    sp = sub.add_parser("new"); sp.set_defaults(func=cmd_new)
    sp.add_argument("--game", required=True, choices=list(config.GAME_BACKENDS))
    sp.add_argument("--target", default="prod", choices=["prod", "local"])
    sp.add_argument("--name", default=None, help="run name (default: autogenerated)")

    sp = sub.add_parser("use"); sp.set_defaults(func=cmd_use)
    sp.add_argument("run")

    sub.add_parser("state").set_defaults(func=cmd_state)

    sp = sub.add_parser("play"); sp.set_defaults(func=cmd_play)
    sp.add_argument("command", nargs="+")
    sp = sub.add_parser("quiet"); sp.set_defaults(func=cmd_quiet)
    sp.add_argument("command", nargs="+")

    sp = sub.add_parser("spine"); sp.set_defaults(func=cmd_spine)
    sp.add_argument("--from", dest="frm", type=int, default=None)
    sp.add_argument("--count", type=int, default=15)

    sp = sub.add_parser("spine-run"); sp.set_defaults(func=cmd_spine_run)
    sp.add_argument("--count", type=int, default=10)
    sp.add_argument("--narrator", action="store_true", help="keep narrator on while advancing")
    sp.add_argument("--verbose", action="store_true")

    sp = sub.add_parser("save"); sp.set_defaults(func=cmd_save)
    sp.add_argument("name")
    sub.add_parser("saves").set_defaults(func=cmd_saves)
    sp = sub.add_parser("restore"); sp.set_defaults(func=cmd_restore)
    sp.add_argument("id")

    sp = sub.add_parser("finding"); sp.set_defaults(func=cmd_finding)
    sp.add_argument("--severity", required=True, choices=oracles.SEVERITIES)
    sp.add_argument("--category", required=True)
    sp.add_argument("--title", required=True)
    sp.add_argument("--detail", default="")
    sp.add_argument("--evidence", default="")
    sp.add_argument("--command", default=None)
    sp.add_argument("--repro", default="")

    sub.add_parser("report").set_defaults(func=cmd_report)
    return p


def main(argv=None):
    args = build_parser().parse_args(argv)
    args.func(args)


if __name__ == "__main__":
    main()
