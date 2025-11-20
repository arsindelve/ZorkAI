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
    rounded-lg border-b platypi"
    style={{
        background: 'linear-gradient(135deg, #2a1f4a 0%, #1a0f2e 100%)',
        borderColor: 'rgba(123, 47, 255, 0.3)',
        boxShadow: '0 4px 20px rgba(123, 47, 255, 0.2)'
    }}>
        <div
            className="flex items-center gap-2 text-xl font-semibold flex-grow max-w-[60%] truncate group"
            data-testid="header-location"
        >
            <ExploreIcon className="group-hover:rotate-12 transition-transform duration-300" style={{color: '#ff6b9d'}}/>
            <span className="text-white">
                {locationName}
            </span>
        </div>
        <div className="flex gap-4">
            <div
                className="hidden sm:flex items-center px-3 py-2 rounded-lg border transition-colors duration-200"
                data-testid="header-moves"
                style={{
                    background: 'rgba(26, 15, 46, 0.6)',
                    borderColor: 'rgba(123, 47, 255, 0.3)'
                }}
            >
                <DirectionsRunIcon className="mr-2 text-sm" fontSize="small" style={{color: '#d4a5ff'}}/>
                <span className="mr-2 text-sm" style={{color: '#c4b5fe'}}>Moves: </span>
                <span className="font-medium" style={{color: '#ff6b9d'}}>{moves}</span>
            </div>
            <div
                className="hidden sm:flex items-center px-3 py-2 rounded-lg border transition-colors duration-200"
                data-testid="header-score"
                style={{
                    background: 'rgba(26, 15, 46, 0.6)',
                    borderColor: 'rgba(123, 47, 255, 0.3)'
                }}
            >
                <ScoreboardIcon className="mr-2 text-sm" fontSize="small" style={{color: '#d4a5ff'}}/>
                <span className="mr-2 text-sm" style={{color: '#c4b5fe'}}>Score: </span>
                <span className="font-medium" style={{color: '#ff6b9d'}}>{score}</span>
            </div>
        </div>
    </div>
);

export default HeaderComponent;
