---
name: run-web-stack
description: >-
  Launch a game's web stack locally — the ASP.NET backend plus its React/Vite
  client — and drive it to confirm a change really works in the app. Covers
  Zork One, Planetfall, Stationfall and Escape Room. Use when asked to run,
  start, serve, or screenshot the web app, to "see it working", or to iterate
  on web-client UI. Not for tests; use `dotnet test` / `npm run test:unit`.
---

# Run a game's web stack locally

Two processes: a **backend** (the game's Lambda project, hosted by Kestrel) and a **frontend**
(that game's Vite dev server). They talk over plain HTTP on fixed ports.

## Do not use `ZorkWeb.Server`

`ZorkWeb.Server/` looks like the web backend and the root `CLAUDE.md` still describes it as
"ASP.NET Core server for web clients". **It is dead code.** It is not in `Zork.sln` and does not
compile — it references a deleted `Azure/Azure.csproj` and fails with:

```
error CS0246: The type or namespace name 'Azure' could not be found
warning MSB9008: The referenced project ../Azure/Azure.csproj does not exist
```

The real backend for every game is its Lambda project, each of which ships a `LocalEntryPoint.cs`
precisely so it can run under Kestrel locally.

## Pick the stack

| Game | Backend project | Client | API route |
|---|---|---|---|
| Zork One | `Lambda/src/Lambda` | `zorkweb.client` | `/ZorkOne` |
| Planetfall | `Planetfall-Lambda/src/Planetfall-Lambda` | `planetfallweb.client` | `/Planetfall` |
| Stationfall | `Stationfall-Lambda/src/Stationfall-Lambda` | — | `/Stationfall` |
| Escape Room | `EscapeRoom-Lambda/src/EscapeRoom-Lambda` | — | `/EscapeRoom` |

**Only one stack at a time.** Every client hardcodes `http://localhost:5000/<Game>` in its
`config.json`, and both Vite configs hardcode port 5173. Running Zork and Planetfall together
requires editing those files; don't attempt it casually.

## Launch

Use the helper, which handles the two environment traps below:

```bash
.claude/skills/run-web-stack/scripts/start-stack.sh zork       # or: planetfall
```

It starts both processes in the background, waits for each port, and tails logs to
`/tmp/zorkai-run/`. Stop everything with `scripts/stop-stack.sh`.

To run them by hand instead:

```bash
# Backend — from the Lambda project directory
cd Lambda/src/Lambda
set -a; . "$(git rev-parse --show-toplevel)/.env"; set +a
ASPNETCORE_URLS=http://localhost:5000 ASPNETCORE_ENVIRONMENT=Development dotnet run

# Frontend — in a second shell
export PATH="$HOME/.nvm/versions/node/v22.23.1/bin:$PATH"
cd zorkweb.client && npm install && npm run dev
```

### Trap 1: the backend needs the repo-root `.env`

`ISessionRepository` is a **DynamoDb** repository — it hits real AWS on every turn, even locally.
The root `.env` sets `AWS_PROFILE=delve` and `AWS_REGION=us-east-1`, pointing at the Delve account
(`576431164672`) where the tables live. Without it the SDK falls back to the machine's default
profile, which is a *different* account, and turns fail on session read/write.

Sanity check: `AWS_PROFILE=delve aws sts get-caller-identity` should print account `576431164672`.

### Trap 2: node is not on the non-interactive PATH

`node` and `npm` are installed via nvm only, so a non-login shell gets `command not found`.
Prepend the bin directory (v22.23.1 is verified working; v20 and v24 are also installed):

```bash
export PATH="$HOME/.nvm/versions/node/v22.23.1/bin:$PATH"
```

## Drive it — launching is not running

A server that binds a port proves nothing about the game. Hit the real endpoint and read the body:

```bash
SID="dev-$(date +%s)"
curl -s "http://localhost:5000/ZorkOne?sessionId=$SID"                      # intro text
curl -s -X POST http://localhost:5000/ZorkOne -H 'Content-Type: application/json' \
  -d "{\"input\":\"wait\",\"sessionId\":\"$SID\"}"                          # -> "moves":1
curl -s -X POST http://localhost:5000/ZorkOne -H 'Content-Type: application/json' \
  -d "{\"input\":\"wait\",\"sessionId\":\"$SID\"}"                          # -> "moves":2
```

`moves` climbing across two separate requests is the real pass condition: it proves the engine
ran *and* that the session round-tripped through DynamoDb. If it stays `0`, session persistence
is broken — that is Trap 1, not a game bug.

**Use `wait` as the probe verb.** `look` and `inventory` deliberately do not burn a turn, so they
leave `moves` at `0` and look like a failure. `wait` counts in every game; `open mailbox` works
too but is Zork-only.

For UI work, open `http://localhost:5173` in the browser (see the `claude-in-chrome` skill),
dismiss the welcome modal, then type a command into the input and press Return. **Look at the
screenshot**: a rendered room description with MOVES incrementing is the pass condition. Two
quirks when driving it:

- Clicking the input in the same batch that dismisses the modal drops the keystrokes. Close the
  modal, *then* click the field in a separate call.
- The transcript pane keeps history across reloads because the session lives server-side, so a
  stale-looking transcript after a refresh is correct behavior, not a caching bug.

## Iterating on the client

Vite HMR is live — edits under `zorkweb.client/src` hot-reload with no restart, and the browser
console should show only `[vite] connecting… / connected.` Restart the dev server only when you
touch `vite.config.ts`, `config.json`, or `package.json`.

Backend C# changes need a restart: stop the process and re-run it (`dotnet run` again), or use
`dotnet watch` from the Lambda project directory.
