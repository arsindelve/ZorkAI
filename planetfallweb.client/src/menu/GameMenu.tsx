import '@fontsource/platypi/500.css';
import AboutMenu from "./AboutMenu.tsx";
import FunctionsMenu from "./FunctionsMenu.tsx";
import { useEffect, useState } from 'react';

export default function GameMenu() {
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        setIsLoaded(true);
    }, []);

    return (
        <div
            data-testid="game-menu-container"
            className={`p-2 grid grid-cols-10 gap-2 w-full fixed top-0 z-10 shadow-lg transition-transform duration-500 ${isLoaded ? 'translate-y-0' : '-translate-y-full'}`}
            style={{
                background: 'linear-gradient(90deg, #0a1628 0%, #0f2940 50%, #0a1628 100%)',
                borderBottom: '2px solid rgba(0, 217, 255, 0.3)',
                boxShadow: '0 4px 20px rgba(0, 217, 255, 0.15)'
            }}
        >
            <div className="col-span-7 flex items-center">
                <img
                    src="https://planetfallai-assets.s3.amazonaws.com/Planetfall.webp"
                    className="w-[100px] m-[3px] hover:scale-105 transition-all duration-300 hover:rotate-3"
                    alt="Planetfall Logo"
                />

                <h1
                    className={`hidden lg:block text-xl m-3 ml-10 font-medium tracking-wider transition-opacity duration-700 ${isLoaded ? 'opacity-100' : 'opacity-0'}`}
                    style={{
                        transitionDelay: '300ms',
                        fontFamily: 'Lato, sans-serif',
                        color: '#e0f2fe',
                        textShadow: '0 0 20px rgba(0, 217, 255, 0.3), 0 0 40px rgba(0, 217, 255, 0.1)'
                    }}
                >
                    Generative AI-Enhanced Planetfall
                </h1>
            </div>
            <div className="col-span-3 flex justify-end items-center space-x-2">
                <div>
                    <FunctionsMenu />
                </div>
                <AboutMenu/>
            </div>
        </div>
    );
}
