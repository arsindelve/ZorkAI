import React, { ChangeEvent, KeyboardEvent, RefObject, useState, useEffect } from 'react';
import { Box } from '@mui/material';
import KeyboardIcon from '@mui/icons-material/Keyboard';
import TextFieldsIcon from '@mui/icons-material/TextFields';

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
    const [cursorBlink, setCursorBlink] = useState(true);

    // Cursor blinking effect
    useEffect(() => {
        if (isFocused) {
            const interval = setInterval(() => {
                setCursorBlink(prev => !prev);
            }, 530);
            return () => clearInterval(interval);
        }
        return undefined;
    }, [isFocused]);

    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        setInput(e.target.value);
    };

    return (
        <Box className="relative w-full sm:w-3/4 md:w-2/3 lg:max-w-xl px-2 py-1 sm:py-2 flex-shrink">
            <div 
                className={`
                    absolute top-0 left-0 w-full h-full 
                    bg-gradient-to-r from-emerald-900/30 to-stone-700 
                    transform ${isFocused ? 'scale-105 opacity-100' : 'scale-100 opacity-50'} 
                    transition-all duration-300 rounded-md -z-10
                `}
            ></div>

            <div className="flex items-center w-full relative">
                <div className="absolute left-4 text-emerald-400 opacity-70">
                    {isFocused ? <TextFieldsIcon /> : <KeyboardIcon />}
                </div>
                
                <input
                    ref={playerInputElement}
                    readOnly={isPending}
                    className={`
                        w-full
                        px-8 sm:px-10 py-1.5 sm:py-2
                        font-mono text-sm sm:text-base
                        text-lime-100
                        bg-stone-800
                        border-2 ${isFocused ? 'border-emerald-400/70' : 'border-stone-600'}
                        rounded-md
                        shadow-md ${isFocused ? 'shadow-emerald-900/30' : ''}
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
                
                {isFocused && (
                    <span 
                        className={`
                            absolute right-4
                            h-4/5 w-0.5
                            ${cursorBlink ? 'bg-emerald-400' : 'bg-transparent'}
                            transition-colors duration-100
                        `}
                    ></span>
                )}
            </div>
            
            <div className="absolute bottom-0 left-0 w-full h-1 overflow-hidden">
                <div 
                    className={`
                        h-full bg-emerald-400/40
                        transform ${isPending ? 'translate-x-0' : '-translate-x-full'}
                        transition-transform duration-300 ease-in-out
                        ${isPending ? 'animate-pulse' : ''}
                    `}
                ></div>
            </div>
        </Box>
    );
};

export default GameInput;
