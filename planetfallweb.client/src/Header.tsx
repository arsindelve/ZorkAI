import React from 'react';

interface HeaderComponentProps {
    locationName: string;
    moves: string;
    score: string;
}

const HeaderComponent: React.FC<HeaderComponentProps> = ({locationName, time, score}) => (
    <div className="flex justify-between items-center bg-sky-950 p-3 text-white bebas-neue-regular text-xl">
        <div className="text-left">{locationName}</div>
        <div className="text-center">Time: {time}</div>
        <div className="text-right">Score: {score}</div>
    </div>
);

export default HeaderComponent;