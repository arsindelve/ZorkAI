import '@fontsource/platypi/500.css';
import {FunctionsMenu, GameMenuShell} from '@zork-ai/shared-types';
import AboutMenu from './AboutMenu';

export default function GameMenu({latestVersion}: {latestVersion: string}) {
    return (
        <GameMenuShell
            className="p-2 grid grid-cols-10 gap-2 shadow-lg"
            functionsMenu={<FunctionsMenu />}
            brandClassName="col-span-7 flex items-center"
            actionsClassName="col-span-3 flex justify-end items-center space-x-2"
            style={{
                background:
                    'linear-gradient(90deg, var(--planetfall-bg-dark), var(--planetfall-bg-medium), var(--planetfall-bg-dark))',
                borderBottom:
                    '2px solid color-mix(in srgb, var(--planetfall-primary) 40%, transparent)',
                boxShadow:
                    '0 4px 20px color-mix(in srgb, var(--planetfall-primary) 20%, transparent)',
            }}
            aboutMenu={<AboutMenu latestVersion={latestVersion} />}
            renderBrand={(loaded) => (
                <img
                    src="/logo.png"
                    alt="Planetfall"
                    className={`hidden lg:block m-3 ml-10 transition-opacity duration-700 ${loaded ? 'opacity-100' : 'opacity-0'}`}
                    style={{transitionDelay: '300ms', width: '240px', height: 'auto'}}
                />
            )}
        />
    );
}
