import '@fontsource/platypi/500.css';
import {FunctionsMenu, GameMenuShell} from '@zork-ai/shared-types';
import AboutMenu from './AboutMenu';

export default function GameMenu({latestVersion}: {latestVersion: string}) {
    return (
        <GameMenuShell
            className="gue-masthead flex items-center gap-4 px-3 sm:px-5 py-2"
            functionsMenu={<FunctionsMenu />}
            aboutMenu={<AboutMenu latestVersion={latestVersion} />}
            renderBrand={() => (
                <img
                    src="https://zorkai-assets.s3.amazonaws.com/Zork.webp"
                    className="gue-emblem w-[74px] sm:w-[92px] shrink-0 hover:scale-105 transition-transform duration-300"
                    alt="Logo"
                />
            )}
        />
    );
}
