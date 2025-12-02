import React from 'react';
import { Explore as ExploreIcon, Scoreboard as ScoreboardIcon, DirectionsRun as DirectionsRunIcon } from '@mui/icons-material';

interface HeaderComponentProps {
    locationName: string;
    moves: string;
    score: string;
}

const HeaderComponent: React.FC<HeaderComponentProps> = ({locationName, moves, score}) => (
    <div className="hidden sm:flex items-center
    mt-5
    justify-between
    bg-gradient-to-r from-stone-800 to-stone-700 p-2
    text-white shadow-lg
    rounded-lg border-b border-stone-600/30 platypi">
        <div
            className="flex items-center gap-2 text-xl font-semibold flex-grow max-w-[60%] truncate group"
            data-testid="header-location"
        >
            <ExploreIcon className="text-lime-600 group-hover:rotate-12 transition-transform duration-300"/>
            <span className="text-white">
                {locationName}
            </span>
        </div>
        <div className="flex gap-4">
            <div
                className="hidden sm:flex items-center px-3 py-2 bg-stone-900/50 rounded-lg border border-stone-700 hover:border-lime-700 transition-colors duration-200"
                data-testid="header-moves"
            >
                <DirectionsRunIcon className="text-stone-400 mr-2 text-sm" fontSize="small"/>
                <span className="text-stone-300 mr-2 text-sm">Moves: </span>
                <span className="font-medium text-lime-600">{moves}</span>
            </div>
            <div
                className="hidden sm:flex items-center px-3 py-2 bg-stone-900/50 rounded-lg border border-stone-700 hover:border-lime-700 transition-colors duration-200"
                data-testid="header-score"
            >
                <ScoreboardIcon className="text-stone-400 mr-2 text-sm" fontSize="small"/>
                <span className="text-stone-300 mr-2 text-sm">Score: </span>
                <span className="font-medium text-lime-600">{score}</span>
            </div>
        </div>
    </div>
);

export default HeaderComponent;
