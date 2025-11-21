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
        background: 'linear-gradient(135deg, var(--planetfall-bg-medium) 0%, var(--planetfall-bg-dark) 100%)',
        borderColor: 'color-mix(in srgb, var(--planetfall-primary) 30%, transparent)',
        boxShadow: '0 4px 20px color-mix(in srgb, var(--planetfall-primary) 20%, transparent)'
    }}>
        <div
            className="flex items-center gap-2 text-xl font-semibold flex-grow max-w-[60%] truncate group"
            data-testid="header-location"
        >
            <ExploreIcon className="group-hover:rotate-12 transition-transform duration-300" style={{color: 'var(--planetfall-accent)'}}/>
            <span className="text-white">
                {locationName}
            </span>
        </div>
        <div className="flex gap-4">
            <div
                className="hidden sm:flex items-center px-3 py-2 rounded-lg border transition-colors duration-200"
                data-testid="header-moves"
                style={{
                    background: 'color-mix(in srgb, var(--planetfall-bg-dark) 60%, transparent)',
                    borderColor: 'color-mix(in srgb, var(--planetfall-primary) 30%, transparent)'
                }}
            >
                <DirectionsRunIcon className="mr-2 text-sm" fontSize="small" style={{color: 'var(--planetfall-accent)'}}/>
                <span className="mr-2 text-sm" style={{color: 'var(--planetfall-text)'}}>Moves: </span>
                <span className="font-medium" style={{color: 'var(--planetfall-primary)'}}>{moves}</span>
            </div>
            <div
                className="hidden sm:flex items-center px-3 py-2 rounded-lg border transition-colors duration-200"
                data-testid="header-score"
                style={{
                    background: 'color-mix(in srgb, var(--planetfall-bg-dark) 60%, transparent)',
                    borderColor: 'color-mix(in srgb, var(--planetfall-primary) 30%, transparent)'
                }}
            >
                <ScoreboardIcon className="mr-2 text-sm" fontSize="small" style={{color: 'var(--planetfall-accent)'}}/>
                <span className="mr-2 text-sm" style={{color: 'var(--planetfall-text)'}}>Score: </span>
                <span className="font-medium" style={{color: 'var(--planetfall-primary)'}}>{score}</span>
            </div>
        </div>
    </div>
);

export default HeaderComponent;
