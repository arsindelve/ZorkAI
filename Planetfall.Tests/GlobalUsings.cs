// Global using directives

// The survival and time mechanics these tests exercise live in the shared StellarPatrol library
// rather than in Planetfall itself. Importing it globally keeps the ~17 test files that name
// HungerLevel/TiredLevel unchanged.
global using StellarPatrol;

// SceneryItem moved from GameEngine.Location to Model.Location so the game interface can
// declare global scenery; Model cannot reference GameEngine.
global using Model.Location;
