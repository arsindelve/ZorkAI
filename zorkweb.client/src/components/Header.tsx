import React from 'react';

interface HeaderComponentProps {
    locationName: string;
    moves: string;
    score: string;
}

const HeaderComponent: React.FC<HeaderComponentProps> = ({locationName, moves, score}) => (
    <div className="grid items-center bg-stone-700 p-3 text-white grid-cols-5 platypi ">
        <div className="col-span-3" data-testid="header-location">{locationName}</div>
        <div className="hidden sm:block" data-testid="header-moves">Moves:&nbsp;&nbsp;{moves}</div>
        <div className="hidden sm:block" data-testid="header-score">Score:&nbsp;&nbsp;{score}</div>
    </div>
);

export default HeaderComponent;
