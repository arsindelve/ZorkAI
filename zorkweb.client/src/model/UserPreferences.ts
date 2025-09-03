export enum Verbosity {
    SuperBrief = 0,
    Brief = 1,
    Verbose = 2
}

export interface IUserPreferences {
    verbosity: Verbosity;
    enableAiParsing: boolean;
    enableAiGeneration: boolean;
    fontSize: number;
    theme: string;
    showCompass: boolean;
    showInventoryPanel: boolean;
    autoSave: boolean;
    autoSaveIntervalMinutes: number;
    maxCommandHistory: number;
    enableSounds: boolean;
    soundVolume: number;
}

export const defaultUserPreferences: IUserPreferences = {
    verbosity: Verbosity.Brief,
    enableAiParsing: true,
    enableAiGeneration: true,
    fontSize: 16,
    theme: "dark",
    showCompass: true,
    showInventoryPanel: true,
    autoSave: false,
    autoSaveIntervalMinutes: 10,
    maxCommandHistory: 50,
    enableSounds: false,
    soundVolume: 50
};