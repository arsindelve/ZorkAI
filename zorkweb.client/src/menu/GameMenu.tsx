import '@fontsource/platypi/500.css';
import AboutMenu from "./AboutMenu.tsx";
import FunctionsMenu from "./FunctionsMenu.tsx";
import config from '../../config.json';

interface GameMenuProps {
    gameMethods: (() => void)[]
    forceClose: boolean
}

export default function GameMenu({gameMethods, forceClose}: GameMenuProps) {
    return (
        <div className="p-1 grid grid-cols-10 bg-gray-200 gap-2">
            <div className="col-span-7">
                <h1 className="text-2xl text-black m-3 font-poppins ">New Zork AI {config.version}</h1>
            </div>
            <div className="col-span-3 flex justify-end mt-2">
                <div className="mr-10"><FunctionsMenu forceClose={forceClose} gameMethods={gameMethods}/></div>
                <AboutMenu/>
            </div>
        </div>
    );
}
