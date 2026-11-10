#!/usr/bin/env bash
# Start a game's backend (Kestrel) + web client (Vite) locally.
# Usage: start-stack.sh [zork|planetfall]   (default: zork)
set -euo pipefail

GAME="${1:-zork}"
case "$GAME" in
  zork)       BACKEND="Lambda/src/Lambda";                             CLIENT="zorkweb.client";       ROUTE="ZorkOne"   ;;
  planetfall) BACKEND="Planetfall-Lambda/src/Planetfall-Lambda";       CLIENT="planetfallweb.client"; ROUTE="Planetfall";;
  *) echo "usage: $0 [zork|planetfall]" >&2; exit 2 ;;
esac

ROOT="$(git -C "$(dirname "${BASH_SOURCE[0]}")" rev-parse --show-toplevel)"
LOGS=/tmp/zorkai-run
mkdir -p "$LOGS"

# Trap 2: node lives under nvm only; a non-login shell has no node on PATH.
NODE_BIN="$(ls -d "$HOME"/.nvm/versions/node/v22* 2>/dev/null | tail -1)/bin"
[ -x "$NODE_BIN/npm" ] || NODE_BIN="$(ls -d "$HOME"/.nvm/versions/node/* | tail -1)/bin"
export PATH="$NODE_BIN:$PATH"

# Trap 1: the DynamoDb session store needs the Delve profile from the repo-root .env.
# Export only well-formed KEY=VALUE lines - the file has historically carried stray junk
# lines that abort a plain `. .env` under `set -e`.
if [ -f "$ROOT/.env" ]; then
  while IFS= read -r line; do
    case "$line" in
      [A-Za-z_]*=*)
        key=${line%%=*}
        value=${line#*=}
        # Strip surrounding quotes. `. .env` removes them as shell syntax; exporting
        # the raw line does not, and a key exported as "sk-..." *with* the quotes is
        # sent verbatim to OpenAI, which rejects it with HTTP 401 invalid_api_key.
        case "$value" in
          \"*\") value=${value#\"}; value=${value%\"} ;;
          \'*\') value=${value#\'}; value=${value%\'} ;;
        esac
        export "$key=$value"
        ;;
    esac
  done < "$ROOT/.env"
else
  echo "WARNING: $ROOT/.env missing - session reads/writes will hit the wrong AWS account." >&2
fi
[ -n "${AWS_PROFILE:-}" ] || echo "WARNING: AWS_PROFILE unset - see SKILL.md Trap 1." >&2

"$(dirname "${BASH_SOURCE[0]}")/stop-stack.sh" >/dev/null 2>&1 || true

echo "Building $BACKEND ..."
dotnet build "$ROOT/$BACKEND" -v quiet --nologo

# Detach stdin too: a background child that keeps the caller's stdout/stdin open will hang any
# pipeline this script is invoked through (e.g. `start-stack.sh | tail`).
echo "Starting backend on :5000 ..."
( exec >"$LOGS/backend.log" 2>&1 </dev/null
  cd "$ROOT/$BACKEND" && ASPNETCORE_URLS=http://localhost:5000 ASPNETCORE_ENVIRONMENT=Development \
    nohup dotnet run --no-build & echo $! >"$LOGS/backend.pid" ) &

echo "Starting $CLIENT on :5173 ..."
( exec >"$LOGS/frontend.log" 2>&1 </dev/null
  cd "$ROOT/$CLIENT" && npm install --silent && nohup npm run dev & echo $! >"$LOGS/frontend.pid" ) &

wait_for () { # url, label
  for _ in $(seq 1 60); do
    curl -sf -o /dev/null -m 2 "$1" && { echo "  $2 up"; return 0; }
    sleep 1
  done
  echo "  $2 FAILED to start - see $LOGS" >&2; return 1
}
wait_for "http://localhost:5000/" backend
wait_for "http://localhost:5173/" frontend

# Drive it: a bound port proves nothing, so play two turns and watch the counter.
# `wait` is the probe verb - it burns a turn in every game, whereas `look` and `inventory`
# deliberately do not increment `moves`, which makes them useless as a liveness signal.
turn () { curl -s -m 30 -X POST "http://localhost:5000/$ROUTE" -H 'Content-Type: application/json' \
  -d "{\"input\":\"wait\",\"sessionId\":\"$1\"}" | sed -n 's/.*"moves":\([0-9]*\).*/\1/p'; }
SID="dev-$(date +%s)-$$"
curl -s -m 30 "http://localhost:5000/$ROUTE?sessionId=$SID" >/dev/null
M1=$(turn "$SID"); M2=$(turn "$SID")
if [ "${M1:-0}" -ge 1 ] && [ "${M2:-0}" -gt "${M1:-0}" ]; then
  echo "  smoke test OK (moves $M1 -> $M2; engine live and session persists across requests)"
else
  echo "  WARNING: moves did not advance ($M1 -> $M2) - check AWS creds, see SKILL.md Trap 1" >&2
fi

echo
echo "  app:     http://localhost:5173"
echo "  api:     http://localhost:5000/$ROUTE"
echo "  logs:    $LOGS/{backend,frontend}.log"
echo "  stop:    $(dirname "${BASH_SOURCE[0]}")/stop-stack.sh"
