import React from 'react';

interface HeaderComponentProps {
    locationName: string;
    moves: string;
    score: string;
}

const HeaderComponent: React.FC<HeaderComponentProps> = ({locationName, moves, score}) => (
    <div className="grid items-center bg-sky-950 p-3 text-white grid-cols-5 bebas-neue-regular text-2xl ">
        <div className="col-span-2">{locationName}</div>
        <div className="col-span-2">Moves: {moves}</div>
        <div>Score: {score}</div>
    </div>
);

export default HeaderComponent;