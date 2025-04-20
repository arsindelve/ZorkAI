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
            className={`p-2 grid grid-cols-10 bg-gradient-to-r from-gray-800 to-gray-900 gap-2 w-full fixed top-0 z-10 shadow-lg transition-transform duration-500 ${isLoaded ? 'translate-y-0' : '-translate-y-full'}`}
        >
            <div className="col-span-7 flex items-center">
                <img
                    src="https://zorkai-assets.s3.amazonaws.com/Zork.webp"
                    className="w-[100px] m-[3px] hover:scale-105 transition-all duration-300 hover:rotate-3"
                    alt="Logo"
                />

                <h1 
                    className={`hidden lg:block text-xl text-white m-3 ml-10 font-['Lato'] tracking-wider transition-opacity duration-700 ${isLoaded ? 'opacity-100' : 'opacity-0'}`}
                    style={{ transitionDelay: '300ms' }}
                >
                    Generative AI-Enhanced Zork I
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
