export interface GameResponse {
    score: number;
    moves: number;
    time?: number;
    locationName: string;
    /**
     * The room's own identity - the location class's name. The display name is not one:
     * two rooms are called "Clearing", two "Cave", four "Forest".
     */
    locationKey?: string;
    response: string;
    inventory: string[];
    exits: string[];
    actionsAvailableFromInventory?: Record<string, string[]>;
    actionsAvailableFromLocation?: Record<string, string[]>;
    /**
     * True when the player is somewhere unlit. The server already withholds the room's
     * exits and action chips in that case (issue #238); the client withholds the room
     * artwork, which would otherwise show a picture of a room the player cannot see.
     */
    itIsDarkHere?: boolean;
}
