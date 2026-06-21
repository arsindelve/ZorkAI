"""Run state, transcript, and the findings ledger (Markdown + JSON).

A "run" is one game session. The server holds the authoritative game state
(keyed by sessionId in DynamoDB); locally we persist just enough to continue
across separate CLI invocations and to produce a reproducible record:

    runs/<name>/state.json       current pointer (session id, turn, last state, spine pos)
    runs/<name>/transcript.jsonl one line per turn (command + result + oracle hits)
    runs/<name>/findings.jsonl   confirmed findings (append-only)
    runs/<name>/findings.md      human-readable ledger, regenerated from jsonl
    runs/CURRENT                 name of the active run
"""
from __future__ import annotations

import json
import os
import time
from dataclasses import asdict, dataclass, field
from pathlib import Path
from typing import Any, Dict, List, Optional

from .oracles import OracleHit, SEVERITIES

RUNS_DIR = Path(os.environ.get("AB_RUNS_DIR", "runs"))


def _now() -> str:
    return time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime())


# --------------------------------------------------------------------------
@dataclass
class Run:
    name: str
    game: str
    target: str
    session_id: str
    client_id: str
    turn: int = 0
    spine_pos: int = 0
    last_response: Optional[Dict[str, Any]] = None  # raw envelope of last GameResponse
    created: str = field(default_factory=_now)

    @property
    def dir(self) -> Path:
        return RUNS_DIR / self.name

    # -- persistence ----------------------------------------------------
    def save(self) -> None:
        self.dir.mkdir(parents=True, exist_ok=True)
        (self.dir / "state.json").write_text(json.dumps(asdict(self), indent=2))

    @classmethod
    def load(cls, name: str) -> "Run":
        data = json.loads((RUNS_DIR / name / "state.json").read_text())
        return cls(**data)

    @classmethod
    def current_name(cls) -> Optional[str]:
        p = RUNS_DIR / "CURRENT"
        return p.read_text().strip() if p.exists() else None

    def make_current(self) -> None:
        RUNS_DIR.mkdir(parents=True, exist_ok=True)
        (RUNS_DIR / "CURRENT").write_text(self.name)

    # -- transcript -----------------------------------------------------
    def append_transcript(self, entry: Dict[str, Any]) -> None:
        self.dir.mkdir(parents=True, exist_ok=True)
        with (self.dir / "transcript.jsonl").open("a", encoding="utf-8") as f:
            f.write(json.dumps(entry) + "\n")

    # -- findings -------------------------------------------------------
    def add_finding(self, finding: Dict[str, Any]) -> None:
        self.dir.mkdir(parents=True, exist_ok=True)
        finding.setdefault("time", _now())
        finding.setdefault("game", self.game)
        finding.setdefault("target", self.target)
        finding.setdefault("run", self.name)
        finding.setdefault("turn", self.turn)
        finding.setdefault("session_id", self.session_id)
        with (self.dir / "findings.jsonl").open("a", encoding="utf-8") as f:
            f.write(json.dumps(finding) + "\n")
        self.render_findings_md()

    def load_findings(self) -> List[Dict[str, Any]]:
        p = self.dir / "findings.jsonl"
        if not p.exists():
            return []
        return [json.loads(l) for l in p.read_text().splitlines() if l.strip()]

    def render_findings_md(self) -> None:
        findings = self.load_findings()
        order = {s: i for i, s in enumerate(reversed(SEVERITIES))}
        findings.sort(key=lambda f: order.get(f.get("severity", "medium"), 99))
        lines = [
            f"# AdventureBreaker findings — {self.game} ({self.target})",
            "",
            f"_Run `{self.name}` · session `{self.session_id}` · generated {_now()}_",
            "",
            f"**{len(findings)}** finding(s).",
            "",
        ]
        by_sev: Dict[str, int] = {}
        for f in findings:
            by_sev[f.get("severity", "medium")] = by_sev.get(f.get("severity", "medium"), 0) + 1
        if by_sev:
            lines.append("| Severity | Count |")
            lines.append("|---|---|")
            for s in reversed(SEVERITIES):
                if s in by_sev:
                    lines.append(f"| {s} | {by_sev[s]} |")
            lines.append("")

        for i, f in enumerate(findings, 1):
            lines.append(f"## {i}. [{f.get('severity','?').upper()}] {f.get('title','(untitled)')}")
            lines.append("")
            meta = (f"- **category:** {f.get('category','?')}  ·  "
                    f"**layer/source:** {f.get('source','manual')}  ·  "
                    f"**turn:** {f.get('turn','?')}  ·  "
                    f"**location:** {f.get('location','?')}")
            lines.append(meta)
            if f.get("command") is not None:
                lines.append(f"- **command:** `{f.get('command')}`")
            if f.get("detail"):
                lines.append("")
                lines.append(f.get("detail"))
            if f.get("evidence"):
                lines.append("")
                lines.append("> " + str(f.get("evidence")).replace("\n", "\n> "))
            if f.get("repro"):
                lines.append("")
                lines.append("<details><summary>repro</summary>\n")
                lines.append("```")
                lines.append(str(f.get("repro")))
                lines.append("```")
                lines.append("</details>")
            lines.append("")
        (self.dir / "findings.md").write_text("\n".join(lines), encoding="utf-8")
