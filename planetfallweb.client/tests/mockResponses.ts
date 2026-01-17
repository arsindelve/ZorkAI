/**
 * Mock API responses for the Zork game
 * These responses simulate the backend API for testing purposes
 */
export const mockResponses = {
    // Initial game response
    init: {
        score: 0,
        moves: 0,
        time: 4600,
        locationName: "West of House",
        response: "ZORK I: The Great Underground Empire\nCopyright (c) 1981, 1982, 1983 Infocom, Inc. All rights reserved.\nZORK is a registered trademark of Infocom, Inc.\nRevision 88 / Serial number 840726\n\nWest of House\nYou are standing in an open field west of a white house, with a boarded front door.\nThere is a small mailbox here.",
        inventory: []
    },
    // Response for restore command
    restore: {
        score: 0,
        moves: 0,
        time: 4600,
        locationName: "West of House",
        response: "<Restore>",
        inventory: []
    },
    // Response for save command
    save: {
        score: 0,
        moves: 0,
        time: 4600,
        locationName: "West of House",
        response: "<Save>",
        inventory: []
    },
    // Response for restart command
    restart: {
        score: 0,
        moves: 0,
        time: 4600,
        locationName: "West of House",
        response: "<Restart>",
        inventory: []
    },
    // Response to "look around" command
    lookAround: {
        score: 0,
        moves: 1,
        time: 4654,
        locationName: "West of House",
        response: "West of House\nYou are standing in an open field west of a white house, with a boarded front door.\nThere is a small mailbox here.",
        inventory: []
    },
    // Response to "look" command
    look: {
        score: 0,
        moves: 1,
        time: 4654,
        locationName: "West of House",
        response: "West of House\nYou are standing in an open field west of a white house, with a boarded front door.\nThere is a small mailbox here.",
        inventory: []
    },
    // Response to "North" command
    north: {
        score: 0,
        moves: 1,
        time: 4654,
        locationName: "North of House",
        response: "North of House\nYou are facing the north side of a white house. There is no door here, and all the windows are boarded up. To the north a narrow path winds through the trees.",
        inventory: []
    },
    // Response with items in inventory
    withInventory: {
        score: 10,
        moves: 5,
        time: 4870,
        locationName: "West of House",
        response: "You are carrying:\nA leaflet\nA brass lantern\nA sword",
        inventory: ["leaflet", "brass lantern", "sword"]
    },
    // Response with empty inventory
    emptyInventory: {
        score: 0,
        moves: 1,
        time: 4654,
        locationName: "West of House",
        response: "You are empty-handed.",
        inventory: []
    },
    // Mock saved games for testing the restore modal
    savedGames: [
        {
            name: "Test Game 1",
            id: "test-game-1",
            date: "2023-01-01T12:00:00Z"
        },
        {
            name: "Test Game 2",
            id: "test-game-2",
            date: "2023-01-02T14:30:00Z"
        },
        {
            name: "Test Game with a Very Long Name That Should Still Display Properly",
            id: "test-game-3",
            date: "2023-01-03T18:45:00Z"
        }
    ]
};
