import React from 'react';
import '@fontsource/platypi/500.css';
import AboutMenu from "./AboutMenu.tsx";

const GameMenu: React.FC = () => (
    <div className="p-1 grid grid-cols-10 bg-gray-200 gap-2">
        <div className="col-span-7">
            <h1 className="text-2xl text-black m-3 font-poppins ">Zork AI Beta 0.1</h1>
        </div>
        <div className="col-span-3 flex justify-end mt-2"><AboutMenu/></div>
        
        {/*<button className="btn col-span-1">Save</button>*/}
        {/*<button className="btn col-span-1">Restore</button>*/}
        {/*<button className="btn col-span-1">Restart</button>*/}
    </div>
);

export default GameMenu;