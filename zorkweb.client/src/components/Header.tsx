import React from 'react';
import ExploreIcon from '@mui/icons-material/Explore';
import ScoreboardIcon from '@mui/icons-material/Scoreboard';
import DirectionsRunIcon from '@mui/icons-material/DirectionsRun';
import CountUp from './CountUp';

interface HeaderComponentProps {
    locationName: string;
    moves: string;
    score: string;
}

const statChip: React.CSSProperties = {
    background: 'rgba(12, 10, 9, 0.55)',
    border: '1px solid rgba(196, 154, 76, 0.25)'
};

const HeaderComponent: React.FC<HeaderComponentProps> = ({locationName, moves, score}) => (
    <div
        className="hidden sm:flex items-center justify-between mt-2 px-4 py-2.5 rounded-xl platypi"
        style={{
            background: 'linear-gradient(135deg, #2b2723 0%, #161310 100%)',
            border: '1px solid rgba(196, 154, 76, 0.35)',
            boxShadow: '0 6px 22px rgba(196, 154, 76, 0.16), inset 0 1px 0 rgba(255, 255, 255, 0.05)'
        }}
    >
        <div
            className="flex items-center gap-3 flex-grow max-w-[60%] truncate group"
            data-testid="header-location"
        >
            <ExploreIcon
                className="group-hover:rotate-12 transition-transform duration-300"
                style={{color: '#c49a4c', fontSize: '1.7rem'}}
            />
            <span
                className="text-white text-2xl font-bold tracking-wide truncate"
                style={{textShadow: '0 0 14px rgba(196, 154, 76, 0.3)'}}
            >
                {locationName}
            </span>
        </div>
        <div className="flex gap-3">
            <div
                className="hidden sm:flex items-center px-3 py-1.5 rounded-lg transition-colors duration-200 hover:border-[#c49a4c]"
                data-testid="header-moves"
                style={statChip}
            >
                <DirectionsRunIcon className="mr-2" style={{color: '#c49a4c'}} fontSize="small"/>
                <span className="mr-2 uppercase tracking-widest text-stone-400" style={{fontSize: '0.65rem'}}>Moves</span>
                <CountUp value={moves} className="font-bold text-lg" style={{color: '#e3c179', textShadow: '0 0 10px rgba(196, 154, 76, 0.45)'}}/>
            </div>
            <div
                className="hidden sm:flex items-center px-3 py-1.5 rounded-lg transition-colors duration-200 hover:border-[#c49a4c]"
                data-testid="header-score"
                style={statChip}
            >
                <ScoreboardIcon className="mr-2" style={{color: '#c49a4c'}} fontSize="small"/>
                <span className="mr-2 uppercase tracking-widest text-stone-400" style={{fontSize: '0.65rem'}}>Score</span>
                <CountUp value={score} className="font-bold text-lg" style={{color: '#e3c179', textShadow: '0 0 10px rgba(196, 154, 76, 0.45)'}}/>
            </div>
        </div>
    </div>
);

export default HeaderComponent;
