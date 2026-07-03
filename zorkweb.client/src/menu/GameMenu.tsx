import '@fontsource/platypi/500.css';
import AboutMenu from "./AboutMenu.tsx";
import {FunctionsMenu} from "@zork-ai/shared-types";
import { useEffect, useState } from 'react';

export default function GameMenu({ latestVersion }: { latestVersion: string }) {
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        setIsLoaded(true);
    }, []);

    return (
        <div
            data-testid="game-menu-container"
            className={`gue-masthead flex items-center gap-4 px-3 sm:px-5 py-2 w-full fixed top-0 z-10 transition-transform duration-500 ${isLoaded ? 'translate-y-0' : '-translate-y-full'}`}
        >
            <div className="flex items-center flex-grow min-w-0">
                <img
                    src="https://zorkai-assets.s3.amazonaws.com/Zork.webp"
                    className="gue-emblem w-[74px] sm:w-[92px] shrink-0 hover:scale-105 transition-transform duration-300"
                    alt="Logo"
                />
            </div>
            <div className="flex justify-end items-center gap-2 shrink-0">
                <FunctionsMenu />
                <AboutMenu latestVersion={latestVersion} />
            </div>
        </div>
    );
}
