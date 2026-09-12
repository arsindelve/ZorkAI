/**
 * Minimal client-side feature flags — the first flag mechanism in the codebase, so it is
 * deliberately boring: no service, no SDK, just a build default plus a per-browser URL override.
 *
 * Resolution order:
 *   1. URL override, persisted per-browser: visiting once with `?<name>=1` forces the feature ON,
 *      `?<name>=0` forces it OFF (stored in localStorage as `ff_<name>` so it survives reloads
 *      until explicitly flipped back).
 *   2. Otherwise the build default supplied by the caller. For the web clients that is a
 *      `<name>_enabled` key in `config.json` — which the deploy workflow rewrites per environment,
 *      so a key absent from the deployed config means OFF in production while the committed
 *      config keeps it ON for local dev and the e2e suite.
 *
 * The URL override is what lets us ship a feature dark and still exercise the real production
 * pipeline: visit prod once with `?hints=1` and the panel is live for that browser only.
 */
export function isFeatureEnabled(name: string, buildDefault: boolean): boolean {
    if (typeof window === 'undefined') return buildDefault;
    const storageKey = `ff_${name}`;
    try {
        const fromUrl = new URLSearchParams(window.location.search).get(name);
        if (fromUrl === '1' || fromUrl === '0') {
            window.localStorage.setItem(storageKey, fromUrl);
        }
        const override = window.localStorage.getItem(storageKey);
        if (override === '1') return true;
        if (override === '0') return false;
    } catch {
        // Storage can be unavailable (private mode, hardened browsers) — the flag must never
        // take the app down, so fall through to the build default.
    }
    return buildDefault;
}
