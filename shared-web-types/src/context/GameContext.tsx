import React, {createContext, useContext, useEffect, useState} from 'react';
import {ISaveGameRequest} from '../SaveGameRequest';
import DialogType from '../DialogType';
import {ISavedGame} from '../SavedGame';
import {
    TranscriptFontSize,
    loadTranscriptFontSize,
    saveTranscriptFontSize,
} from '../preferences/TranscriptFontSize';
import {CompassSize, loadCompassSize, saveCompassSize} from '../preferences/CompassPreferences';
import {
    TranscriptFont,
    loadTranscriptFont,
    saveTranscriptFont,
} from '../preferences/TranscriptFont';
import {
    TranscriptLineSpacing,
    loadTranscriptLineSpacing,
    saveTranscriptLineSpacing,
} from '../preferences/TranscriptLineSpacing';
import {
    TranscriptMarker,
    loadTranscriptMarker,
    saveTranscriptMarker,
} from '../preferences/TranscriptMarker';
import {
    showCompassPreference,
    showVerbsMenuPreference,
    showCommandsMenuPreference,
    showLocationButtonPreference,
    showInventoryButtonPreference,
    animationsPreference,
} from '../preferences/toggles';
import {useBooleanPreference} from '../preferences/useBooleanPreference';

// Define the context type
interface GameContextType {
    dialogToOpen: DialogType | undefined;
    setDialogToOpen: (dialog: DialogType | undefined) => void;

    restartGame: boolean;
    setRestartGame: (restartGame: boolean) => void;

    saveGameRequest: ISaveGameRequest | undefined;
    setSaveGameRequest: (saveGameRequest: ISaveGameRequest | undefined) => void;

    restoreGameRequest: ISavedGame | undefined;
    setRestoreGameRequest: (restoreGameRequest: ISavedGame | undefined) => void;

    deleteGameRequest: ISavedGame | undefined;
    setDeleteGameRequest: (deleteGameRequest: ISavedGame | undefined) => void;

    copyGameTranscript: () => Promise<void>;
    setCopyGameTranscript: (copyFn: () => () => Promise<void>) => void;

    transcriptFontSize: TranscriptFontSize;
    setTranscriptFontSize: (size: TranscriptFontSize) => void;

    transcriptFont: TranscriptFont;
    setTranscriptFont: (font: TranscriptFont) => void;

    transcriptLineSpacing: TranscriptLineSpacing;
    setTranscriptLineSpacing: (spacing: TranscriptLineSpacing) => void;

    transcriptMarker: TranscriptMarker;
    setTranscriptMarker: (marker: TranscriptMarker) => void;

    showCompass: boolean;
    setShowCompass: (show: boolean) => void;

    compassSize: CompassSize;
    setCompassSize: (size: CompassSize) => void;

    showVerbsMenu: boolean;
    setShowVerbsMenu: (show: boolean) => void;

    showCommandsMenu: boolean;
    setShowCommandsMenu: (show: boolean) => void;

    showLocationButton: boolean;
    setShowLocationButton: (show: boolean) => void;

    showInventoryButton: boolean;
    setShowInventoryButton: (show: boolean) => void;

    animations: boolean;
    setAnimations: (enabled: boolean) => void;
}

// Create the context
const GameContext = createContext<GameContextType | undefined>(undefined);

// Custom hook for using the context
export const useGameContext = () => {
    const context = useContext(GameContext);
    if (!context) {
        throw new Error('useGameContext must be used within a GameProvider');
    }
    return context;
};

// Context provider
export const GameProvider: React.FC<{children: React.ReactNode}> = ({children}) => {
    const [dialogToOpen, setDialogToOpen] = useState<DialogType | undefined>(undefined);
    const [restartGame, setRestartGame] = useState(false);
    const [saveGameRequest, setSaveGameRequest] = useState(
        undefined as ISaveGameRequest | undefined,
    );
    const [restoreGameRequest, setRestoreGameRequest] = useState(
        undefined as ISavedGame | undefined,
    );
    const [deleteGameRequest, setDeleteGameRequest] = useState(undefined as ISavedGame | undefined);
    const [copyGameTranscript, setCopyGameTranscript] = useState<() => Promise<void>>(
        () => async () => {},
    );

    // Seeded from localStorage via the lazy initialiser so the very first paint is
    // already at the chosen size - reading it in an effect would flash the default.
    const [transcriptFontSize, setTranscriptFontSizeState] =
        useState<TranscriptFontSize>(loadTranscriptFontSize);

    const setTranscriptFontSize = (size: TranscriptFontSize) => {
        setTranscriptFontSizeState(size);
        saveTranscriptFontSize(size);
    };

    const [transcriptFont, setTranscriptFontState] =
        useState<TranscriptFont>(loadTranscriptFont);

    const setTranscriptFont = (font: TranscriptFont) => {
        setTranscriptFontState(font);
        saveTranscriptFont(font);
    };

    const [transcriptLineSpacing, setTranscriptLineSpacingState] =
        useState<TranscriptLineSpacing>(loadTranscriptLineSpacing);

    const setTranscriptLineSpacing = (spacing: TranscriptLineSpacing) => {
        setTranscriptLineSpacingState(spacing);
        saveTranscriptLineSpacing(spacing);
    };

    const [transcriptMarker, setTranscriptMarkerState] =
        useState<TranscriptMarker>(loadTranscriptMarker);

    const setTranscriptMarker = (marker: TranscriptMarker) => {
        setTranscriptMarkerState(marker);
        saveTranscriptMarker(marker);
    };

    const [animations, setAnimations] = useBooleanPreference(animationsPreference);

    // Flagged on the document element rather than an app wrapper so it also reaches
    // MUI dialogs and menus, which render through a portal outside the React tree.
    useEffect(() => {
        const root = document.documentElement;
        if (animations) {
            delete root.dataset.reduceMotion;
        } else {
            root.dataset.reduceMotion = 'true';
        }
    }, [animations]);

    const [showCompass, setShowCompass] = useBooleanPreference(showCompassPreference);
    const [showVerbsMenu, setShowVerbsMenu] = useBooleanPreference(showVerbsMenuPreference);
    const [showCommandsMenu, setShowCommandsMenu] = useBooleanPreference(showCommandsMenuPreference);
    const [showLocationButton, setShowLocationButton] =
        useBooleanPreference(showLocationButtonPreference);
    const [showInventoryButton, setShowInventoryButton] =
        useBooleanPreference(showInventoryButtonPreference);

    const [compassSize, setCompassSizeState] = useState<CompassSize>(loadCompassSize);

    const setCompassSize = (size: CompassSize) => {
        setCompassSizeState(size);
        saveCompassSize(size);
    };

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
                setRestoreGameRequest,
                deleteGameRequest,
                setDeleteGameRequest,
                copyGameTranscript,
                setCopyGameTranscript,
                transcriptFontSize,
                setTranscriptFontSize,
                transcriptFont,
                setTranscriptFont,
                transcriptLineSpacing,
                setTranscriptLineSpacing,
                transcriptMarker,
                setTranscriptMarker,
                showCompass,
                setShowCompass,
                compassSize,
                setCompassSize,
                showVerbsMenu,
                setShowVerbsMenu,
                showCommandsMenu,
                setShowCommandsMenu,
                showLocationButton,
                setShowLocationButton,
                showInventoryButton,
                setShowInventoryButton,
                animations,
                setAnimations,
            }}
        >
            {children}
        </GameContext.Provider>
    );
};
