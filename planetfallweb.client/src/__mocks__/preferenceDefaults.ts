/**
 * The preference slice of the game context, at its defaults.
 *
 * Any component that renders the Preferences dialog destructures every one of these,
 * so a test mocking useGameContext with a partial object crashes the moment a new
 * preference is added. Spread this into the mock instead, and adding a preference
 * means updating one file rather than every suite that happens to render <App />.
 */
export const preferenceDefaults = {
    transcriptFontSize: 'medium' as const,
    setTranscriptFontSize: jest.fn(),
    transcriptFont: 'terminal' as const,
    setTranscriptFont: jest.fn(),
    transcriptLineSpacing: 'normal' as const,
    setTranscriptLineSpacing: jest.fn(),
    transcriptMarker: 'none' as const,
    setTranscriptMarker: jest.fn(),
    animations: true,
    setAnimations: jest.fn(),
    showCompass: true,
    setShowCompass: jest.fn(),
    compassSize: 'medium' as const,
    setCompassSize: jest.fn(),
    showVerbsMenu: true,
    setShowVerbsMenu: jest.fn(),
    showCommandsMenu: true,
    setShowCommandsMenu: jest.fn(),
    showLocationButton: true,
    setShowLocationButton: jest.fn(),
    showInventoryButton: true,
    setShowInventoryButton: jest.fn(),
};
