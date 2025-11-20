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

### nuget-packages.tar.gz (Optional, not checked in)
Cached NuGet packages to enable offline builds in Claude Code sessions.

**To create this file:**
```bash
# From the repository root, after running dotnet restore locally:
dotnet restore
tar -czf .claude/nuget-packages.tar.gz -C ~/.nuget/packages .
```

**Note:** This file is excluded from git via `.gitignore` because it's large (~100-500MB typically) and user-specific. Each developer working with Claude Code should create their own cache.

## Why This Setup?

Claude Code sessions run in sandboxed environments with restricted network access. By pre-caching NuGet packages:
- `dotnet build` and `dotnet test` can run locally during development
- Tests can be validated before pushing to CI/CD
- Development workflow is faster and more reliable

## Usage

1. **First time setup:**
   ```bash
   dotnet restore
   tar -czf .claude/nuget-packages.tar.gz -C ~/.nuget/packages .
   ```

2. **After dependency changes:**
   - Update packages locally
   - Recreate the tarball with the command above

3. **Start Claude Code session:**
   - SessionStart hook automatically extracts cached packages
   - `dotnet build` and `dotnet test` work immediately
