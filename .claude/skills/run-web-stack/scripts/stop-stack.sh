#!/usr/bin/env bash
# Stop the local backend (:5000) and Vite dev server (:5173).
set -uo pipefail

LOGS=/tmp/zorkai-run

for port in 5000 5173; do
  pids=$(lsof -ti "tcp:$port" 2>/dev/null || true)
  if [ -n "$pids" ]; then
    echo "$pids" | xargs kill 2>/dev/null || true
    sleep 1
    pids=$(lsof -ti "tcp:$port" 2>/dev/null || true)
    [ -n "$pids" ] && echo "$pids" | xargs kill -9 2>/dev/null || true
    echo "stopped :$port"
  fi
done

rm -f "$LOGS"/backend.pid "$LOGS"/frontend.pid 2>/dev/null || true
