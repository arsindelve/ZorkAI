# Running the Games Locally

How to launch and play all four game backends on one machine with **no AWS account and no OpenAI
key**. Written for an agent that needs to start a backend, drive it over HTTP, and recognize the
failure modes without re-deriving them.

---

## 0. Prerequisite: which branch you are on

**This does not work on `main`.** Self-hosted play arrives in a three-PR stack:

| PR | Branch | Adds |
|----|--------|------|
| #385 | `claude/issue-383-discussion-l2nisa` | Self-hosted console: local endpoint settings, `FileSessionRepository`, `LocalSecretsManager`, `LocalParseConversation`, `LocalCompanionChat` |
| #569 | `pr385-backends` | The four HTTP backends: `FileSavedGameRepository`, the `ServicesHelper` self-hosted branch, `SelfHostedMode` |
| #570 | `pr385-compose` | `docker/Dockerfile`, `compose.yaml`, `.dockerignore` |

Check before doing anything else:

```bash
test -f compose.yaml && echo "compose available" || echo "on main or an older branch — see the table above"
```

If `compose.yaml` is absent you are on a branch without #570. Either check out `pr385-compose`, or
use the [single-backend](#single-backend-without-docker) path, which needs only #569.

---

## 1. Quickstart

```bash
ollama pull qwen3:14b          # or any capable instruct model you already have
docker compose up -d --build   # from the repo root
```

First build takes a few minutes (SDK image pull, one restore, one publish pass). After that:

```bash
docker compose ps              # all four should read (healthy)
```

| Game | Endpoint | Container |
|------|----------|-----------|
| Zork One | `http://localhost:5100/ZorkOne` | `zorkai-zork1` |
| Planetfall | `http://localhost:5101/Planetfall` | `zorkai-planetfall` |
| Escape Room | `http://localhost:5102/EscapeRoom` | `zorkai-escaperoom` |
| Stationfall | `http://localhost:5103/Stationfall` | `zorkai-stationfall` |

One container per game, four separate images, one shared build stage. Ports bind to `127.0.0.1`
only. `restart: unless-stopped`, so they survive a reboot as long as Docker Desktop starts at login.

Play a turn:

```bash
curl -s -X POST http://localhost:5100/ZorkOne \
  -H "Content-Type: application/json" \
  -d '{"Input":"open mailbox","SessionId":"my-session","NoGeneratedResponses":true}'
```

Expect `Opening the small mailbox reveals a leaflet.`

---

## 2. The AI endpoint is a real dependency

These backends need no AWS, but they **do** need a reachable OpenAI-compatible server. This
engine's parser is load-bearing: it tries deterministic matching first and falls back to the model.

| With a reachable model | Without one |
|---|---|
| Everything | Movement (`north`, `enter pod`) and global commands (`look`, `inventory`, `score`, `diagnose`) only |

`open mailbox` needs the model. `look` does not. A backend with no model still starts, still answers,
and degrades in-fiction — which reads like a game bug rather than a configuration problem, so check
the endpoint before investigating gameplay.

Default endpoint is an Ollama **on the host** (`host.docker.internal:11434`), not a sidecar — the
model is usually already there. For a fully containerized model instead:

```bash
docker compose --profile bundled-ollama up -d
# then set OPENAI_BASE_URL=http://ollama:11434/v1 and pull a model into that container
```

---

## 3. HTTP API

Every game exposes the same surface under its own route prefix. Substitute `{Game}` with `ZorkOne`,
`Planetfall`, `EscapeRoom`, or `Stationfall`, and `{port}` from the table above.

### Play a turn

```
POST /{Game}
{"Input": "open mailbox", "SessionId": "my-session", "NoGeneratedResponses": false}
```

Returns `GameResponse`: `response`, `locationName`, `previousLocationName`, `moves`, `score`,
`time`, `inventory`, `actionsAvailableFromLocation`, `actionsAvailableFromInventory`, `exits`,
`lastMovementDirection`.

`NoGeneratedResponses: true` suppresses AI-generated *narration* for failed actions. It does **not**
disable the AI parser — a two-word interaction still calls the model.

### Resume a session

```
GET /{Game}?sessionId=my-session
```

Restores saved state and replays history (or `look` if there is none). Use this to read current
state without consuming a turn.

### Named saves

```
POST   /{Game}/saveGame        {"SessionId":"s","ClientId":"c","Name":"Before the troll","Id":null}
GET    /{Game}/saveGame?sessionId=c
POST   /{Game}/restoreGame     {"SessionId":"s","ClientId":"c","Id":"<save id>"}
DELETE /{Game}/saveGame/{id}?sessionId=c
```

Two traps here, both verified:

1. **`POST /saveGame` fails unless that `SessionId` has already played a turn.** It reads the live
   session first and returns **HTTP 500** with `Session had empty game data before attempting save
   game.` if there is none. Play one turn before saving.
2. **`GET /saveGame`'s `sessionId` parameter takes the `ClientId` you saved with**, not the
   `SessionId`. The storage layer keys saves by client. Same for the `DELETE` query parameter. The
   names disagree with each other; the behavior is as described.

`Id: null` creates a new save and returns its generated id. A non-null `Id` overwrites that slot.

### Hints (Planetfall only)

```
POST /Planetfall/hint
{"SessionId": "my-session", "Question": "how do I get past the door?", "History": null}
```

Returns `{"text": "..."}`. Consumes no turn and mutates no state. **Deliberately absent for the
other three games** — the hint engine needs a game-specific `IHintProvider`, and only Planetfall has
one (`Planetfall/Hints/PlanetfallHintProvider.cs`).

---

## 4. Environment variables

| Variable | Purpose |
|---|---|
| `ZORKAI_SELF_HOSTED` | **The opt-in.** `1`/`true`/`yes`/`on` swaps DynamoDB, Secrets Manager, CloudWatch and the Floyd Lambda for local equivalents. Anything else, including unset, means cloud mode. |
| `OPENAI_BASE_URL` | Endpoint for every AI client, e.g. `http://localhost:11434/v1`. |
| `ZORKAI_PROVIDER` | Preset instead of a URL: `openai`, `lmstudio` (1234), `ollama` (11434), `koboldcpp` (5001). An unknown value is a hard error. |
| `OPENAI_MODEL` | Overrides every hardcoded model id. Required for local servers. |
| `OPEN_AI_KEY` | Only for the real OpenAI API. Optional when a custom endpoint is set. |
| `ZORKAI_SAVE_DIR` | Root for sessions and saves. Default `~/.zorkai`; the containers set `/data`. |
| `ZORKAI_SYSTEM_PROMPT` | Overrides the built-in narrator prompt used in self-hosted mode. |
| `ASPNETCORE_URLS` | Bind address. Containers use `http://+:8080`. |
| `ASPNETCORE_ENVIRONMENT` | Must be `Production`. See traps. |

**`ZORKAI_SELF_HOSTED` is separate from `OPENAI_BASE_URL` on purpose.** Setting only the endpoint
leaves the backend on DynamoDB — the endpoint says where the *model* lives, which is a different
question from whether AWS exists, and `OPENAI_BASE_URL` is a generic name that gateways and proxies
also use. Inferring from it would mean a deployed Lambda pointed at an OpenAI gateway silently moved
every player's session onto ephemeral disk. See `GameEngine/SelfHostedMode.cs`.

The console is the exception: `--provider ollama` sets `ZORKAI_SELF_HOSTED` for you.

---

## 5. Where state lives

Under `ZORKAI_SAVE_DIR` (`/data` in the containers):

```
<root>/<table>/<sessionId>.session          base64 game state, rewritten every turn
<root>/<table>/<sessionId>.steps.log        append-only transcript
<root>/<table>/<clientId>/<id>.savedgame    one named save, JSON
```

Table names are per game: `zork1_session`, `zork1_savegame`, `planetfall_session`,
`escaperoom_session`, `stationfall_session`, … so one shared volume cannot collide.

Inspect or reset:

```bash
docker compose exec zork1 sh -c 'find /data -type f'
docker compose down -v          # deletes the volume and every session and save
```

---

## 6. Single backend without Docker

Needs #569 only. One backend at a time, on any port:

```bash
ZORKAI_SELF_HOSTED=true \
OPENAI_BASE_URL=http://localhost:11434/v1 \
OPENAI_MODEL=qwen3:14b \
ZORKAI_SAVE_DIR=/tmp/zorkai \
ASPNETCORE_URLS=http://localhost:5100 \
ASPNETCORE_ENVIRONMENT=Production \
dotnet run --project Lambda/src/Lambda --no-launch-profile
```

Project paths: `Lambda/src/Lambda`, `Planetfall-Lambda/src/Planetfall-Lambda`,
`EscapeRoom-Lambda/src/EscapeRoom-Lambda`, `Stationfall-Lambda/src/Stationfall-Lambda`.

There are no `launchSettings.json` files anywhere, so without `ASPNETCORE_URLS` every backend binds
`http://localhost:5000` and only one can run at a time.

---

## 7. Console (no HTTP at all)

```bash
dotnet run --project Console ZorkOne --provider ollama --model qwen3:14b
dotnet run --project Console Planetfall --endpoint http://gpu-box:8080/v1 --model my-model
```

Games: `ZorkOne`, `Planetfall`, `Stationfall`, `EscapeRoom` (case-insensitive, first argument).
Flags: `--provider`, `--endpoint`, `--model`. A bad game name or provider prints a message and exits
1. Saves land under `~/.zorkai`.

The console is the only host where companion conversation memory accumulates — see
`Planetfall/AI/LocalCompanionChat.cs`. In the HTTP backends every turn re-runs the characters' field
initializers, so each turn starts with an empty history. That matches the cloud path, which sends no
session or thread id either.

---

## 8. Traps

Ordered by how much time they cost.

1. **`OPENAI_MODEL` must name a model the server actually has.** Ollama returns `HTTP 404
   (not_found_error)` for an unknown model, and the engine turns that into its graceful in-fiction
   error — so a wrong model id looks exactly like a game bug. `curl -s
   http://localhost:11434/v1/models` lists what is available.
2. **`ASPNETCORE_ENVIRONMENT` must be `Production`.** Planetfall, EscapeRoom and Stationfall each
   register `GameEngineInitializer`, an `IHostedService` (singleton) that injects the scoped
   `IGameEngine`. Under `Development`, DI scope validation is on and the host throws at startup.
   Pre-existing, worked around rather than fixed.
3. **`ZORKAI_SELF_HOSTED` alone controls de-clouding.** Setting `OPENAI_BASE_URL` without it leaves
   you on DynamoDB and you will see `security token` / credential errors.
4. **`POST /saveGame` needs a session that has already played a turn** (§3).
5. **`GET`/`DELETE /saveGame` take the `ClientId` in a parameter named `sessionId`** (§3).
6. **The web clients cannot reach these ports.** `zorkweb.client/config.json` and
   `planetfallweb.client/config.json` hardcode `http://localhost:5000`, and CI rewrites them at
   deploy. Pointing a client at 5100/5101 means editing a tracked file. There are no web clients for
   Escape Room or Stationfall at all.
7. **`koboldcpp`'s preset port is 5001** — adjacent to the game ports. Prefer 5100–5103 for games.
8. **`Failed to determine the https port for redirect` is benign.** All four call
   `UseHttpsRedirection()` and the containers bind HTTP only, so the middleware logs once and passes
   the request through.
9. **Do not exclude `*.Tests` from the Docker build context.** `Planetfall.csproj` embeds
   `../Planetfall.Tests/Walkthrough/WalkthroughTestOne.cs` as a resource for the hint knowledge
   base. Excluding test projects breaks the Planetfall build. See `.dockerignore`.
10. **`dotnet test A B` fails with MSB1008.** Run each test project separately.
11. **On this machine, use `C:/Users/arsin/.dotnet/dotnet.exe`.** The machine-wide
    `C:\Program Files\dotnet` has only .NET 6/8 and no ASP.NET Core runtime; .NET 10 lives in the
    user profile.

---

## 9. Troubleshooting

| Symptom | Cause |
|---|---|
| `Something goes wrong, and for a moment the world seems to flicker.` | AI call failed. Wrong `OPENAI_MODEL`, or nothing at the endpoint. |
| `look` works, `open mailbox` does not | Model unreachable. Deterministic paths don't need it. |
| `The security token included in the request is invalid` | Still in cloud mode — `ZORKAI_SELF_HOSTED` not set. |
| `Cannot consume scoped service ... from singleton` at startup | `ASPNETCORE_ENVIRONMENT=Development`. Trap 2. |
| `Session had empty game data before attempting save game.` | Saving a session that never played a turn. |
| `GET /saveGame` returns `[]` after a successful save | Querying with `SessionId` instead of `ClientId`. |
| `Missing environment variable OPEN_AI_KEY` | No key and no custom endpoint. Set `OPENAI_BASE_URL` or `ZORKAI_PROVIDER`. |
| `Unknown ZORKAI_PROVIDER 'x'` | Typo. Valid: `openai`, `lmstudio`, `ollama`, `koboldcpp`. |
| Sessions vanish on restart | `ZORKAI_SAVE_DIR` not on a mounted volume. |
| Container `(unhealthy)` | `docker compose logs <service>`. Healthcheck is `GET /`. |
| Port already in use | Another backend on 5000, or `docker compose ps -a`. |

---

## 10. Smoke test

Confirms all four are up, gameplay works, and the AI path is wired.

```bash
for p in 5100 5101 5102 5103; do
  until curl -s -o /dev/null -m 2 "http://localhost:$p/"; do :; done
done

for pair in "5100 ZorkOne" "5101 Planetfall" "5102 EscapeRoom" "5103 Stationfall"; do
  set -- $pair
  printf "%-12s " "$2"
  curl -s -m 60 -X POST "http://localhost:$1/$2" \
    -H "Content-Type: application/json" \
    -d '{"Input":"look","SessionId":"smoke","NoGeneratedResponses":true}' \
  | grep -oE '"locationName":"[^"]*"'
done
```

Expected: `West Of House`, `Deck Nine`, `Reception`, `Deck Twelve`.

Then the AI path, and a check that nothing reached AWS:

```bash
curl -s -m 300 -X POST http://localhost:5100/ZorkOne \
  -H "Content-Type: application/json" \
  -d '{"Input":"open mailbox","SessionId":"smoke","NoGeneratedResponses":true}' \
| grep -oE '"response":"[^"]*"'
# -> "Opening the small mailbox reveals a leaflet.\n"

docker compose logs zork1 | grep -ciE "dynamodb|secretsmanager|cloudwatch|credential"
# -> 0
```

---

## See also

- `GameEngine/SelfHostedMode.cs` — the opt-in, and why it is not inferred
- `GameEngine/Web/ServicesHelper.cs` — cloud vs. local composition root
- `ZorkAI.OpenAI/OpenAIEndpointSettings.cs` — endpoint and model resolution
- `GameEngine/FileSessionRepository.cs`, `GameEngine/FileSavedGameRepository.cs` — on-disk state
- `README.md` — "Play It Offline (Self-Hosted AI)"
- `.claude/CLAUDE.md` — engine architecture, testing rules, port anti-patterns
