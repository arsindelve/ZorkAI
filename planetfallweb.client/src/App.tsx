import './App.css';
import {useState} from 'react';
import {
    GameAppContent,
    ReleaseNotesServer,
    ReleaseNotesModal,
    PreferencesModal,
    RestartConfirmDialog,
    RestoreModal,
    SaveModal,
    SessionHandler,
    useGameContext,
    VideoDialog,
} from '@zork-ai/shared-types';
import Game from './Game';
import GameMenu from './menu/GameMenu';
import WelcomeDialog from './modal/WelcomeModal';
import Server from './Server';
import {TRANSCRIPT_BASE_FONT_SIZE_PX} from './transcriptFontSize';

export default function App() {
    const [server] = useState(() => new Server());
    const [clientId] = useState(() => new SessionHandler().getClientId());
    const dialogContext = useGameContext();
    return (
        <div
            className="relative bg-[url('https://planetfallai-assets.s3.amazonaws.com/Blue+Nebula+2+-+1024x1024.png')] bg-cover bg-center bg-fixed h-screen overflow-hidden flex flex-col"
            style={{backgroundSize: 'cover', backgroundAttachment: 'fixed'}}
        >
            <div className="absolute inset-0 bg-gradient-to-br from-black/60 to-black/40 pointer-events-none" />
            <div className="relative flex-grow flex flex-col min-h-0 z-10">
                <GameAppContent
                    server={server}
                    clientId={clientId}
                    dialogContext={dialogContext}
                    loadReleases={ReleaseNotesServer}
                    dialogs={{
                        Restart: RestartConfirmDialog,
                        Restore: RestoreModal,
                        Save: SaveModal,
                        Video: VideoDialog,
                        ReleaseNotes: ReleaseNotesModal,
                        Preferences: PreferencesModal,
                    }}
                    gameName="Planetfall AI"
                    baseFontSizePx={TRANSCRIPT_BASE_FONT_SIZE_PX}
                    specimenCommand="press the button"
                    specimenRoom="Reactor Lobby"
                    specimenBody="The door slides open with a soft hiss."
                    renderGame={(latestVersion) => (
                        <>
                            <GameMenu latestVersion={latestVersion} />
                            <Game />
                        </>
                    )}
                    renderWelcome={(open, close) => (
                        <WelcomeDialog open={open} handleClose={close} />
                    )}
                />
            </div>
            <footer className="hidden sm:block relative footer-gradient backdrop-blur-md border-t-2 py-2 z-10">
                <p className="text-center">
                    <a
                        target="_blank"
                        href="https://github.com/arsindelve/ZorkAI"
                        className="footer-text transition-colors duration-300"
                    >
                        Created By Mike in Dallas. Check out the repository.
                    </a>
                </p>
            </footer>
        </div>
    );
}
