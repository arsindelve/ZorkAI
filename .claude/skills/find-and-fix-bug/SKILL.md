---
name: find-and-fix-bug
description: >-
  Find a genuine logic bug anywhere in this repo (C# engine/games, the
  React/TypeScript web clients, or the Lambda APIs), prove it with a failing
  test, then fix it and confirm green with no regressions. Use when the user
  asks to "find a bug", "find and fix a bug", "prove a bug with a test", or
  wants a TDD bug-hunt.
---

# Find a bug, prove it with a failing test, then fix it

A disciplined TDD bug hunt across this repository. The goal is **one real, unambiguous bug**
proven red→green — not a pile of speculative findings. This applies to *any* part of the repo,
not just the game engine.

## 0. Pick the surface and learn its test setup
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

## 1. Hunt for candidates
Read logic-heavy areas where "correct" is objective and provable without external services
(no AWS/AI/network/DB): parsing, string/list formatting, arithmetic and limits, comparisons and
boundary conditions, collection handling, state machines, and any place that re-implements or
overrides shared behavior. Skim recent diffs and TODO/FIXME/HACK markers.

For breadth, fan out read-only `Explore` agents over non-overlapping subsystems (engine,
a specific game, a web client, the Lambdas); have each report `file:line`, the wrong behavior,
the correct behavior, and a concrete reproducing input. Don't also run the same search yourself.

## 2. Vet ruthlessly (most important step)
Quick reads and subagents produce false positives. Before committing to a bug, **verify it
yourself** by reading the actual code and the types/contracts involved:
- Don't trust a precedence/associativity claim until you've confirmed how the language parses it
  (e.g. a C# `is` pattern with `or` is a *pattern combinator* bound to the `is`, not `||`).
- Check type/interface hierarchies and implicit conversions (a "specific" check may also match a
  broader or not-ready state — e.g. an off lamp still matching a light-source interface).
- Confirm the buggy code is actually reached — find the call site.
- Confirm no existing test already asserts the current behavior as intended.
Prefer the **purest** bug: a pure function with no state beats a deep stateful one.

## 3. Prove it with a failing test (red)
- Put the test next to related tests and match the file's style and framework.
- Deterministic only: no real randomness, clock, network, filesystem, AWS, or AI. For C#, use real
  `Repository` objects with `Repository.Reset()` in setup and mock `IRandomChooser`; for the web
  clients, stub the equivalent seams. Pick inputs with a single correct output.
- Clear `subject_scenario_expectedBehavior` name; assert the specific value/message (FluentAssertions
  on the C# side).
- Run just this test and confirm it fails **for the right reason** — read the assertion diff, not
  just a non-zero exit. A test that errors on setup hasn't proven the bug.

## 4. Fix it (green)
- Make the **minimal** change that corrects the behavior; match surrounding style and add a
  one-line comment explaining the trap so it isn't reintroduced.
- Prefer fixes that are behavior-preserving for the cases that already worked.

## 5. Verify, no regressions
- Re-run the new test → green.
- Run every suite that exercises the changed code (shared helpers are used widely across games and
  clients). Run each project/runner separately if it can't take multiple targets at once.
- Report the bug, the `file:line`, the proof (red→green), and the pass counts.

## Guardrails
- One bug, fully proven. Mention other candidates you found but did not touch.
- The proving test must run in the component's normal test command — no `[Explicit]`, integration,
  or cloud-gated tests.
- Don't invent a bug to satisfy the request. If nothing is clearly wrong, say so and list the
  strongest suspects with the open questions.
