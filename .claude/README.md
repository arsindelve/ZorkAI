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

### nuget-packages.tar.gz (Generated and committed to git)
Cached NuGet packages to enable offline builds in Claude Code sessions.

**Generate this file by running:** `.claude/update-cache.sh`

**Important:** This file contains ONLY packages used by this project (not entire ~/.nuget/packages), so it's small enough to commit (~10-50MB typically).

### update-cache.sh
Executable script to update the NuGet package cache. Run this after adding or updating packages, then commit the updated tarball.

## How It Works

### Project-Specific Cache Generation
When you run `.claude/update-cache.sh`:
1. Scans all .csproj files to find PackageReference entries
2. Copies only those specific packages from `~/.nuget/packages`
3. Creates a tarball at `.claude/nuget-packages.tar.gz`
4. Much smaller than full cache (suitable for git commit)

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
   git add .claude/nuget-packages.tar.gz
   git commit -m "Add NuGet package cache for Claude Code"
   ```

2. **Start Claude Code session:**
   - SessionStart hook automatically extracts cached packages from git
   - `dotnet build` and `dotnet test` work immediately

## Ongoing Usage

**Update and commit the cache when dependencies change:**

After adding packages:
```bash
dotnet add package Foo
.claude/update-cache.sh
git add .claude/nuget-packages.tar.gz
git commit -m "Update NuGet cache for new package"
```

After updating packages:
```bash
dotnet restore
.claude/update-cache.sh
git add .claude/nuget-packages.tar.gz
git commit -m "Update NuGet cache"
```

**Important:** The tarball must be committed to git so Claude Code sessions can access it. You only need to update when package dependencies change, not for every build.
