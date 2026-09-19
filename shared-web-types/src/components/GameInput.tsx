import {ChangeEvent, KeyboardEvent, RefObject, useEffect, useState} from 'react';
import {Box} from '@mui/material';

export interface GameInputProps {
    playerInputElement: RefObject<HTMLInputElement | null>;
    isPending: boolean;
    playerInput: string;
    setInput: (value: string) => void;
    handleKeyDown: (event: KeyboardEvent<HTMLInputElement>) => void;
    commandHistory?: string[];
}

export default function GameInput({
    playerInputElement,
    isPending,
    playerInput,
    setInput,
    handleKeyDown,
    commandHistory = [],
}: GameInputProps) {
    const [isFocused, setIsFocused] = useState(false);
    const [historyIndex, setHistoryIndex] = useState<number | null>(null);

    useEffect(() => setHistoryIndex(null), [commandHistory.length]);

    const handleChange = (event: ChangeEvent<HTMLInputElement>) => setInput(event.target.value);

    const onKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
        if (event.key === 'ArrowUp') {
            if (commandHistory.length === 0) return;
            event.preventDefault();
            const index =
                historyIndex === null ? commandHistory.length - 1 : Math.max(0, historyIndex - 1);
            setHistoryIndex(index);
            setInput(commandHistory[index]);
            return;
        }
        if (event.key === 'ArrowDown') {
            if (historyIndex === null) return;
            event.preventDefault();
            const index = historyIndex + 1;
            if (index >= commandHistory.length) {
                setHistoryIndex(null);
                setInput('');
            } else {
                setHistoryIndex(index);
                setInput(commandHistory[index]);
            }
            return;
        }
        handleKeyDown(event);
    };

    return (
        <Box className="relative w-full px-2 py-1 sm:py-2">
            <div
                className={`absolute top-0 left-0 w-full h-full transform ${isFocused ? 'scale-105 opacity-100' : 'scale-100 opacity-50'} transition-all duration-300 rounded-md -z-10`}
                style={{
                    background:
                        'var(--game-input-glow, linear-gradient(90deg, rgba(120, 80, 20, .3), #44403c))',
                }}
            />
            <div className="flex items-center w-full relative">
                <span
                    className="absolute left-3 sm:left-4 font-mono text-base sm:text-lg font-bold select-none pointer-events-none animate-blink"
                    style={{color: 'var(--game-input-accent, #c49a4c)'}}
                    aria-hidden="true"
                >
                    &gt;
                </span>
                <input
                    ref={playerInputElement}
                    readOnly={isPending}
                    className="w-full pl-8 sm:pl-10 pr-4 sm:pr-5 py-2 sm:py-2.5 font-mono text-sm sm:text-base rounded-md shadow-md focus:outline-none transition-all duration-300"
                    style={{
                        background: 'var(--game-input-bg, #292524)',
                        color: 'var(--game-input-text, #f5e7c4)',
                        border: `2px solid ${isFocused ? 'var(--game-input-focus-border, rgba(161, 98, 7, .7))' : 'var(--game-input-border, #57534e)'}`,
                        boxShadow: isFocused
                            ? 'var(--game-input-focus-shadow, 0 0 20px rgba(120, 80, 20, .3))'
                            : 'none',
                    }}
                    value={playerInput}
                    placeholder="Type your command, then press enter/return..."
                    onChange={handleChange}
                    onKeyDown={onKeyDown}
                    onFocus={() => setIsFocused(true)}
                    onBlur={() => setIsFocused(false)}
                    data-testid="game-input"
                />
            </div>
            <p
                className="mt-1 ml-1 text-[11px] italic select-none hidden sm:block"
                style={{color: 'var(--game-input-muted, rgba(168, 162, 158, .7))'}}
            >
                Tip: click any word above to add it to your command &middot; &uarr;/&darr; recalls
                past commands
            </p>
        </Box>
    );
}
