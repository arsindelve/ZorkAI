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
                    bg-gradient-to-r from-lime-900/30 to-stone-700 
                    transform ${isFocused ? 'scale-105 opacity-100' : 'scale-100 opacity-50'} 
                    transition-all duration-300 rounded-md -z-10
                `}
            ></div>

            <div className="flex items-center w-full relative">
                <input
                    ref={playerInputElement}
                    readOnly={isPending}
                    className={`
                        w-full
                        px-4 sm:px-5 py-1.5 sm:py-2
                        font-mono text-sm sm:text-base
                        text-lime-100
                        bg-stone-800
                        border-2 ${isFocused ? 'border-lime-600/70' : 'border-stone-600'}
                        rounded-md
                        shadow-md ${isFocused ? 'shadow-lime-900/30' : ''}
                        focus:outline-none
                        placeholder-stone-500
                        transition-all duration-300
                    `}
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
