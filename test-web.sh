#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
FAILED=0

run_tests() {
    local project="$1"
    local dir="$ROOT_DIR/$project"

    echo "=== $project: Jest unit tests ==="
    if (cd "$dir" && npx jest); then
        echo "PASS: $project Jest"
    else
        echo "FAIL: $project Jest"
        FAILED=1
    fi

    echo ""
    echo "=== $project: Playwright tests ==="
    if (cd "$dir" && npx playwright test); then
        echo "PASS: $project Playwright"
    else
        echo "FAIL: $project Playwright"
        FAILED=1
    fi

    echo ""
}

run_tests "zorkweb.client"
run_tests "planetfallweb.client"

if [ "$FAILED" -ne 0 ]; then
    echo "=== Some tests failed ==="
    exit 1
else
    echo "=== All tests passed ==="
fi
