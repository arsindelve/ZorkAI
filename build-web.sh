#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "=== Installing shared-web-types ==="
cd "$ROOT_DIR/shared-web-types"
npm ci

echo ""
echo "=== Installing and building zorkweb.client ==="
cd "$ROOT_DIR/zorkweb.client"
npm ci
npm run build

echo ""
echo "=== Installing and building planetfallweb.client ==="
cd "$ROOT_DIR/planetfallweb.client"
npm ci
npm run build

echo ""
echo "=== All web clients built successfully ==="
