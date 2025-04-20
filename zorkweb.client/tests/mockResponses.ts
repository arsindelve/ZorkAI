/**
 * Mock API responses for the Zork game
 * These responses simulate the backend API for testing purposes
 */
export const mockResponses = {
    // Initial game response
    init: {
        score: 0,
        moves: 0,
        locationName: "West of House",
        response: "ZORK I: The Great Underground Empire\nCopyright (c) 1981, 1982, 1983 Infocom, Inc. All rights reserved.\nZORK is a registered trademark of Infocom, Inc.\nRevision 88 / Serial number 840726\n\nWest of House\nYou are standing in an open field west of a white house, with a boarded front door.\nThere is a small mailbox here.",
        inventory: []
    },
    // Response to "look around" command
    lookAround: {
        score: 0,
        moves: 1,
        locationName: "West of House",
        response: "West of House\nYou are standing in an open field west of a white house, with a boarded front door.\nThere is a small mailbox here.",
        inventory: []
    },
    // Response to "look" command
    look: {
        score: 0,
        moves: 1,
        locationName: "West of House",
        response: "West of House\nYou are standing in an open field west of a white house, with a boarded front door.\nThere is a small mailbox here.",
        inventory: []
    },
    // Response to "North" command
    north: {
        score: 0,
        moves: 1,
        locationName: "North of House",
        response: "North of House\nYou are facing the north side of a white house. There is no door here, and all the windows are boarded up. To the north a narrow path winds through the trees.",
        inventory: []
    }
};