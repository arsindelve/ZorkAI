import React, {createContext, useContext, useState} from "react";
import {ISaveGameRequest} from "./model/SaveGameRequest.ts";
import DialogType from "./model/DialogType.ts";
import {ISavedGame} from "./model/SavedGame.ts";

// Define the context type
interface GameContextType {
    dialogToOpen: DialogType | undefined;
    setDialogToOpen: (dialog: DialogType | undefined) => void;

    restartGame: Boolean;
    setRestartGame: (restartGame: boolean) => void;

    saveGameRequest: ISaveGameRequest | undefined;
    setSaveGameRequest: (saveGameRequest: ISaveGameRequest | undefined) => void;

    restoreGameRequest: ISavedGame | undefined;
    setRestoreGameRequest: (restoreGameRequest: ISavedGame | undefined) => void;
}

// Create the context
const GameContext = createContext<GameContextType | undefined>(undefined);

// Custom hook for using the context
export const useGameContext = () => {
    const context = useContext(GameContext);
    if (!context) {
        throw new Error("useGameContext must be used within a GameProvider");
    }
    return context;
};

// Context provider
export const GameProvider: React.FC<{ children: React.ReactNode }> = ({children}) => {
    const [dialogToOpen, setDialogToOpen] = useState<DialogType | undefined>(undefined);
    const [restartGame, setRestartGame] = useState(false);
    const [saveGameRequest, setSaveGameRequest] = useState(undefined as ISaveGameRequest | undefined);
    const [restoreGameRequest, setRestoreGameRequest] = useState(undefined as ISavedGame | undefined);

    return (
        <GameContext.Provider
            value={{
                dialogToOpen,
                setDialogToOpen,
                restartGame,
                setRestartGame,
                saveGameRequest,
                setSaveGameRequest,
                restoreGameRequest,
                setRestoreGameRequest
            }}>
            {children}
        </GameContext.Provider>
    );
};