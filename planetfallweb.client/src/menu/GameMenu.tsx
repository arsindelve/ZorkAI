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
                background: 'linear-gradient(90deg, var(--planetfall-bg-dark) 0%, var(--planetfall-bg-medium) 50%, var(--planetfall-bg-dark) 100%)',
                borderBottom: '2px solid color-mix(in srgb, var(--planetfall-primary) 40%, transparent)',
                boxShadow: '0 4px 20px color-mix(in srgb, var(--planetfall-primary) 20%, transparent)'
            }}
        >
            <div className="col-span-7 flex items-center">
                <h1
                    className={`hidden lg:block text-xl m-3 ml-10 font-medium tracking-wider transition-opacity duration-700 ${isLoaded ? 'opacity-100' : 'opacity-0'}`}
                    style={{
                        transitionDelay: '300ms',
                        fontFamily: 'Lato, sans-serif',
                        color: 'var(--planetfall-text)',
                        textShadow: '0 0 20px color-mix(in srgb, var(--planetfall-primary) 30%, transparent), 0 0 40px color-mix(in srgb, var(--planetfall-primary) 10%, transparent)'
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
