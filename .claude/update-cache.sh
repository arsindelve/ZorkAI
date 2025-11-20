#!/bin/bash
# Manually update the Claude Code NuGet package cache
# Run this after adding/updating packages

set -e

CACHE_FILE=".claude/nuget-packages.tar.gz"
NUGET_DIR="${HOME}/.nuget/packages"

echo "Updating Claude Code NuGet cache..."
echo "This may take 30-60 seconds..."

cd "$(dirname "$0")/.." || exit 1

if [ ! -d "$NUGET_DIR" ]; then
    echo "Error: NuGet packages directory not found at $NUGET_DIR"
    exit 1
fi

tar -czf "$CACHE_FILE" -C "$NUGET_DIR" .

echo "✓ Cache updated successfully at $CACHE_FILE"
echo "Size: $(du -h "$CACHE_FILE" | cut -f1)"
