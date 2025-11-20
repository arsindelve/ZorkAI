#!/bin/bash
# Update the Claude Code NuGet package cache with only project-specific packages
# Run this after adding/updating packages

set -e

CACHE_FILE=".claude/nuget-packages.tar.gz"
NUGET_DIR="${HOME}/.nuget/packages"
TEMP_DIR=$(mktemp -d)

echo "Updating Claude Code NuGet cache (project packages only)..."

cd "$(dirname "$0")/.." || exit 1

if [ ! -d "$NUGET_DIR" ]; then
    echo "Error: NuGet packages directory not found at $NUGET_DIR"
    exit 1
fi

# Get list of packages used by this project from all .csproj files
echo "Analyzing project dependencies..."
PACKAGES=$(find . -name "*.csproj" -exec grep -h '<PackageReference Include=' {} \; | \
           sed 's/.*Include="\([^"]*\)".*/\1/' | \
           tr '[:upper:]' '[:lower:]' | \
           sort -u)

if [ -z "$PACKAGES" ]; then
    echo "Warning: No packages found in .csproj files"
    echo "Make sure you've run 'dotnet restore' first"
fi

# Copy only the packages this project uses
mkdir -p "$TEMP_DIR/packages"
COPIED=0

for pkg in $PACKAGES; do
    if [ -d "$NUGET_DIR/$pkg" ]; then
        echo "  Including: $pkg"
        cp -r "$NUGET_DIR/$pkg" "$TEMP_DIR/packages/"
        COPIED=$((COPIED + 1))
    else
        echo "  Warning: Package not found in cache: $pkg"
    fi
done

if [ $COPIED -eq 0 ]; then
    echo "Error: No packages were copied. Run 'dotnet restore' first."
    rm -rf "$TEMP_DIR"
    exit 1
fi

# Create tarball from temp directory
tar -czf "$CACHE_FILE" -C "$TEMP_DIR/packages" .
rm -rf "$TEMP_DIR"

echo "✓ Cache updated successfully at $CACHE_FILE"
echo "  Packages included: $COPIED"
echo "  Size: $(du -h "$CACHE_FILE" | cut -f1)"
echo ""
echo "This file should be committed to git."
