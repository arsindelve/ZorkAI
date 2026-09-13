import {createLocationImageSet} from '@zork-ai/shared-types';

/**
 * The rooms of Zork I that have a picture drawn for them.
 *
 * The keys are the location names exactly as the engine reports them (note "West Of
 * House" against "North of House" - the C# names are not consistently cased, which the
 * shared lookup normalises away). Art is added by uploading `<PascalCaseRoom>.webp` under
 * the bucket's `locations/` prefix and listing the room here; a room whose file is not
 * there yet simply shows nothing.
 *
 * Two names are deliberately shared by more than one room: both Cave classes are called
 * "Cave" and both Clearings "Clearing", so each pair shows the same picture. That is the
 * right outcome - to the player they are the same place by name.
 */
export const ZORK_LOCATION_IMAGE_BASE_URL = 'https://zorkai-assets.s3.amazonaws.com/locations/';

export const ZORK_LOCATION_IMAGES = createLocationImageSet(ZORK_LOCATION_IMAGE_BASE_URL, {
    Altar: 'Altar.webp',
    'Aragain Falls': 'AragainFalls.webp',
    Attic: 'Attic.webp',
    'Behind House': 'BehindHouse.webp',
    'Canyon Bottom': 'CanyonBottom.webp',
    'Canyon View': 'CanyonView.webp',
    Cave: 'Cave.webp',
    Cellar: 'Cellar.webp',
    Clearing: 'Clearing.webp',
    Dam: 'Dam.webp',
    'Dam Base': 'DamBase.webp',
    'Dam Lobby': 'DamLobby.webp',
    'Damp Cave': 'DampCave.webp',
    'Dome Room': 'DomeRoom.webp',
    'East of Chasm': 'EastOfChasm.webp',
    'East-West Passage': 'EastWestPassage.webp',
    'Egyptian Room': 'EgyptianRoom.webp',
    'End of Rainbow': 'EndOfRainbow.webp',
    'Engravings Cave': 'EngravingsCave.webp',
    'Entrance to Hades': 'EntranceToHades.webp',
    'Forest Path': 'ForestPath.webp',
    Gallery: 'Gallery.webp',
    Kitchen: 'Kitchen.webp',
    'Living Room': 'LivingRoom.webp',
    'Maintenance Room': 'MaintenanceRoom.webp',
    'Mirror Room': 'MirrorRoom.webp',
    'North of House': 'NorthOfHouse.webp',
    'On The Rainbow': 'OnTheRainbow.webp',
    'Reservoir South': 'ReservoirSouth.webp',
    'Rocky Ledge': 'RockyLedge.webp',
    'Round Room': 'RoundRoom.webp',
    'Sandy Beach': 'SandyBeach.webp',
    'Sandy Cave': 'SandyCave.webp',
    Shore: 'Shore.webp',
    'South of House': 'SouthOfHouse.webp',
    Studio: 'Studio.webp',
    Temple: 'Temple.webp',
    'The Troll Room': 'TrollRoom.webp',
    'Torch Room': 'TorchRoom.webp',
    'Up A Tree': 'UpATree.webp',
    'West Of House': 'WestOfHouse.webp',
    'White Cliffs Beach': 'WhiteCliffsBeach.webp',
});
