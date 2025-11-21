import React, { ChangeEvent, KeyboardEvent, RefObject, useState } from 'react';
import { Box } from '@mui/material';

interface GameInputProps {
    playerInputElement: RefObject<HTMLInputElement>;
    isPending: boolean;
    playerInput: string;
    setInput: (value: string) => void;
    handleKeyDown: (e: KeyboardEvent<HTMLInputElement>) => void;
}

const GameInput: React.FC<GameInputProps> = ({
    playerInputElement,
    isPending,
    playerInput,
    setInput,
    handleKeyDown
}) => {
    const [isFocused, setIsFocused] = useState(false);


    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        setInput(e.target.value);
    };

    return (
        <Box className="relative w-full sm:w-3/4 md:w-2/3 lg:max-w-xl px-2 py-1 sm:py-2 flex-shrink">
            <div
                className={`
                    absolute top-0 left-0 w-full h-full
                    transform ${isFocused ? 'scale-105 opacity-100' : 'scale-100 opacity-50'}
                    transition-all duration-300 rounded-md -z-10
                `}
                style={{
                    background: 'linear-gradient(90deg, color-mix(in srgb, var(--planetfall-primary) 15%, transparent) 0%, color-mix(in srgb, var(--planetfall-bg-dark) 80%, transparent) 100%)'
                }}
            ></div>

            <div className="flex items-center w-full relative">
                <input
                    ref={playerInputElement}
                    readOnly={isPending}
                    className={`
                        w-full
                        px-4 sm:px-5 py-1.5 sm:py-2
                        font-mono text-sm sm:text-base
                        rounded-md
                        shadow-md
                        focus:outline-none
                        transition-all duration-300
                    `}
                    style={{
                        background: 'var(--planetfall-bg-dark)',
                        color: 'var(--planetfall-text)',
                        borderWidth: '2px',
                        borderStyle: 'solid',
                        borderColor: isFocused ? 'color-mix(in srgb, var(--planetfall-accent) 60%, transparent)' : 'color-mix(in srgb, var(--planetfall-primary) 30%, transparent)',
                        boxShadow: isFocused ? '0 0 20px color-mix(in srgb, var(--planetfall-primary) 40%, transparent)' : 'none'
                    }}
                    value={playerInput}
                    placeholder="Type your command, then press enter/return..."
                    onChange={handleChange}
                    onKeyDown={handleKeyDown}
                    onFocus={() => setIsFocused(true)}
                    onBlur={() => setIsFocused(false)}
                    data-testid="game-input"
                />
                
            </div>

        </Box>
    );
};

export default GameInput;
