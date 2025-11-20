# Claude Code Configuration

This directory contains configuration for Claude Code sessions.

## Files

### SessionStart (Hook)
Executable bash script that runs when a Claude Code session starts. It:
1. Checks for cached NuGet packages at `.claude/nuget-packages.tar.gz`
2. Extracts the cache to `~/.nuget/packages` if present
3. Runs `dotnet restore` to prepare the build environment

### settings.local.json
Permissions configuration allowing specific bash commands to run without user approval.

### nuget-packages.tar.gz (Manually generated, not checked in)
Cached NuGet packages to enable offline builds in Claude Code sessions.

**Generate this file by running:** `.claude/update-cache.sh`

**Note:** This file is excluded from git via `.gitignore` because it's large (~100-500MB typically) and user-specific.

### update-cache.sh
Executable script to manually update the NuGet package cache. Run this after adding or updating packages.

## How It Works

### Manual Cache Generation
When you run `.claude/update-cache.sh`:
1. Creates a tarball of `~/.nuget/packages`
2. Saves it to `.claude/nuget-packages.tar.gz`
3. Takes 30-60 seconds depending on cache size

### Claude Code Session Startup
When a Claude Code session starts:
1. SessionStart hook checks for the tarball
2. Extracts it to the sandbox's `~/.nuget/packages`
3. Runs `dotnet restore` (fast, uses extracted cache)
4. `dotnet build` and `dotnet test` now work locally

## Why This Setup?

Claude Code sessions run in sandboxed environments with restricted network access. By pre-caching NuGet packages:
- `dotnet build` and `dotnet test` can run locally during development
- Tests can be validated before pushing to CI/CD
- Development workflow is faster and more reliable

## First Time Setup

1. **Restore packages and generate the cache:**
   ```bash
   dotnet restore
   .claude/update-cache.sh
   ```
   The script will create `.claude/nuget-packages.tar.gz` (takes ~30-60 seconds)

2. **Start Claude Code session:**
   - SessionStart hook automatically extracts cached packages
   - `dotnet build` and `dotnet test` work immediately

## Ongoing Usage

**Update the cache when needed:**
- After adding new packages: `dotnet add package Foo && .claude/update-cache.sh`
- After updating packages: `dotnet restore && .claude/update-cache.sh`
- After removing packages: `dotnet remove package Bar && .claude/update-cache.sh`

You don't need to update the cache for every build - only when your package dependencies change.
