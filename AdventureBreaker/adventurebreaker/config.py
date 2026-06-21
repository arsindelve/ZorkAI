"""Backend + game configuration.

Mirrors PlayZork/VersionTwo/config.py's backend registry (the prod endpoints
are identical) but adds a per-target (prod vs local) split and the
max/target score facts used by the oracles.
"""
from __future__ import annotations

GAME_BACKENDS = {
    "zork": {
        "name": "Zork I",
        "endpoint": "/ZorkOne",
        "targets": {
            "prod": "https://bxqzfka0hc.execute-api.us-east-1.amazonaws.com/Prod",
            "local": "http://localhost:5000",
        },
        "max_score": 350,
        "target_score": 350,
        "spine": "zork1.json",
    },
    "planetfall": {
        "name": "Planetfall",
        "endpoint": "/Planetfall",
        "targets": {
            "prod": "https://6kvs9n5pj4.execute-api.us-east-1.amazonaws.com/Prod",
            "local": "http://localhost:5000",
        },
        "max_score": 80,
        "target_score": 80,
        "spine": "planetfall.json",
    },
}


def resolve(game: str, target: str = "prod"):
    if game not in GAME_BACKENDS:
        raise ValueError(f"Unknown game {game!r}; choose from {list(GAME_BACKENDS)}")
    cfg = GAME_BACKENDS[game]
    if target not in cfg["targets"]:
        raise ValueError(f"Unknown target {target!r}; choose from {list(cfg['targets'])}")
    base = cfg["targets"][target].rstrip("/")
    return {
        "game": game,
        "name": cfg["name"],
        "target": target,
        "base_url": base,
        "endpoint": cfg["endpoint"],
        "url": base + cfg["endpoint"],
        "max_score": cfg["max_score"],
        "target_score": cfg["target_score"],
        "spine": cfg["spine"],
    }
