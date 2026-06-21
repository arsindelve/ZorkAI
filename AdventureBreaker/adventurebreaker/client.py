"""Zero-dependency HTTP client for the ZorkAI backend.

Uses only the standard library so the harness runs against production with no
install step. Captures everything an oracle might care about: HTTP status,
latency, raw body, parse errors -- a leaked 500 / stack trace is itself a bug.
"""
from __future__ import annotations

import json
import time
import urllib.error
import urllib.parse
import urllib.request
from dataclasses import dataclass
from typing import Any, Dict, Optional

from .models import GameResponse


@dataclass
class ApiResult:
    ok: bool
    status: int
    latency_ms: int
    url: str
    method: str
    request_body: Optional[dict]
    raw_text: str
    json: Optional[dict]
    parse_error: Optional[str]
    transport_error: Optional[str]

    def game(self) -> Optional[GameResponse]:
        if self.json is None:
            return None
        try:
            return GameResponse.from_json(self.json)
        except Exception:
            return None


class ApiClient:
    def __init__(self, base_url: str, endpoint: str, timeout: int = 45):
        self.base_url = base_url.rstrip("/")
        self.endpoint = endpoint
        self.timeout = timeout

    # -- low level -------------------------------------------------------
    def _do(self, method: str, path: str, *, body: Optional[dict] = None,
            query: Optional[dict] = None) -> ApiResult:
        url = self.base_url + path
        if query:
            url += "?" + urllib.parse.urlencode(query)
        data = json.dumps(body).encode("utf-8") if body is not None else None
        req = urllib.request.Request(url, data=data, method=method)
        req.add_header("Accept", "application/json")
        if data is not None:
            req.add_header("Content-Type", "application/json")
        t0 = time.time()
        status, raw, transport_error = 0, "", None
        try:
            with urllib.request.urlopen(req, timeout=self.timeout) as resp:
                status = resp.getcode()
                raw = resp.read().decode("utf-8", errors="replace")
        except urllib.error.HTTPError as e:
            status = e.code
            try:
                raw = e.read().decode("utf-8", errors="replace")
            except Exception:
                raw = ""
            transport_error = f"HTTP {e.code} {e.reason}"
        except Exception as e:  # timeouts, connection errors, etc.
            transport_error = f"{type(e).__name__}: {e}"
        latency = int((time.time() - t0) * 1000)

        parsed, parse_error = None, None
        if raw:
            try:
                parsed = json.loads(raw)
            except Exception as e:
                parse_error = f"{type(e).__name__}: {e}"
        ok = (200 <= status < 300) and transport_error is None
        return ApiResult(ok=ok, status=status, latency_ms=latency, url=url,
                         method=method, request_body=body, raw_text=raw,
                         json=parsed if isinstance(parsed, dict) else None,
                         parse_error=parse_error, transport_error=transport_error)

    # -- game operations -------------------------------------------------
    def init(self, session_id: str) -> ApiResult:
        """GET ?sessionId=... -> intro text or restored state."""
        return self._do("GET", self.endpoint, query={"sessionId": session_id})

    def play(self, session_id: str, text: str, narrator: bool = True) -> ApiResult:
        """POST a player command. narrator=False sets NoGeneratedResponses."""
        body = {"input": text, "sessionId": session_id,
                "noGeneratedResponses": (not narrator)}
        return self._do("POST", self.endpoint, body=body)

    def save(self, session_id: str, client_id: str, name: str,
             save_id: Optional[str] = None) -> ApiResult:
        body = {"sessionId": session_id, "clientId": client_id,
                "name": name, "id": save_id}
        return self._do("POST", self.endpoint + "/saveGame", body=body)

    def list_saves(self, client_id: str) -> ApiResult:
        # NOTE: the web client passes the client id in the 'sessionId' query param.
        return self._do("GET", self.endpoint + "/saveGame",
                        query={"sessionId": client_id})

    def restore(self, session_id: str, client_id: str, save_id: str) -> ApiResult:
        body = {"sessionId": session_id, "clientId": client_id, "id": save_id}
        return self._do("POST", self.endpoint + "/restoreGame", body=body)

    def delete_save(self, save_id: str, session_id: str) -> ApiResult:
        return self._do("DELETE", self.endpoint + f"/saveGame/{save_id}",
                        query={"sessionId": session_id})
