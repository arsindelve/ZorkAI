---
name: find-and-fix-bug
description: >-
  Find a genuine logic bug anywhere in this repo (C# engine/games, the
  React/TypeScript web clients, or the Lambda APIs), prove it with a failing
  test, fix it, and open a PR. Always starts from a fresh branch off the latest
  main and creates the PR automatically when done. Use when the user asks to
  "find a bug", "find and fix a bug", "prove a bug with a test", or wants a
  TDD bug-hunt.
---

# Find a bug, prove it with a failing test, fix it, open a PR

A disciplined TDD bug hunt across this repository, wrapped in git automation. The goal is **one
real, unambiguous bug** proven red→green — not a pile of speculative findings. This applies to
*any* part of the repo, not just the game engine.

## 1. Start from a fresh branch off the latest main
Always begin clean so the work lands in its own PR:
```bash
git checkout main
git pull --ff-only
git checkout -b fix/<short-slug>     # name it after the bug once you know it
```
If you start before knowing the bug, branch on a placeholder and rename, or just branch after
step 3 — but never commit the fix onto `main` or onto an unrelated branch. Leave pre-existing
unrelated working-tree changes unstaged; only stage the files for this fix.

## 2. Pick the surface and learn its test setup
This repo has several testable components, each with its own runner. Choose the one that matches
the code you're hunting in, and confirm the exact command to run a single test and a full suite:
- **C# engine & games** — `dotnet test <Project>` (e.g. `UnitTests`, `ZorkOne.Tests`,
  `Planetfall.Tests`, `Lambda.Tests`); single test via `--filter "FullyQualifiedName~<Name>"`.
  Note `dotnet test A B` errors with MSB1008 — run each project separately.
- **Web clients** (`zorkweb.client`, `planetfallweb.client`, `shared-web-types`) — the
  React/TypeScript projects; use their `package.json` scripts (unit runner and/or Playwright).
- **Lambda APIs** — their own `*.Tests` projects via `dotnet test`.

Match the surrounding test files' framework, layout, and assertion style. Skim `CLAUDE.md`
"Development Best Practices" for the C# conventions.

## 3. Hunt for candidates
Read logic-heavy areas where "correct" is objective and provable without external services
(no AWS/AI/network/DB): parsing, string/list formatting, arithmetic and limits, comparisons and
boundary conditions, collection handling, state machines, and any place that re-implements or
overrides shared behavior. Skim recent diffs and TODO/FIXME/HACK markers.

For breadth, fan out read-only `Explore` agents over non-overlapping subsystems (engine,
a specific game, a web client, the Lambdas); have each report `file:line`, the wrong behavior,
the correct behavior, and a concrete reproducing input. Don't also run the same search yourself.

## 4. Vet ruthlessly (most important step)
Quick reads and subagents produce false positives. Before committing to a bug, **verify it
yourself** by reading the actual code and the types/contracts involved:
- Don't trust a precedence/associativity claim until you've confirmed how the language parses it
  (e.g. a C# `is` pattern with `or` is a *pattern combinator* bound to the `is`, not `||`).
- Check type/interface hierarchies and implicit conversions (a "specific" check may also match a
  broader or not-ready state — e.g. an off lamp still matching a light-source interface).
- Confirm the buggy code is actually reached — find the call site.
- Confirm no existing test already asserts the current behavior as intended.
Prefer the **purest** bug: a pure function with no state beats a deep stateful one.

## 5. Prove it with a failing test (red)
- Put the test next to related tests and match the file's style and framework.
- Deterministic only: no real randomness, clock, network, filesystem, AWS, or AI. For C#, use real
  `Repository` objects with `Repository.Reset()` in setup and mock `IRandomChooser`; for the web
  clients, stub the equivalent seams. Pick inputs with a single correct output.
- Clear `subject_scenario_expectedBehavior` name; assert the specific value/message (FluentAssertions
  on the C# side).
- Run just this test and confirm it fails **for the right reason** — read the assertion diff, not
  just a non-zero exit. A test that errors on setup hasn't proven the bug.

## 6. Fix it (green)
- Make the **minimal** change that corrects the behavior; match surrounding style and add a
  one-line comment explaining the trap so it isn't reintroduced.
- Prefer fixes that are behavior-preserving for the cases that already worked.

## 7. Verify, no regressions
- Re-run the new test → green.
- Run every suite that exercises the changed code (shared helpers are used widely across games and
  clients). Run each project/runner separately if it can't take multiple targets at once.

## 8. Open a PR automatically
When the fix is green and regression-free, commit only this fix's files, push the branch, and
create the PR — don't wait to be asked:
```bash
git add <the fix + test files>
git commit -m "<concise summary>

<why it was wrong and what changed>

Co-Authored-By: Claude Opus 4.8 (1M context) <noreply@anthropic.com>"
git push -u origin fix/<short-slug>
gh pr create --base main --fill   # or pass --title/--body with the bug, the proof, and pass counts
```
End PR bodies with the "Generated with Claude Code" line. Then report the bug, the `file:line`,
the proof (red→green), the pass counts, and the PR URL. Mention other candidates you found but
did not touch.

## Guardrails
- One bug, fully proven. Never commit to `main`; always work on the fresh branch from step 1.
- The proving test must run in the component's normal test command — no `[Explicit]`, integration,
  or cloud-gated tests.
- Don't invent a bug to satisfy the request. If nothing is clearly wrong, say so on the branch (or
  skip the PR) and list the strongest suspects with the open questions.
