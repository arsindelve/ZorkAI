import React from 'react';

interface HeaderComponentProps {
    locationName: string;
    moves: string;
    score: string;
}

const HeaderComponent: React.FC<HeaderComponentProps> = ({locationName, moves, score}) => (
    <div className="flex items-center justify-between bg-gradient-to-r from-stone-800 to-stone-700 p-4 text-white shadow-md platypi">
        <div 
            className="text-xl font-semibold flex-grow max-w-[60%] truncate" 
            data-testid="header-location"
        >
            {locationName}
        </div>
        <div className="flex gap-6">
            <div 
                className="hidden sm:flex items-center px-3 py-1.5 bg-stone-900/30 rounded-lg" 
                data-testid="header-moves"
            >
                <span className="text-stone-300 mr-2">Moves:</span>
                <span className="font-medium">{moves}</span>
            </div>
            <div 
                className="hidden sm:flex items-center px-3 py-1.5 bg-stone-900/30 rounded-lg" 
                data-testid="header-score"
            >
                <span className="text-stone-300 mr-2">Score:</span>
                <span className="font-medium">{score}</span>
            </div>
        </div>
    </div>
);

export default HeaderComponent;
