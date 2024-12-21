import React, {createContext, useContext, useState} from "react";

// Define the context type
interface GameContextType {
    dialogToOpen: string;
    setDialogToOpen: (dialog: string) => void;
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
    const [dialogToOpen, setDialogToOpen] = useState("");

    return (
        <GameContext.Provider value={{dialogToOpen, setDialogToOpen}}>
            {children}
        </GameContext.Provider>
    );
};