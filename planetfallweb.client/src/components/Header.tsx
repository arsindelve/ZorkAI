import React from 'react';
import ExploreIcon from '@mui/icons-material/Explore';
import ScoreboardIcon from '@mui/icons-material/Scoreboard';
import DirectionsRunIcon from '@mui/icons-material/DirectionsRun';

interface HeaderComponentProps {
    locationName: string;
    moves: string;
    score: string;
}

const HeaderComponent: React.FC<HeaderComponentProps> = ({locationName, moves, score}) => (
    <div className="hidden sm:flex items-center
    mt-5
    justify-between
    p-2
    text-white shadow-lg
    rounded-lg border-b border-cyan-500/30 platypi"
    style={{
        background: 'linear-gradient(135deg, #0f1f35 0%, #0a1628 100%)',
        boxShadow: '0 4px 20px rgba(0, 217, 255, 0.15)'
    }}>
        <div
            className="flex items-center gap-2 text-xl font-semibold flex-grow max-w-[60%] truncate group"
            data-testid="header-location"
        >
            <ExploreIcon className="text-cyan-400 group-hover:rotate-12 transition-transform duration-300"/>
            <span className="text-white">
                {locationName}
            </span>
        </div>
        <div className="flex gap-4">
            <div
                className="hidden sm:flex items-center px-3 py-2 rounded-lg border border-cyan-500/30 hover:border-cyan-400 transition-colors duration-200"
                data-testid="header-moves"
                style={{background: 'rgba(10, 22, 40, 0.5)'}}
            >
                <DirectionsRunIcon className="text-cyan-300 mr-2 text-sm" fontSize="small"/>
                <span className="text-cyan-200 mr-2 text-sm">Moves: </span>
                <span className="font-medium text-cyan-400">{moves}</span>
            </div>
            <div
                className="hidden sm:flex items-center px-3 py-2 rounded-lg border border-cyan-500/30 hover:border-cyan-400 transition-colors duration-200"
                data-testid="header-score"
                style={{background: 'rgba(10, 22, 40, 0.5)'}}
            >
                <ScoreboardIcon className="text-cyan-300 mr-2 text-sm" fontSize="small"/>
                <span className="text-cyan-200 mr-2 text-sm">Score: </span>
                <span className="font-medium text-cyan-400">{score}</span>
            </div>
        </div>
    </div>
);

export default HeaderComponent;
