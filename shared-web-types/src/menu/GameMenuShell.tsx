import React, {useEffect, useState} from 'react';

export interface GameMenuShellProps {
    aboutMenu: React.ReactNode;
    functionsMenu: React.ReactNode;
    renderBrand: (loaded: boolean) => React.ReactNode;
    className?: string;
    style?: React.CSSProperties;
    brandClassName?: string;
    actionsClassName?: string;
}

export default function GameMenuShell({
    aboutMenu,
    functionsMenu,
    renderBrand,
    className = '',
    style,
    brandClassName = 'flex items-center flex-grow min-w-0',
    actionsClassName = 'flex justify-end items-center gap-2 shrink-0',
}: GameMenuShellProps) {
    const [loaded, setLoaded] = useState(false);
    useEffect(() => setLoaded(true), []);

    return (
        <div
            data-testid="game-menu-container"
            className={`${className} w-full fixed top-0 z-10 transition-transform duration-500 ${loaded ? 'translate-y-0' : '-translate-y-full'}`}
            style={style}
        >
            <div className={brandClassName}>{renderBrand(loaded)}</div>
            <div className={actionsClassName}>
                {functionsMenu}
                {aboutMenu}
            </div>
        </div>
    );
}
