"""Data models for the game response envelope.

Unlike PlayZork's ZorkApiResponse (which keeps only response/location/moves/
score), we capture the *full* envelope -- inventory, exits, time, and the
actionsAvailableFrom* maps -- because those structured fields are the
ground truth the narrator prose is checked against (the L1 oracle).
"""
from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Dict, List, Optional

# Direction enum serialized as integers by the C# backend.
# Order from Model/Movement Direction enum:
#   N S E W NE NW SW SE In Out Up Down Unknown
DIRECTION_BY_INT = {
    0: "N", 1: "S", 2: "E", 3: "W", 4: "NE", 5: "NW", 6: "SW", 7: "SE",
    8: "In", 9: "Out", 10: "Up", 11: "Down", 12: "Unknown",
}
# Canonical directional inputs the move parser understands.
ALL_DIRECTIONS = ["N", "S", "E", "W", "NE", "NW", "SW", "SE", "Up", "Down", "In", "Out"]


def _dirs(raw: Any) -> List[str]:
    out = []
    for d in raw or []:
        if isinstance(d, int):
            out.append(DIRECTION_BY_INT.get(d, f"?{d}"))
        else:
            out.append(str(d))
    return out


@dataclass
class GameResponse:
    response: str = ""
    location_name: str = ""
    moves: int = 0
    score: int = 0
    time: int = 0
    previous_location_name: Optional[str] = None
    last_movement_direction: Optional[str] = None
    inventory: List[str] = field(default_factory=list)
    exits: List[str] = field(default_factory=list)
    actions_from_location: Dict[str, List[str]] = field(default_factory=dict)
    actions_from_inventory: Dict[str, List[str]] = field(default_factory=dict)
    raw: Dict[str, Any] = field(default_factory=dict)

    @classmethod
    def from_json(cls, j: Dict[str, Any]) -> "GameResponse":
        return cls(
            response=j.get("response") or "",
            location_name=j.get("locationName") or "",
            moves=j.get("moves") or 0,
            score=j.get("score") or 0,
            time=j.get("time") or 0,
            previous_location_name=j.get("previousLocationName"),
            last_movement_direction=j.get("lastMovementDirection"),
            inventory=list(j.get("inventory") or []),
            exits=_dirs(j.get("exits")),
            actions_from_location=dict(j.get("actionsAvailableFromLocation") or {}),
            actions_from_inventory=dict(j.get("actionsAvailableFromInventory") or {}),
            raw=j,
        )

    def short(self) -> str:
        return (f"[{self.location_name}] score={self.score} moves={self.moves} "
                f"time={self.time} exits={self.exits} inv={self.inventory}")
