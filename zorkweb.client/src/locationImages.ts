import {createLocationImageSet} from '@zork-ai/shared-types';

/**
 * The rooms of Zork I that have a picture drawn for them.
 *
 * Keyed by `GameResponse.locationKey` - the location class's own name - because the display
 * name is not an identity. `Clearing` and `ClearingBehindHouse` are both "Clearing" to the
 * player and only the first has the grating under the leaves; fifteen separate classes are
 * all called "Maze". Keying on the name would put one picture in all of them, and would
 * miss the rooms whose class is an abstract base nobody is ever standing in.
 *
 * Rooms that are the same place to a player share a picture on purpose - the maze, the two
 * Caves, both ends of White Cliffs Beach. Add art by uploading `<Name>.webp` under the
 * bucket's `locations/` prefix and listing the room here; a room whose file is not there
 * yet simply shows nothing.
 */
export const ZORK_LOCATION_IMAGE_BASE_URL = 'https://zorkai-assets.s3.amazonaws.com/locations/';

export const ZORK_LOCATION_IMAGES = createLocationImageSet(ZORK_LOCATION_IMAGE_BASE_URL, {
    Altar: 'Altar.webp',
    AragainFalls: 'AragainFalls.webp',
    Attic: 'Attic.webp',
    BatRoom: 'BatRoom.webp',
    BehindHouse: 'BehindHouse.webp',
    CanyonBottom: 'CanyonBottom.webp',
    CanyonView: 'CanyonView.webp',
    CaveNorth: 'Cave.webp',
    CaveSouth: 'Cave.webp',
    Cellar: 'Cellar.webp',
    Clearing: 'Clearing.webp',
    ClearingBehindHouse: 'ClearingBehindHouse.webp',
    ColdPassage: 'ColdPassage.webp',
    CyclopsRoom: 'CyclopsRoom.webp',
    Dam: 'Dam.webp',
    DamBase: 'DamBase.webp',
    DamLobby: 'DamLobby.webp',
    DampCave: 'DampCave.webp',
    DomeRoom: 'DomeRoom.webp',
    EastOfChasm: 'EastOfChasm.webp',
    EastWestPassage: 'EastWestPassage.webp',
    EgyptianRoom: 'EgyptianRoom.webp',
    EndOfRainbow: 'EndOfRainbow.webp',
    EngravingsCave: 'EngravingsCave.webp',
    EntranceToHades: 'EntranceToHades.webp',
    ForestPath: 'ForestPath.webp',
    Gallery: 'Gallery.webp',
    GratingRoom: 'GratingRoom.webp',
    InStream: 'Stream.webp',
    Kitchen: 'Kitchen.webp',
    LadderBottom: 'LadderBottom.webp',
    LadderTop: 'LadderTop.webp',
    LandOfTheDead: 'LandOfTheDead.webp',
    LivingRoom: 'LivingRoom.webp',
    MaintenanceRoom: 'MaintenanceRoom.webp',
    MazeEight: 'Maze.webp',
    MazeEleven: 'Maze.webp',
    MazeFifteen: 'Maze.webp',
    MazeFive: 'Maze.webp',
    MazeFour: 'Maze.webp',
    MazeFourteen: 'Maze.webp',
    MazeNine: 'Maze.webp',
    MazeOne: 'Maze.webp',
    MazeSeven: 'Maze.webp',
    MazeSix: 'Maze.webp',
    MazeTen: 'Maze.webp',
    MazeThirteen: 'Maze.webp',
    MazeThree: 'Maze.webp',
    MazeTwelve: 'Maze.webp',
    MazeTwo: 'Maze.webp',
    MineEntrance: 'MineEntrance.webp',
    MirrorRoomNorth: 'MirrorRoom.webp',
    MirrorRoomSouth: 'MirrorRoom.webp',
    NorthOfHouse: 'NorthOfHouse.webp',
    OnTheRainbow: 'OnTheRainbow.webp',
    ReservoirSouth: 'ReservoirSouth.webp',
    RockyLedge: 'RockyLedge.webp',
    RoundRoom: 'RoundRoom.webp',
    SandyBeach: 'SandyBeach.webp',
    SandyCave: 'SandyCave.webp',
    ShaftRoom: 'ShaftRoom.webp',
    Shore: 'Shore.webp',
    SlideRoom: 'SlideRoom.webp',
    SouthOfHouse: 'SouthOfHouse.webp',
    StreamView: 'StreamView.webp',
    Studio: 'Studio.webp',
    Temple: 'Temple.webp',
    TorchRoom: 'TorchRoom.webp',
    TreasureRoom: 'TreasureRoom.webp',
    TrollRoom: 'TrollRoom.webp',
    UpATree: 'UpATree.webp',
    WestOfHouse: 'WestOfHouse.webp',
    WhiteCliffsBeachNorth: 'WhiteCliffsBeach.webp',
    WhiteCliffsBeachSouth: 'WhiteCliffsBeach.webp',
});
