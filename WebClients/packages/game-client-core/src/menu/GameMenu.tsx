import '@fontsource/platypi/500.css';
import { useEffect, useState, ReactNode } from 'react';

interface GameMenuProps {
    logoUrl: string;
    logoAlt: string;
    title: string;
    children: ReactNode;
}

export default function GameMenu({ logoUrl, logoAlt, title, children }: GameMenuProps) {
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        setIsLoaded(true);
    }, []);

    return (
        <div
            data-testid="game-menu-container"
            className={`p-2 grid grid-cols-10 bg-gradient-to-r from-gray-800 to-gray-900 gap-2 w-full fixed top-0 z-10 shadow-lg transition-transform duration-500 ${isLoaded ? 'translate-y-0' : '-translate-y-full'}`}
        >
            <div className="col-span-7 flex items-center">
                <img
                    src={logoUrl}
                    className="w-[100px] m-[3px] hover:scale-105 transition-all duration-300 hover:rotate-3"
                    alt={logoAlt}
                />

                <h1
                    className={`hidden lg:block text-xl text-white m-3 ml-10 font-['Lato'] tracking-wider transition-opacity duration-700 ${isLoaded ? 'opacity-100' : 'opacity-0'}`}
                    style={{ transitionDelay: '300ms' }}
                >
                    {title}
                </h1>
            </div>
            <div className="col-span-3 flex justify-end items-center space-x-2">
                {children}
            </div>
        </div>
    );
}
