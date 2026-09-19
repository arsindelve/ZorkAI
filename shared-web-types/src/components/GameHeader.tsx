import React from 'react';
import ExploreIcon from '@mui/icons-material/Explore';
import ScoreboardIcon from '@mui/icons-material/Scoreboard';
import CountUp from './CountUp';

export interface HeaderStat {
    label: string;
    value: string;
    icon: React.ReactNode;
    testId: string;
}

export interface GameHeaderProps {
    locationName: string;
    primaryStat: HeaderStat;
    score: string;
}

function StatChip({stat}: {stat: HeaderStat}) {
    return (
        <div
            className="hidden sm:flex flex-1 items-center justify-center px-3 py-2 rounded-lg border transition-colors duration-200"
            data-testid={stat.testId}
            style={{
                background: 'var(--game-header-chip-bg, rgba(12, 10, 9, .55))',
                borderColor: 'var(--game-header-border, rgba(196, 154, 76, .25))',
            }}
        >
            <span className="mr-2 flex" style={{color: 'var(--game-header-accent, #c49a4c)'}}>
                {stat.icon}
            </span>
            <span
                className="mr-2 text-xs uppercase tracking-widest"
                style={{color: 'var(--game-header-muted, #a8a29e)'}}
            >
                {stat.label}:{' '}
            </span>
            <CountUp
                value={stat.value}
                className="font-bold text-base"
                style={{color: 'var(--game-header-value, #e3c179)'}}
            />
        </div>
    );
}

export default function GameHeader({locationName, primaryStat, score}: GameHeaderProps) {
    return (
        <div
            className="hidden sm:flex items-center justify-between mt-2 px-4 py-2.5 rounded-xl platypi"
            style={{
                background: 'var(--game-header-bg, linear-gradient(135deg, #2b2723, #161310))',
                border: '1px solid var(--game-header-border, rgba(196, 154, 76, .35))',
                boxShadow: 'var(--game-header-shadow, 0 6px 22px rgba(196, 154, 76, .16))',
            }}
        >
            <div
                className="flex items-center gap-3 flex-grow max-w-[60%] truncate group"
                data-testid="header-location"
            >
                <ExploreIcon
                    className="group-hover:rotate-12 transition-transform duration-300"
                    style={{color: 'var(--game-header-accent, #c49a4c)', fontSize: '1.7rem'}}
                />
                <span className="text-white text-base font-bold tracking-wide truncate">
                    {locationName}
                </span>
            </div>
            <div className="flex gap-3 w-[242px] lg:w-[274px] mr-1">
                <StatChip stat={primaryStat} />
                <StatChip
                    stat={{
                        label: 'Score',
                        value: score,
                        icon: <ScoreboardIcon fontSize="small" />,
                        testId: 'header-score',
                    }}
                />
            </div>
        </div>
    );
}
