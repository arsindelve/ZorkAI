import React, { ChangeEvent, KeyboardEvent, RefObject, useEffect, useState } from 'react';
import { Box } from '@mui/material';

interface GameInputProps {
    playerInputElement: RefObject<HTMLInputElement | null>;
    isPending: boolean;
    playerInput: string;
    setInput: (value: string) => void;
    handleKeyDown: (e: KeyboardEvent<HTMLInputElement>) => void;
    /** Previously submitted commands, oldest first. Enables Up/Down recall. */
    commandHistory?: string[];
}

const GameInput: React.FC<GameInputProps> = ({
    playerInputElement,
    isPending,
    playerInput,
    setInput,
    handleKeyDown,
    commandHistory = []
}) => {
    const [isFocused, setIsFocused] = useState(false);
    // null = not navigating history (editing the live line); otherwise an index
    // into commandHistory.
    const [historyIndex, setHistoryIndex] = useState<number | null>(null);

    // Whenever a new command is recorded, drop back to the live line so the next
    // Up press starts from the most recent command.
    useEffect(() => {
        setHistoryIndex(null);
    }, [commandHistory.length]);

    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        setInput(e.target.value);
    };

    const onKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'ArrowUp') {
            if (commandHistory.length === 0) return;
            e.preventDefault();
            const idx = historyIndex === null
                ? commandHistory.length - 1
                : Math.max(0, historyIndex - 1);
            setHistoryIndex(idx);
            setInput(commandHistory[idx]);
            return;
        }
        if (e.key === 'ArrowDown') {
            if (historyIndex === null) return;
            e.preventDefault();
            const idx = historyIndex + 1;
            if (idx >= commandHistory.length) {
                setHistoryIndex(null);
                setInput('');
            } else {
                setHistoryIndex(idx);
                setInput(commandHistory[idx]);
            }
            return;
        }
        handleKeyDown(e);
    };

    return (
        <Box className="relative w-full px-2 py-1 sm:py-2">
            <div
                className={`
                    absolute top-0 left-0 w-full h-full
                    bg-gradient-to-r from-yellow-900/30 to-stone-700
                    transform ${isFocused ? 'scale-105 opacity-100' : 'scale-100 opacity-50'}
                    transition-all duration-300 rounded-md -z-10
                `}
            ></div>

            <div className="flex items-center w-full relative">
                <span
                    className="absolute left-3 sm:left-4 font-mono text-base sm:text-lg font-bold select-none pointer-events-none animate-blink text-[#c49a4c]"
                    aria-hidden="true"
                >
                    &gt;
                </span>
                <input
                    ref={playerInputElement}
                    readOnly={isPending}
                    className={`
                        w-full
                        pl-8 sm:pl-10 pr-4 sm:pr-5 py-2 sm:py-2.5
                        font-mono text-sm sm:text-base
                        text-[#f5e7c4]
                        bg-stone-800
                        border-2 ${isFocused ? 'border-yellow-700/70' : 'border-stone-600'}
                        rounded-md
                        shadow-md ${isFocused ? 'shadow-yellow-900/30' : ''}
                        focus:outline-none
                        placeholder-stone-500
                        transition-all duration-300
                    `}
                    value={playerInput}
                    placeholder="Type your command, then press enter/return..."
                    onChange={handleChange}
                    onKeyDown={onKeyDown}
                    onFocus={() => setIsFocused(true)}
                    onBlur={() => setIsFocused(false)}
                    data-testid="game-input"
                />
            </div>

            <p className="mt-1 ml-1 text-[11px] italic text-stone-400/70 select-none hidden sm:block">
                Tip: click any word above to add it to your command &middot; &uarr;/&darr; recalls past commands
            </p>
        </Box>
    );
};

export default GameInput;
