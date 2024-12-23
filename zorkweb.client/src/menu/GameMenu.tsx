import '@fontsource/platypi/500.css';
import AboutMenu from "./AboutMenu.tsx";
import FunctionsMenu from "./FunctionsMenu.tsx";

interface GameMenuProps {
    gameMethods: (() => void)[]
    forceClose: boolean
}

export default function GameMenu({gameMethods, forceClose}: GameMenuProps) {
    return (
        <div className="p-1 grid grid-cols-10 bg-gray-200 gap-2 w-full fixed top-0 z-10">
            <div className="col-span-7 flex items-center">
                <img
                    src="https://zorkai-assets.s3.amazonaws.com/Zork.webp"
                    className="w-[100px] m-[3px]"
                    alt="Logo"
                />

                <h1 className="hidden lg:block text-xl text-black m-3 ml-20 font-['Lato']">Generative AI-Enhanced Zork
                    I</h1>


            </div>
            <div className="col-span-3 flex justify-end items-center">
                <div className="mr-15">
                    <FunctionsMenu forceClose={forceClose} gameMethods={gameMethods}/>
                </div>
                <AboutMenu/>
            </div>
        </div>

    );
}
